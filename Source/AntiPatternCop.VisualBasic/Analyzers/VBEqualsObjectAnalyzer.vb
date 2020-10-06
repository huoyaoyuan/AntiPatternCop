Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class VBEqualsObjectAnalyzer
        Inherits AbstractEqualsObjectAnalyzer

        Protected Overrides Function GetMethodName(invocation As IInvocationOperation) As SyntaxNode
            Dim syntax = TryCast(invocation.Syntax, InvocationExpressionSyntax)
            If syntax Is Nothing Then Return invocation.Syntax

            Dim memberAccess = TryCast(syntax.Expression, MemberAccessExpressionSyntax)
            If memberAccess IsNot Nothing Then Return memberAccess.Name

            Return syntax
        End Function
    End Class
End Namespace
