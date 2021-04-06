using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        private static readonly List<string> commands = new List<string>();

        public static bool RunTests(IEnumerable<string> fileNames, DyaOptions dyaOptions, BuilderOptions buildOptions)
        {
#if !DEBUG
            try
#endif
            {
                var funs = Compile(fileNames, buildOptions, out var warns);
                Printer.Output($"Running tests from {funs.Count} file(s):");
                Printer.Output(string.Join(' ', funs.Select(f => Path.GetFileName(f.FileName))));

                if (funs == null)
                    return false;

                Run(funs, dyaOptions);

                if (warns.Count > 0)
                {
                    Printer.LineFeed();
                    Printer.Output($"Warnings:");
                    foreach (var w in warns)
                        Printer.Output(w.ToString());
                }

                return true;
            }
#if !DEBUG
            catch (Exception ex)
            {
                Printer.Error($"Failure! {ex.Message}");
                return false;
            }
#endif
        }

        private static void Run(IList<FunSet> funs, DyaOptions options)
        {
            var passed = 0;
            var failed = 0;
            var i = 0;

            foreach (var funSet in funs)
            {
                var padLen = funSet.Funs.Count.ToString().Length;
                var hasHeader = false;

                void printIt(int i, bool failed)
                {
                    if (!hasHeader && (failed || !options.ShowOnlyFailedTests))
                    {
                        Printer.LineFeed();
                        Printer.Output($"{Path.GetFileName(funSet.FileName)}:");
                        hasHeader = true;
                    }

                    if (failed || !options.ShowOnlyFailedTests)
                        Console.Write($"[{i.ToString().PadLeft(padLen, '0')}] ");
                }

                foreach (var fn in funSet.Funs)
                {
                    i++;

                    try
                    {
                        var res = fn.Value.Call(funSet.Context).ToObject();
                        printIt(i, false);
                        Success(options, fn.Key, funSet.FileName);
                        passed++;
                    }
                    catch (AccessViolationException ex)
                    {
                        printIt(i, true);
                        Failed(options, fn.Key, ex.Message, funSet.FileName);
                        failed++;
                    }
                }
            }

            Printer.LineFeed();
            Printer.Output("Total:");
            Printer.Output($"{passed} passed, {failed} failed in {funs.Count} file(s)");

            if (options.AppVeyour)
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

        private static void Failed(DyaOptions options, string name, string reason, string fileName)
        {
            commands.Add($"AddTest {name} -Outcome Failed -Framework DyaUnit -FileName {fileName}");
            Printer.Output($"{name}: Failed: {reason}");
        }

        private static void Success(DyaOptions options, string name, string fileName)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DyaUnit -FileName {fileName}");
            if (!options.ShowOnlyFailedTests)
                Printer.Output($"{name}: Success");
        }

        private static IList<FunSet> Compile(IEnumerable<string> files, BuilderOptions buildOptions, out List<BuildMessage> warns)
        {
            var funColl = new List<FunSet>();
            warns = new List<BuildMessage>();

            foreach (var file in files)
            {
                var linker = new DyLinker(FileLookup.Create(Path.GetDirectoryName(file)), buildOptions);
                var cres = linker.Make(SourceBuffer.FromFile(file));
                var funs = new FunSet();
                funs.Funs = new Dictionary<string, DyFunction>(StringComparer.OrdinalIgnoreCase);
                funs.FileName = file;

                if (!cres.Success)
                    throw new DyBuildException(cres.Messages);

                warns.AddRange(cres.Messages.Where(m => m.Type == BuildMessageType.Warning));
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
