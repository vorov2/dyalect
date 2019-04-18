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

        [Test] public void WhileTest1() => ShouldBe(33);

        [Test] public void WhileTest2() => ShouldBe(42);

        [Test] public void FizzbuzzTest() => ShouldBe("12fizz4buzzfizz78fizzbuzz");

        [Test] public void IterTest() => ShouldBe(10);

        [Test] public void FactTest() => ShouldBe(2432902008176640000);

        [Test] public void PowerTest() => ShouldBe(1000);

        [Test] public void BinaryConversionTest() => ShouldBe("10011101");

        [Test] public void FibTest() => ShouldBe(89);

        [Test] public void PhoneParserTest() => ShouldBe("9645061112");

        #region Execution
        private void ShouldBe(int expected, [CallerMemberName]string callerName = "") =>
            ShouldBe((long)expected, callerName);

        private void ShouldBe(object expected, [CallerMemberName]string callerName = "")
        {
            var res = RunTest(callerName).ToObject();
            Assert.True(expected.Equals(res), $"Expected {expected}, got {res}");
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