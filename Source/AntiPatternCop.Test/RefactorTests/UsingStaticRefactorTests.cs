using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VerifyCS = AntiPatternCop.Test.CSharpCodeRefactoringVerifier<
    AntiPatternCop.Refactors.CSharpUsingStaticRefactorProvider>;

namespace AntiPatternCop.Test.RefactorTests
{
    [TestClass]
    public class UsingStaticRefactorTests
    {
        [TestMethod]
        public async Task Test1()
        {
            string initial = @"
using System;

class C
{
    void M()
    {
        [|Console|].WriteLine(""abc"");
    }
}
";
            string expected = @"
using System;
using static System.Console;

class C
{
    void M()
    {
        WriteLine(""abc"");
    }
}
";
            await VerifyCS.VerifyRefactoringAsync(initial, expected);
        }

        [TestMethod]
        public async Task Test2()
        {
            string initial =
@"class C
{
    void M()
    {
        System.Console.[|WriteLine|](""abc"");
    }
}
";
            string expected =
@"using static System.Console;

class C
{
    void M()
    {
        WriteLine(""abc"");
    }
}
";
            await VerifyCS.VerifyRefactoringAsync(initial, expected);
        }

        [TestMethod]
        public async Task Test3()
        {
            string initial =
@"class A
{
    public class B
    {
    }
}

class C
{
    void M()
    {
        A.[|B|] obj;
    }
}
";
            string expected =
@"using static A;

class A
{
    public class B
    {
    }
}

class C
{
    void M()
    {
        B obj;
    }
}
";
            await VerifyCS.VerifyRefactoringAsync(initial, expected);
        }
    }
}
