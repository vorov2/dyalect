using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dyalect
{
    public static class TestRunner
    {
        sealed class FunSet
        {
            public string FileName { get; set; }

            public ExecutionContext Context { get; set; }

            public Dictionary<string, DyFunction> Funs { get; set; }
        }

        private static List<string> commands = new List<string>();

        public static bool RunTests(string fileName, bool appveyor)
        {
            //try
            {
                var funs = Compile(fileName);
                Printer.Output($"Running tests from {funs.Count} file(s)...");

                if (funs == null)
                    return false;

                Run(funs, appveyor);
                return true;
            }
            //catch (Exception ex)
            //{
            //    Printer.Error($"Failure! {ex.Message}");
            //    return false;
            //}
        }

        private static void Run(IList<FunSet> funs, bool appveyor)
        {
            var passed = 0;
            var failed = 0;
            var i = 0;
            var fi = 0;

            foreach (var funSet in funs)
            {
                Printer.LineFeed();
                Printer.Output($"{++fi}. {funSet.FileName}:");
                var padLen = funSet.Funs.Count.ToString().Length;

                foreach (var fn in funSet.Funs)
                {
                    Console.Write($"[{(++i).ToString().PadLeft(padLen, '0')}] ");

                    //try
                    {
                        var res = fn.Value.Call(funSet.Context).ToObject();
                        Success(fn.Key);
                        passed++;
                    }
                    //catch (Exception ex)
                    //{
                    //    Failed(fn.Key, ex.Message);
                    //    failed++;
                    //}
                }
            }

            Printer.LineFeed();
            Printer.Output("Total:");
            Printer.Output($"{passed} passed, {failed} failed in {funs.Count} file(s)");

            if (appveyor)
                Submit();
        }

        private static void Submit()
        {
            Printer.LineFeed();
            Printer.Output("Submitting test results...");

            try
            {
                foreach (var c in commands)
                    Process.Start("appveyor", c);

                Printer.Output("Ok");
            }
            catch (Exception ex)
            {
                Printer.Error($"Failure! {ex.Message}");
            }
        }

        private static void Failed(string name, string reason)
        {
            commands.Add($"AddTest {name} -Outcome Failed -Framework DUnit -FileName tests.dy");
            Printer.Output($"{name}: Failed: {reason}");
        }

        private static void Success(string name)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DUnit -FileName tests.dy");
            Printer.Output($"{name}: Success");
        }

        private static IList<FunSet> Compile(string fileName)
        {
            var funColl = new List<FunSet>();
            IEnumerable<string> files = null;

            if (File.Exists(fileName))
                files = new string[] { fileName };
            else if (Directory.Exists(fileName))
                files = Directory.EnumerateFiles(fileName, "*.dy");
            else
            {
                Printer.Error($"File or directory {fileName} not found.");
                return null;
            }

            foreach (var file in files)
            {
                var linker = new DyLinker(FileLookup.Create(file), BuilderOptions.Default);
                var cres = linker.Make(SourceBuffer.FromFile(file));
                var funs = new FunSet();
                funs.Funs = new Dictionary<string, DyFunction>(StringComparer.OrdinalIgnoreCase);
                funs.FileName = file;

                if (!cres.Success)
                    throw new DyBuildException(cres.Messages);

                var ctx = DyMachine.CreateExecutionContext(cres.Value);
                funs.Context = ctx;
                DyMachine.Execute(ctx);

                foreach (var v in DyMachine.DumpVariables(ctx))
                {
                    if (v.Value is DyFunction fn)
                    {
                        funs.Funs.Remove(v.Name);
                        funs.Funs.Add(v.Name, fn);
                    }
                }

                funColl.Add(funs);
            }

            return funColl;
        }
    }
}
