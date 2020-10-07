using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using AntiPatternCop.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Operations;

namespace AntiPatternCop.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class EqualsObjectCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(AbstractEqualsObjectAnalyzer.MessageId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var cancellation = context.CancellationToken;
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);

            var node = syntaxRoot.FindNode(context.Diagnostics[0].AdditionalLocations[0].SourceSpan);
            if (node is null)
                return;

            var semanticModel = await context.Document.GetSemanticModelAsync(cancellation).ConfigureAwait(false);

            var operation = (IInvocationOperation)semanticModel.GetOperation(node);
            IOperation left, right;
            if (operation.Instance is null)
            {
                left = operation.Arguments[0].Value;
                right = operation.Arguments[1].Value;
            }
            else
            {
                left = operation.Instance;
                right = operation.Arguments[0].Value;
            }

            left = RemoveImplicitConversion(left);
            right = RemoveImplicitConversion(right);

            if (SymbolEqualityComparer.Default.Equals(left.Type, right.Type))
            {
                var equalityComparer = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.EqualityComparer`1");
                if (equalityComparer is { IsGenericType: true })
                {
                    string title = "Use EqualityComparer<T>.Default";
                    var codeAction = CodeAction.Create(
                        title,
                        ct => UseEqualityComparerAsync(context.Document, node, left, right, equalityComparer, ct),
                        equivalenceKey: title);

                    context.RegisterCodeFix(codeAction, context.Diagnostics);
                }

                if (semanticModel.Compilation.GetTypeByMetadataName("System.IEquatable`1")
                    is { TypeKind: TypeKind.Interface, IsGenericType: true } iequatable
                    && left.Type is
                    {
                        TypeKind: TypeKind.TypeParameter,
                        DeclaringSyntaxReferences: { Length: 1 } declaration
                    } genericParameter)
                {
                    string title = "Add IEquatable<T> constraint";
                    var codeAction = CodeAction.Create(
                        title,
                        ct => AddIEquatableConstraintAsync(context.Document.Project.Solution, declaration[0], genericParameter, iequatable, ct),
                        equivalenceKey: title);

                    context.RegisterCodeFix(codeAction, context.Diagnostics);
                }
            }
        }

        private static IOperation RemoveImplicitConversion(IOperation operation)
        {
            if (operation is IConversionOperation { IsImplicit: true } conversion)
                return conversion.Operand;

            return operation;
        }

        private static async Task<Document> UseEqualityComparerAsync(
            Document document,
            SyntaxNode nodeToFix,
            IOperation left,
            IOperation right,
            INamedTypeSymbol equalityComparerType,
            CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            var generator = editor.Generator;

            var typeExpression = generator.TypeExpression(equalityComparerType.Construct(left.Type));
            var memberAccess = generator.MemberAccessExpression(
                typeExpression,
                generator.IdentifierName("Default"));
            var equals = generator.MemberAccessExpression(
                memberAccess,
                generator.IdentifierName(WellKnownMemberNames.ObjectEquals));
            var invoke = generator.InvocationExpression(
                equals,
                left.Syntax,
                right.Syntax);

            editor.ReplaceNode(nodeToFix, invoke);
            return await ImportAdder.AddImportsAsync(editor.GetChangedDocument(),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        private static async Task<Solution> AddIEquatableConstraintAsync(
            Solution solution,
            SyntaxReference genericParameterDeclaration,
            ITypeSymbol genericParameter,
            ITypeSymbol iequatable,
            CancellationToken cancellationToken)
        {
            var declarationSyntax = genericParameterDeclaration.GetSyntax(cancellationToken);
            var documentId = solution.GetDocumentId(declarationSyntax.SyntaxTree);
            var document = solution.GetDocument(documentId);

            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            var generator = editor.Generator;

            var oldDeclaration = generator.GetDeclaration(declarationSyntax);
            editor.SetTypeConstraint(oldDeclaration,
                genericParameter.Name,
                SpecialTypeConstraintKind.None,
                new[] { generator.TypeExpression(iequatable) });

            var newDocument = await ImportAdder.AddImportsAsync(
                editor.GetChangedDocument(),
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return solution.WithDocumentSyntaxRoot(documentId,
                await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
