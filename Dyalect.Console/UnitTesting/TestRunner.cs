using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dyalect.UnitTesting
{
    public sealed class TestRunner
    {
        private readonly DyaOptions dyaOptions;
        private readonly BuilderOptions buildOptions;

        public TestRunner(BuilderOptions buildOptions, DyaOptions dyaOptions) =>
            (this.buildOptions, this.dyaOptions) = (buildOptions, dyaOptions);

        public bool RunTests(IEnumerable<string> fileNames)
        {
            var report = new TestReport { TestFiles = fileNames.ToArray() };

            try
            {
                var warns = new List<BuildMessage>();
                var blocks = GatherTests(fileNames, warns);
                
                if (blocks is null)
                    return false;

                RunTests(report, blocks, dyaOptions, buildOptions, warns);

                if (warns.Count > 0)
                    report.BuildWarnings = warns;

                var output = TestFormatter.Format(report, dyaOptions.ShowOnlyFailedTests ? TestFormatFlags.OnlyFailed : TestFormatFlags.None);
                Console.WriteLine(output);
                return true;
            }
#if !DEBUG
            catch (Exception ex)
            {
                Printer.Error($"Failure! {ex.Message}");
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
                        var reportStr = TestFormatter.Format(report, dyaOptions.UseMarkdown ? TestFormatFlags.Markdown : TestFormatFlags.None);
                        File.WriteAllText(path, reportStr);
                    }
                    catch (Exception ex)
                    {
                        Printer.Error($"Unable to save test results: {ex.Message}");
                    }
                }
            }
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

        private void RunTests(TestReport report, TestBlockInfo[] testBlocks, DyaOptions options, BuilderOptions builderOptions, List<BuildMessage> warns)
        {
            const string INIT = "Initialize";

            if (testBlocks.Length == 0)
                return;

            Dictionary<string, DyCodeModel> inits;

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
                var fileName = bi.FileName ?? "unknown";

                if (bi.Block?.Name == INIT)
                    continue;
    
                if (bi.Block is null)
                {
                    report.Results.Add(new TestResult
                    {
                        Error = bi.Error ?? "Unknown error",
                        FileName = fileName
                    });
                    report.FailedFiles.Add(fileName);
                    continue;
                }

                var ast = bi.Block.Body;

                if (inits.TryGetValue(fileName, out var init))
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

                var linker = new DyLinker(FileLookup.Create(builderOptions, Path.GetDirectoryName(fileName)!), builderOptions);
                var cres = linker.Make(ast);
                warns.AddRange(cres.Messages.Where(m => m.Type == BuildMessageType.Warning));

                if (!cres.Success)
                {
                    report.Results.Add(new TestResult
                    {
                        Name = bi.Block.Name,
                        Error = string.Join(' ', cres.Messages.Select(m => m.Message)),
                        FileName = bi.FileName ?? "<unknown>"
                    });
                    continue;
                }

                var ctx = DyMachine.CreateExecutionContext(cres.Value!);

                try
                {
                    DyMachine.Execute(ctx);
                    ctx.ThrowIf();
                    report.Results.Add(new TestResult
                    {
                        Name = bi.Block.Name,
                        FileName = bi.FileName ?? "<unknown>"
                    });
                }
                catch (Exception ex)
                {
                    report.Results.Add(new TestResult
                    {
                        Name = bi.Block.Name,
                        Error = ex.Message,
                        FileName = bi.FileName ?? "<unknown>"
                    });
                }
            }
        }
    }
}
