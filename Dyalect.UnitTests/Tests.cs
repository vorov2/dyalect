using Dyalect;
using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dyalect
{
    //[TestClass]
    //public class Tests
    //{
    //    [TestMethod] public void MathTest1() => ShouldBe(38);

    //    [TestMethod] public void MathTest2() => ShouldBe(1.5919117647058822);

    //    [TestMethod] public void MathTest3() => ShouldBe(6.6129032258064528);

    //    [TestMethod] public void MathTest4() => ShouldBe(11.256);

    //    [TestMethod] public void MathTest5() => ShouldBe(0.0099999999999997868);

    //    [TestMethod] public void MathTest6() => ShouldBe(783.59999999999991);

    //    [TestMethod] public void LogicalTest1() => ShouldBe(true);

    //    [TestMethod] public void LogicalTest2() => ShouldBe(false);

    //    [TestMethod] public void ComparisonTest1() => ShouldBe(true);

    //    [TestMethod] public void ComparisonTest2() => ShouldBe(true);

    //    [TestMethod] public void ToStringTest1() => ShouldBe("(1, 2, 3)");

    //    [TestMethod] public void ToStringTest2() => ShouldBe("[12.2, \"string\", true]");

    //    [TestMethod] public void ToStringTest3() => ShouldBe("(x: 42, y: \"foo\")");

    //    [TestMethod] public void WhileTest1() => ShouldBe(33);

    //    [TestMethod] public void WhileTest2() => ShouldBe(42);

    //    [TestMethod] public void FizzbuzzTest() => ShouldBe("12fizz4buzzfizz78fizzbuzz");

    //    [TestMethod] public void RecursionTest() => ShouldBe(10);

    //    [TestMethod] public void FactTest() => ShouldBe(2432902008176640000);

    //    [TestMethod] public void PowerTest() => ShouldBe(1000);

    //    [TestMethod] public void BinaryConversionTest() => ShouldBe("10011101");

    //    [TestMethod] public void FibTest1() => ShouldBe(89);

    //    [TestMethod] public void FibTest2() => ShouldBe("0 1 1 2 3 5 8 13 21 34 55");

    //    [TestMethod] public void PhoneParserTest() => ShouldBe("9645061112");

    //    [TestMethod] public void IteratorTest1() => ShouldBe(116.8);

    //    [TestMethod] public void IteratorTest2() => ShouldBe(36);

    //    [TestMethod] public void IteratorTest3() => ShouldBe(20);

    //    [TestMethod] public void IteratorTest4() => ShouldBe("olleH");

    //    [TestMethod] public void IteratorTest5() => ShouldBe("ll");

    //    [TestMethod] public void IteratorTest6() => ShouldBe(8);

    //    [TestMethod] public void CalcETest() => ShouldBe(2.7182818284590455);

    //    [TestMethod] public void DammTest() => ShouldBe("yesnoyesno");

    //    [TestMethod] public void GcdTest() => ShouldBe(12);

    //    [TestMethod] public void EulerTest() => ShouldBe("0=100;10=44;20=27.2;30=22.16;40=20.648;50=20.1944;60=20.05832;70=20.017496;80=20.0052488;90=20.00157464;100=20.000472392;");

    //    [TestMethod] public void AckTest() => ShouldBe(125);

    //    [TestMethod] public void FoldTest() => ShouldBe(21);

    //    #region Execution
    //    private void ShouldBe(int expected, [CallerMemberName]string callerName = "") =>
    //        ShouldBe((long)expected, callerName);

    //    private void ShouldBe(object expected, [CallerMemberName]string callerName = "")
    //    {
    //        var res = RunTest(callerName).ToObject();
    //        Assert.IsTrue(expected.Equals(res), $"Expected <<{expected}>>, got <<{res}>>.");
    //    }

    //    private Dictionary<string, DyCodeModel> ast;
    //    private static string startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests");

    //    private DyObject RunTest(string name)
    //    {
    //        var testFile = FindFile("tests");

    //        if (ast == null)
    //        {
    //            ast = new Dictionary<string, DyCodeModel>(StringComparer.OrdinalIgnoreCase);
    //            var p = new DyParser();
    //            var res = p.Parse(SourceBuffer.FromFile(testFile));

    //            if (!res.Success)
    //                throw new DyBuildException(res.Messages);

    //            foreach (var f in res.Value.Root.Nodes.OfType<DFunctionDeclaration>())
    //            {
    //                var call = new DApplication(f, f.Location);
    //                var block = new DBlock(f.Location);
    //                block.Nodes.Add(call);
    //                var cm = new DyCodeModel(block, testFile);
    //                ast.Add(f.Name, cm);
    //            }
    //        }

    //        if (!ast.TryGetValue(name, out var codeModel))
    //            throw new System.Exception("Not found: " + name);

    //        var linker = new DyLinker(FileLookup.Create(startupPath), BuilderOptions.Default);
    //        var cres = linker.Make(codeModel);

    //        if (!cres.Success)
    //            throw new DyBuildException(cres.Messages);

    //        var m = new DyMachine(cres.Value);
    //        return m.Execute().Value;
    //    }

    //    private string FindFile(string name)
    //    {
    //        return Path.Combine(startupPath, name + ".dy");
    //    }
    //    #endregion
    //}

    public class Tests
    {
        public object MathTest1 => 38;

        public object MathTest2 => 1.5919117647058822;

        public object MathTest3 => 6.6129032258064528;

        public object MathTest4 => 11.256;

        public object MathTest5 => 0.0099999999999997868;

        public object MathTest6 => 783.59999999999991;

        public object LogicalTest1 => true;

        public object LogicalTest2 => false;

        public object ComparisonTest1 => true;

        public object ComparisonTest2 => true;

        public object ToStringTest1 => "(1, 2, 3)";

        public object ToStringTest2 => "[12.2, \"string\", true]";

        public object ToStringTest3 => "(x: 42, y: \"foo\")";

        public object WhileTest1 => 33;

        public object WhileTest2 => 42;

        public object FizzbuzzTest => "12fizz4buzzfizz78fizzbuzz";

        public object RecursionTest => 10;

        public object FactTest => 2432902008176640000;

        public object PowerTest => 1000;

        public object BinaryConversionTest => "10011101";

        public object FibTest1 => 89;

        public object FibTest2 => "0 1 1 2 3 5 8 13 21 34 55";

        public object PhoneParserTest => "9645061112";

        public object IteratorTest1 => 116.8;

        public object IteratorTest2 => 36;

        public object IteratorTest3 => 20;

        public object IteratorTest4 => "olleH";

        public object IteratorTest5 => "ll";

        public object IteratorTest6 => 8;

        public object CalcETest => 2.7182818284590455;

        public object DammTest => "yesnoyesno";

        public object GcdTest => 12;

        public object EulerTest => "0=100;10=44;20=27.2;30=22.16;40=20.648;50=20.1944;60=20.05832;70=20.017496;80=20.0052488;90=20.00157464;100=20.000472392;";

        public object AckTest => 125;

        public object FoldTest => 21;
    }
}