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
        private static List<string> commands = new List<string>();

        public static void Main()
        {
            try
            {
                WriteLineWithColor(ConsoleColor.Cyan, "Running tests...");
                var funs = Run();
                Analyze(funs);
            }
            catch (Exception ex)
            {
                WriteLineWithColor(ConsoleColor.Red, $"Failure! {ex.Message}");
                Analyze(new Dictionary<string, (ExecutionContext,DyFunction)>());
            }
        }

        private static void Analyze(Dictionary<string, (ExecutionContext,DyFunction)> funs)
        {
            var passed = 0;
            var i = 0;

            foreach (var kv in funs)
            {
                var cont = kv.Value;
                var fn = cont.Item2;
                Console.Write($"[{(++i).ToString().PadLeft(3, '0')}] ");

                try
                {
                    var res = fn.Call(cont.Item1).ToObject();

                    if (res != null)
                        throw new Exception("Not run.");

                    Success(kv.Key);
                    passed++;
                }
                catch (Exception ex)
                {
                    Failed(kv.Key, ex.Message);
                }
            }

            var err = Console.ForegroundColor;

            if (passed != funs.Count)
                err = ConsoleColor.Red;


            var good = Console.ForegroundColor;

            if (passed > 0)
                good = ConsoleColor.Green;

            Console.WriteLine(new string('-', 20));
            WriteWithColor(good, $"{passed}");
            Console.Write(" passed, ");
            WriteWithColor(err, $"{funs.Count - passed}");
            Console.WriteLine(" failed");
            Console.WriteLine();
            Submit();
            Console.WriteLine();
        }

        private static void Submit()
        {
            WriteLineWithColor(ConsoleColor.Cyan, "Submitting test results...");

            try
            {
                foreach (var c in commands)
                    Process.Start("appveyor", c);

                WriteLineWithColor(ConsoleColor.Green, "Ok");
            }
            catch (Exception ex)
            {
                WriteLineWithColor(ConsoleColor.Red, $"Failure! {ex.Message}");
            }
        }

        private static void Failed(string name, string reason)
        {
            commands.Add($"AddTest {name} -Outcome Failed -Framework DUnit -FileName tests.dy");
            Console.Write($"{name}: ");
            WriteLineWithColor(ConsoleColor.Red, $"Failed: {reason}");
        }

        private static void Success(string name)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DUnit -FileName tests.dy");
            Console.Write($"{name}: ");
            WriteLineWithColor(ConsoleColor.Green, "Success");
        }

        private static void WriteLineWithColor(ConsoleColor col, string line)
        {
            var ocol = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.WriteLine(line);
            Console.ForegroundColor = ocol;
        }

        private static void WriteWithColor(ConsoleColor col, string txt)
        {
            var ocol = Console.ForegroundColor;
            Console.ForegroundColor = col;
            Console.Write(txt);
            Console.ForegroundColor = ocol;
        }

        private static Dictionary<string, (ExecutionContext,DyFunction)> Run()
        {
            var startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests"); ;
            var dict = new Dictionary<string, (ExecutionContext,DyFunction)>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(startupPath, "*.dy"))
            {
                var linker = new DyLinker(FileLookup.Create(startupPath), BuilderOptions.Default);
                var cres = linker.Make(SourceBuffer.FromFile(file));

                if (!cres.Success)
                    throw new DyBuildException(cres.Messages);

                var ctx = DyMachine.CreateExecutionContext(cres.Value);
                DyMachine.Execute(ctx);

                foreach (var v in DyMachine.DumpVariables(ctx))
                {
                    if (v.Value is DyFunction fn)
                    {
                        dict.Remove(v.Name);
                        dict.Add(v.Name, (ctx, fn));
                    }
                }
            }

            return dict;
        }
    }
}
