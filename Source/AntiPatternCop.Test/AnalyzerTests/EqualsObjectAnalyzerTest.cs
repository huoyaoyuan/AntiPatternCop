using System.Threading.Tasks;
using AntiPatternCop.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = AntiPatternCop.Test.CSharpAnalyzerVerifier<
    AntiPatternCop.Analyzers.CSharpEqualsObjectAnalyzer>;

namespace AntiPatternCop.Test.AnalyzerTests
{
    [TestClass]
    public class EqualsObjectAnalyzerTest
    {
        [TestMethod]
        public async Task VerifySimple()
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
    }
}
