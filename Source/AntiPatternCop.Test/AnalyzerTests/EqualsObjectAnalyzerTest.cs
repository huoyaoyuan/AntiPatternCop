using System.Threading.Tasks;
using AntiPatternCop.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = AntiPatternCop.Test.CSharpAnalyzerVerifier<
    AntiPatternCop.Analyzers.CSharpEqualsObjectAnalyzer>;
using VerifyVB = AntiPatternCop.Test.VisualBasicAnalyzerVerifier<
    AntiPatternCop.Analyzers.VBEqualsObjectAnalyzer>;

namespace AntiPatternCop.Test.AnalyzerTests
{
    [TestClass]
    public class EqualsObjectAnalyzerTest
    {
        [TestMethod]
        public async Task VerifySimpleCSharp()
        {
            string source = @"
class C
{
    bool M(object o)
    {
        return o.Equals(1);
    }
}";
            var expected = VerifyCS.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(6, 18);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task VeryfiSimpleVB()
        {
            string source = @"
Class C
    Function M(o As Object) As Boolean
        Return o.Equals(1)
    End Function
End Class";
            var expected = VerifyVB.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(4, 18);
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
        }
    }
}
