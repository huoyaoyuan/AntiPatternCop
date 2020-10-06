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

        [TestMethod]
        public async Task VerifyOverrideCSharp()
        {
            string source = @"
class C
{
    bool M(string o)
    {
        return o.Equals(1);
    }
}";
            var expected = VerifyCS.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(6, 18);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task VeryfiOverrideVB()
        {
            string source = @"
Class C
    Function M(o As String) As Boolean
        Return o.Equals(1)
    End Function
End Class";
            var expected = VerifyVB.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(4, 18);
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task VerifyStaticCSharp()
        {
            string source = @"
class C
{
    bool M(string o)
    {
        return object.Equals(o, 1);
    }
}";
            var expected = VerifyCS.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(6, 23);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task VerifyStaticImplicitCSharp()
        {
            string source = @"
class C
{
    bool M(string o)
    {
        return Equals(o, 1);
    }
}";
            var expected = VerifyCS.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(6, 16);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [TestMethod]
        public async Task VerifyNotOnStrongTyle()
        {
            string source = @"
class C
{
    bool M(string o)
    {
        return o.Equals("""");
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [TestMethod]
        public async Task VerifyNotOnIEquatable()
        {
            string source = @"
using System;

class C
{
    bool M<T>(T a, T b) where T : IEquatable<T>
    {
        return a.Equals(b);
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(source);
        }
    }
}
