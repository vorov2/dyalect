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
        [Test] public void WhileTest1() => AssertEquals(33, RunTest());

        [Test] public void WhileTest2() => AssertEquals(42, RunTest());

        [Test] public void FizzbuzzTest() => AssertEquals("12fizz4buzzfizz78fizzbuzz", RunTest());

        [Test] public void IterTest() => AssertEquals(10, RunTest());
        
        #region Execution
        private void AssertEquals(object expected, DyObject result)
        {
            Assert.True(TypeConverter.ConvertFrom(expected, null).Equals(result), $"Expected {expected}, god {result}");
        }

        private Dictionary<string, DyCodeModel> ast;
        private static string startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests");

        private DyObject RunTest([CallerMemberName]string name = "")
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