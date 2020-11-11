using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;

namespace AntiPatternCop.Refactors
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp)]
    public class CSharpUsingStaticRefactorProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var syntaxRoot = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var node = syntaxRoot.FindNode(context.Span, true);

            var memberAccess = node.FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
            if (memberAccess != null)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(memberAccess, context.CancellationToken);
                var expressionSymbolInfo = semanticModel.GetSymbolInfo(memberAccess.Expression, context.CancellationToken);

                if (symbolInfo.Symbol is { IsStatic: true } && expressionSymbolInfo.Symbol is INamedTypeSymbol namedType)
                {
                    context.RegisterRefactoring(CodeAction.Create(
                        "Use 'using static'",
                        ct => AddUsingStaticAsync(document, memberAccess, namedType, ct)));

                    return;
                }
            }

            var qualifiedName = node.FirstAncestorOrSelf<QualifiedNameSyntax>();
            if (qualifiedName != null)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(qualifiedName, context.CancellationToken);
                var expressionSymbolInfo = semanticModel.GetSymbolInfo(qualifiedName.Left, context.CancellationToken);

                if (symbolInfo.Symbol is INamedTypeSymbol && expressionSymbolInfo.Symbol is INamedTypeSymbol namedType)
                {
                    context.RegisterRefactoring(CodeAction.Create(
                        "Use 'using static'",
                        ct => AddUsingStaticAsync(document, qualifiedName, namedType, ct)));

                    return;
                }
            }
        }

        private static async Task<Document> AddUsingStaticAsync(Document document, SyntaxNode targetNode, INamedTypeSymbol namedType, CancellationToken cancell)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancell).ConfigureAwait(false);
            var generator = editor.Generator;
            var usingStatic = SyntaxFactory.UsingDirective(SyntaxFactory.Token(SyntaxKind.StaticKeyword), null, (NameSyntax)generator.TypeExpression(namedType));

            editor.ReplaceNode(targetNode, targetNode.WithAdditionalAnnotations(Simplifier.Annotation));

            var changed = (CompilationUnitSyntax)editor.GetChangedRoot();
            changed = changed.AddUsings(usingStatic);
            return document.WithSyntaxRoot(changed);
        }
    }
}
