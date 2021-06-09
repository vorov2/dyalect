using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dyalect
{
    public static class NewTestRunner
    {
        private static readonly List<string> commands = new();

        public static bool RunTests(IEnumerable<string> fileNames, DyaOptions dyaOptions, BuilderOptions buildOptions)
        {
#if !DEBUG
            try
#endif
            {
                var warns = new List<BuildMessage>();
                var blocks = GatherTests(fileNames, warns);
                Printer.Output($"Running tests from {fileNames.Count()} file(s):");
                Printer.Output(string.Join(' ', blocks.Select(b => Path.GetFileName(b.FileName))));

                if (blocks is null)
                    return false;

                RunTests(blocks, dyaOptions, buildOptions, warns);

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

        private static void Failed(string name, string reason, string fileName)
        {
            commands.Add($"AddTest {name} -Outcome Failed -Framework DyaUnit -FileName {fileName}");
            Printer.Output($"[ ] {name} FAILED: {reason}");
        }

        private static void Success(DyaOptions options, string name, string fileName)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DyaUnit -FileName {fileName}");

            if (!options.ShowOnlyFailedTests)
                Printer.Output($"[+] {name}");
        }

        private static DTestBlock[] GatherTests(IEnumerable<string> files, List<BuildMessage> warns)
        {
            var blocks = new List<DTestBlock>();

            foreach (var file in files)
            {
                var res = DyParser.Parse(SourceBuffer.FromFile(file));

                if (!res.Success)
                    throw new DyBuildException(res.Messages);

                if (res.Messages.Any())
                    warns.AddRange(res.Messages);

                foreach (var node in res.Value!.Root.Nodes)
                    if (node is DTestBlock b)
                    {
                        b.FileName = file;
                        blocks.Add(b);
                    }
            }

            return blocks.ToArray();
        }

        private static void RunTests(DTestBlock[] testBlocks, DyaOptions options, BuilderOptions builderOptions, List<BuildMessage> warns)
        {
            if (testBlocks.Length == 0)
                return;

            var passed = 0;
            var failed = 0;
            var allCounter = 0;
            var fileCounter = 0;
            var currentFile = "";

            Dictionary<string, DyCodeModel> inits;

            try
            {
                inits = testBlocks.Where(b => b.Name == "init")
                    .ToDictionary(b => b.FileName!, b => b.Body);
            }
            catch (ArgumentException)
            {
                throw new Exception("Multiple initialization block in a test file.");
            }

            foreach (var block in testBlocks)
            {
                if (block.Name == "init")
                    continue;

                if (currentFile != block.FileName)
                {
                    fileCounter++;
                    currentFile = block.FileName;
                    PrintFileHeader(options, block.FileName!);
                    allCounter = 0;
                }

                allCounter++;
                var ast = block.Body;

                if (inits.TryGetValue(block.FileName!, out var init))
                {
                    var imports = ast.Imports;

                    if (init.Imports is not null)
                    {
                        imports = new DImport[ast.Imports.Length + init.Imports.Length];
                        Array.Copy(init.Imports, 0, imports, 0, init.Imports.Length);
                        Array.Copy(ast.Imports, 0, imports, init.Imports.Length, ast.Imports.Length);
                    }

                    var root = new DBlock(init.Root.Location);
                    root.Nodes.AddRange(init.Root.Nodes);
                    root.Nodes.AddRange(ast.Root.Nodes);
                    ast = new DyCodeModel(root, imports, ast.FileName);
                }

                var linker = new DyLinker(FileLookup.Create(Path.GetDirectoryName(block.FileName)!), builderOptions);
                var cres = linker.Make(ast);
                warns.AddRange(cres.Messages.Where(m => m.Type == BuildMessageType.Warning));

                if (!cres.Success)
                {
                    failed++;
                    Failed(block.Name, string.Join(' ', cres.Messages.Select(m => m.Message)), block.FileName!);
                    continue;
                }

                var ctx = DyMachine.CreateExecutionContext(cres.Value!);

                try
                {
                    DyMachine.Execute(ctx);
                    ctx.ThrowIf();
                    Success(options, block.Name, block.FileName!);
                    passed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    Failed(block.Name, ex.Message, block.FileName!);
                }
            }

            Printer.LineFeed();
            Printer.Output("Total:");
            Printer.Output($"{passed} passed, {failed} failed in {fileCounter} file(s)");

            if (options.AppVeyour)
                Submit();
            }

        private static void PrintFileHeader(DyaOptions options, string fileName)
        {
            if (!options.ShowOnlyFailedTests)
            {
                Printer.LineFeed();
                var fi = new FileInfo(fileName);
                Printer.Output($"{fi.Directory?.Name}/{fi.Name}:");
            }
        }
    }
}
