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
        private static int failures;

        public static void Main()
        {
            var props = typeof(Tests).GetProperties();
            var obj = Activator.CreateInstance(typeof(Tests));
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var pi in props)
            {
                var v = pi.GetValue(obj);

                if (v is int)
                    v = Convert.ToInt64(v);

                dict.Add(pi.Name, v);
            }

            try
            {
                var funs = Run();
                Analyze(dict, funs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failure: {0}", ex.Message);
                Analyze(dict, new Dictionary<string, DyFunction>());
            }
        }

        private static void Analyze(Dictionary<string, object> expected, Dictionary<string, DyFunction> funs)
        {
            foreach (var k in expected)
            {
                if (funs.TryGetValue(k.Key, out var fn))
                {
                    try
                    {
                        var res = fn.Call().ToObject();

                        if (!res.Equals(k.Value))
                            Failed(k.Key, $"Expected <{k.Value}>, got <{res}>.");
                        else
                            Success(k.Key);
                    }
                    catch (Exception ex)
                    {
                        Failed(k.Key, ex.Message);
                    }
                }
            }

            Console.WriteLine($"Total tests: {expected.Count}. Failed: {failures}.");
            Console.WriteLine();
            Submit();
            Console.WriteLine();
        }

        private static void Submit()
        {
            Console.WriteLine("Submitting test results...");

            try
            {
                foreach (var c in commands)
                {
                    Process.Start("appveyor", c);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Submit failed: {ex.Message}");
            }
        }

        private static void Failed(string name, string reason)
        {
            commands.Add($"AddTest {name} -Outcome Failed -Framework DUnit -FileName tests.dy");
            Console.WriteLine($"{name}: Failed: {reason}");
            failures++;
        }

        private static void Success(string name)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DUnit -FileName tests.dy");
            Console.WriteLine($"{name}: Success");
        }

        private static Dictionary<string, DyFunction> Run()
        {
            var startupPath = Path.Combine(Path.GetDirectoryName(typeof(Tests).Assembly.Location), "Tests"); ;
            var dict = new Dictionary<string, DyFunction>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(startupPath, "*.dy"))
            {
                var linker = new DyLinker(FileLookup.Create(startupPath), BuilderOptions.Default);
                var cres = linker.Make(SourceBuffer.FromFile(file));

                if (!cres.Success)
                    throw new DyBuildException(cres.Messages);

                var m = new DyMachine(cres.Value);
                m.Execute();

                foreach (var v in m.DumpVariables())
                {
                    if (v.Value is DyFunction fn)
                    {
                        dict.Remove(v.Name);
                        dict.Add(v.Name, fn);
                    }
                }
            }

            return dict;
        }
    }
}
