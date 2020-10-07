using System.Threading.Tasks;
using AntiPatternCop.Analyzers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = AntiPatternCop.Test.CSharpCodeFixVerifier<
    AntiPatternCop.Analyzers.CSharpEqualsObjectAnalyzer,
    AntiPatternCop.CodeFixes.EqualsObjectCodeFixProvider>;
using VerifyVB = AntiPatternCop.Test.VisualBasicCodeFixVerifier<
    AntiPatternCop.Analyzers.VBEqualsObjectAnalyzer,
    AntiPatternCop.CodeFixes.EqualsObjectCodeFixProvider>;

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
                .WithLocation(6, 18).WithLocation(6, 16);
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
                .WithLocation(4, 18).WithLocation(4, 16);
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
                .WithLocation(6, 18).WithLocation(6, 16);
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
                .WithLocation(4, 18).WithLocation(4, 16);
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
                .WithLocation(6, 23).WithLocation(6, 16);
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
                .WithLocation(6, 16).WithLocation(6, 16);
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

        [TestMethod]
        public async Task VerifyUnconstrainedGenericCSharp()
        {
            string source = @"
using System;
using System.Collections.Generic;

class C
{
    bool M<T>(T a, T b)
    {
        return a.{|#0:Equals|}(b);
    }
}";
            string fix = @"
using System;
using System.Collections.Generic;

class C
{
    bool M<T>(T a, T b)
    {
        return EqualityComparer<T>.Default.Equals(a, b);
    }
}";
            var expected = VerifyCS.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(9, 18).WithLocation(9, 16);
            await VerifyCS.VerifyCodeFixAsync(source, expected, fix);
        }

        [TestMethod]
        public async Task VerifyUnconstrainedGenericVB()
        {
            string source = @"
Imports System
Imports System.Collections.Generic

Class C
    Function M(Of T)(a As T, B As T) As Boolean
        Return a.{|#0:Equals(b)|}
    End Function
End Class";
            string fix = @"
Imports System
Imports System.Collections.Generic

Class C
    Function M(Of T)(a As T, B As T) As Boolean
        Return EqualityComparer(Of T).Default.Equals(a, b)
    End Function
End Class";
            var expected = VerifyVB.Diagnostic(AbstractEqualsObjectAnalyzer.MessageId)
                .WithLocation(7, 18).WithLocation(7, 16);
            await VerifyVB.VerifyCodeFixAsync(source, expected, fix);
        }
    }
}
