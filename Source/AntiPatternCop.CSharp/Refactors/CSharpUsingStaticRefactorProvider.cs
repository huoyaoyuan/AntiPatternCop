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

            var node = syntaxRoot.FindNode(context.Span, true).FirstAncestorOrSelf<MemberAccessExpressionSyntax>();
            var symbolInfo = semanticModel.GetSymbolInfo(node, context.CancellationToken);
            var expressionSymbolInfo = semanticModel.GetSymbolInfo(node.Expression, context.CancellationToken);

            if (symbolInfo.Symbol is { IsStatic: true } && expressionSymbolInfo.Symbol is INamedTypeSymbol namedType)
            {
                context.RegisterRefactoring(CodeAction.Create(
                    "Use 'using static'",
                    async ct =>
                    {
                        var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
                        var generator = editor.Generator;
                        var usingStatic = SyntaxFactory.UsingDirective(SyntaxFactory.Token(SyntaxKind.StaticKeyword), null, (NameSyntax)generator.TypeExpression(namedType));

                        editor.ReplaceNode(node, node.WithAdditionalAnnotations(Simplifier.Annotation));

                        var changed = (CompilationUnitSyntax)editor.GetChangedRoot();
                        changed = changed.AddUsings(usingStatic);
                        return document.WithSyntaxRoot(changed);
                    }));
            }
        }
    }
}
