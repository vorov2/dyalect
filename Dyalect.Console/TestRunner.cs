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
using System.Reflection;
using System.Text;

namespace Dyalect
{
    public class TestRunner
    {
        private readonly List<string> commands = new();
        private readonly StringBuilder builder = new();
        private readonly DyaOptions dyaOptions;
        private readonly BuilderOptions buildOptions;

        sealed class TestBlockInfo
        {
            public DRegion? Block { get; init; }
            public string FileName { get; }
            public string? Error { get; init; }
            public TestBlockInfo(string fileName) => FileName = fileName;
        }

        public TestRunner(BuilderOptions buildOptions, DyaOptions dyaOptions) =>
            (this.buildOptions, this.dyaOptions) = (buildOptions, dyaOptions);

        private void Output(string text)
        {
            builder.AppendLine(text);
            Printer.Output(text);
        }

        private void Error(string text)
        {
            builder.AppendLine(text);
            Printer.Error(text);
        }

        private void LineFeed()
        {
            builder.AppendLine();
            Printer.LineFeed();
        }

        public bool RunTests(IEnumerable<string> fileNames)
        {
            try
            {
                var warns = new List<BuildMessage>();
                var blocks = GatherTests(fileNames, warns);
                Output($"Running tests from {fileNames.Count()} file(s):");
                Output(string.Join(' ', blocks.Select(b => Path.GetFileName(b.FileName)).Distinct()));

                if (blocks is null)
                    return false;

                RunTests(blocks, dyaOptions, buildOptions, warns);

                if (warns.Count > 0)
                {
                    LineFeed();
                    Output($"Warnings:");
                    foreach (var w in warns)
                        Output(w.ToString());
                }

                return true;
            }
#if !DEBUG
            catch (Exception ex)
            {
                Error($"Failure! {ex.Message}");
                return false;
            }
#endif
            finally
            {
                if (!string.IsNullOrWhiteSpace(dyaOptions.SaveTestResults))
                {
                    try
                    {
                        var path = Path.Combine(Environment.CurrentDirectory, dyaOptions.SaveTestResults);
                        File.WriteAllText(path, builder.ToString());
                    }
                    catch (Exception ex)
                    {
                        Error($"Unable to save test results: {ex.Message}");
                    }
                }
            }
        }

        private void Submit()
        {
            LineFeed();
            Output("Submitting test results...");

            try
            {
                foreach (var c in commands)
                    Process.Start("appveyor", c);

                Output("Ok");
            }
            catch (Exception ex)
            {
                Error($"Failure! {ex.Message}");
            }
        }

        private void Failed(string name, string reason, string fileName)
        {
            commands.Add($"AddTest \"{name}\" -Outcome Failed -Framework DyaUnit -FileName {fileName}");
            Output($"[ ] {name} FAILED: {reason}");
        }

        private void Success(DyaOptions options, string name, string fileName)
        {
            commands.Add($"AddTest {name} -Outcome Passed -Framework DyaUnit -FileName {fileName}");

            if (!options.ShowOnlyFailedTests)
                Output($"[+] {name}");
        }

        private TestBlockInfo[] GatherTests(IEnumerable<string> files, List<BuildMessage> warns)
        {
            var blocks = new List<TestBlockInfo>();

            foreach (var file in files)
            {
                var res = DyParser.Parse(SourceBuffer.FromFile(file));

                if (!res.Success)
                {
                    blocks.Add(new TestBlockInfo(file)
                    {
                        Error = "Unable to process test file: " + string.Join(" ", res.Messages)
                    });
                    continue;
                }

                if (res.Messages.Any())
                    warns.AddRange(res.Messages);

                foreach (var node in res.Value!.Root.Nodes)
                    if (node is DRegion b)
                    {
                        blocks.Add(new TestBlockInfo(file)
                        {
                            Block = b
                        });
                    }
            }

            return blocks.ToArray();
        }

        private void RunTests(TestBlockInfo[] testBlocks, DyaOptions options, BuilderOptions builderOptions, List<BuildMessage> warns)
        {
            const string INIT = "Initialize";

            if (testBlocks.Length == 0)
                return;

            var plusFails = false;
            var passed = 0;
            var failed = 0;
            var allCounter = 0;
            var fileCounter = 0;
            var currentFile = "";

            Dictionary<string, DyCodeModel> inits;
            Dictionary<string, bool> files = new();

            try
            {
                inits = testBlocks.Where(b => b.Block is not null && b.Block?.Name == INIT)
                    .ToDictionary(b => b.FileName, b => b.Block.Body);
            }
            catch (ArgumentException)
            {
                throw new Exception("Multiple initialization blocks in a test file.");
            }

            foreach (var bi in testBlocks)
            {
                if (bi.Block?.Name == INIT)
                    continue;

                if (currentFile != bi.FileName)
                {
                    fileCounter++;
                    currentFile = bi.FileName;
                    PrintFileHeader(options, bi.FileName!);
                    allCounter = 0;
                }

                if (bi.Block is null)
                {
                    if (options.ShowOnlyFailedTests)
                        PrintFileHeader(options, bi.FileName, true);

                    Error(bi.Error ?? "Unknown error");
                    failed++;
                    plusFails = true;
                    continue;
                }

                allCounter++;
                var ast = bi.Block.Body;

                if (inits.TryGetValue(bi.FileName, out var init))
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

                var linker = new DyLinker(FileLookup.Create(Path.GetDirectoryName(bi.FileName)!), builderOptions);
                var cres = linker.Make(ast);
                warns.AddRange(cres.Messages.Where(m => m.Type == BuildMessageType.Warning));

                if (!cres.Success)
                {
                    failed++;

                    if (options.ShowOnlyFailedTests && !files.ContainsKey(bi.FileName))
                    {
                        PrintFileHeader(options, bi.FileName!, true);
                        files[bi.FileName] = true;
                    }

                    Failed(bi.Block.Name, string.Join(' ', cres.Messages.Select(m => m.Message)), bi.FileName!);
                    continue;
                }

                var ctx = DyMachine.CreateExecutionContext(cres.Value!);

                try
                {
                    DyMachine.Execute(ctx);
                    ctx.ThrowIf();
                    Success(options, bi.Block.Name, bi.FileName!);
                    passed++;
                }
                catch (Exception ex)
                {
                    failed++;

                    if (options.ShowOnlyFailedTests && !files.ContainsKey(bi.FileName))
                    {
                        PrintFileHeader(options, bi.FileName!, true);
                        files[bi.FileName] = true;
                    }

                    Failed(bi.Block.Name, ex.Message, bi.FileName!);
                }
            }

            LineFeed();
            Output("Total:");
            Output($"{passed} passed, {failed}{(plusFails ? "+" : "")} failed in {fileCounter} file(s)");

            if (options.AppVeyour)
                Submit();
        }

        private void PrintFileHeader(DyaOptions options, string fileName, bool printAlways = false)
        {
            if (printAlways || !options.ShowOnlyFailedTests)
            {
                LineFeed();
                var fi = new FileInfo(fileName);
                Output($"{fi.Directory?.Name}/{fi.Name}:");
            }
        }
    }
}
