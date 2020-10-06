Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Operations

Namespace Analyzers
    <DiagnosticAnalyzer(LanguageNames.VisualBasic)>
    Public Class VBEqualsObjectAnalyzer
        Inherits AbstractEqualsObjectAnalyzer

        Protected Overrides Function GetMethodName(invocation As IInvocationOperation) As SyntaxNode
            Return invocation.Syntax
        End Function
    End Class
End Namespace
