using Dyalect;
using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tests
{
    public class Tests
    {
        [Test] public void MathTest1() => ShouldBe(38);

        [Test] public void MathTest2() => ShouldBe(1.5919117647058822);

        [Test] public void MathTest3() => ShouldBe(6.6129032258064528);

        [Test] public void MathTest4() => ShouldBe(11.256);

        [Test] public void MathTest5() => ShouldBe(0.0099999999999997868);

        [Test] public void MathTest6() => ShouldBe(783.59999999999991);

        [Test] public void LogicalTest1() => ShouldBe(true);

        [Test] public void LogicalTest2() => ShouldBe(false);

        [Test] public void ComparisonTest1() => ShouldBe(true);

        [Test] public void ComparisonTest2() => ShouldBe(true);

        [Test] public void ToStringTest1() => ShouldBe("(1, 2, 3)");

        [Test] public void ToStringTest2() => ShouldBe("[12.2, \"string\", true]");

        [Test] public void ToStringTest3() => ShouldBe("(x: 42, y: \"foo\")");

        [Test] public void WhileTest1() => ShouldBe(33);

        [Test] public void WhileTest2() => ShouldBe(42);

        [Test] public void FizzbuzzTest() => ShouldBe("12fizz4buzzfizz78fizzbuzz");

        [Test] public void RecursionTest() => ShouldBe(10);

        [Test] public void FactTest() => ShouldBe(2432902008176640000);

        [Test] public void PowerTest() => ShouldBe(1000);

        [Test] public void BinaryConversionTest() => ShouldBe("10011101");

        [Test] public void FibTest() => ShouldBe(89);

        [Test] public void PhoneParserTest() => ShouldBe("9645061112");

        [Test] public void IteratorTest1() => ShouldBe(116.8);

        [Test] public void IteratorTest2() => ShouldBe(36);

        [Test] public void IteratorTest3() => ShouldBe(20);

        [Test] public void IteratorTest4() => ShouldBe("olleH");

        [Test] public void IteratorTest5() => ShouldBe("ll");

        [Test] public void CalcETest() => ShouldBe(2.7182818284590455);

        [Test] public void DammTest() => ShouldBe("yesnoyesno");

        [Test] public void EulerTest() => ShouldBe("0=100;10=44;20=27.2;30=22.16;40=20.648;50=20.1944;60=20.05832;70=20.017496;80=20.0052488;90=20.00157464;100=20.000472392;");

        #region Execution
        private void ShouldBe(int expected, [CallerMemberName]string callerName = "") =>
            ShouldBe((long)expected, callerName);

        private void ShouldBe(object expected, [CallerMemberName]string callerName = "")
        {
            var res = RunTest(callerName).ToObject();
            Assert.True(expected.Equals(res), $"Expected <<{expected}>>, got <<{res}>>.");
        }

        private Dictionary<string, DyCodeModel> ast;
        private static string startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests");

        private DyObject RunTest(string name)
        {
            var testFile = FindFile("tests");

            if (ast == null)
            {
                ast = new Dictionary<string, DyCodeModel>(StringComparer.OrdinalIgnoreCase);
                var p = new DyParser();
                var res = p.Parse(SourceBuffer.FromFile(testFile));

                if (!res.Success)
                    throw new DyBuildException(res.Messages);

                foreach (var f in res.Value.Root.Nodes.OfType<DFunctionDeclaration>())
                {
                    var call = new DApplication(f, f.Location);
                    var block = new DBlock(f.Location);
                    block.Nodes.Add(call);
                    var cm = new DyCodeModel(block, testFile);
                    ast.Add(f.Name, cm);
                }
            }

            if (!ast.TryGetValue(name, out var codeModel))
                throw new System.Exception("Not found: " + name);

            var linker = new DyLinker(FileLookup.Create(startupPath), BuilderOptions.Default);
            var cres = linker.Make(codeModel);

            if (!cres.Success)
                throw new DyBuildException(cres.Messages);

            var m = new DyMachine(cres.Value);
            return m.Execute().Value;
        }

        private string FindFile(string name)
        {
            return Path.Combine(startupPath, name + ".dy");
        }
        #endregion
    }
}