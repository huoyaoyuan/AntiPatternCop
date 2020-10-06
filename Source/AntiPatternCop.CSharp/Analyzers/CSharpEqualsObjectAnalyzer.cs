using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace AntiPatternCop.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CSharpEqualsObjectAnalyzer : AbstractEqualsObjectAnalyzer
    {
        protected override SyntaxNode GetMethodName(IInvocationOperation invocation)
        {
            if (invocation.Syntax is not InvocationExpressionSyntax syntax)
                return invocation.Syntax;

            return syntax.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                var other => other
            };
        }
    }
}
