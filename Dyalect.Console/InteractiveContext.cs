using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using System;
using System.IO;
using System.Linq;

namespace Dyalect
{
    internal sealed class InteractiveContext
    {
        public InteractiveContext(DyaOptions options)
        {
            Options = options;
            BuildOptions = CreateBuildOptions(options);
            var nofn = options.FileNames is null || options.FileNames.Length == 0 || string.IsNullOrWhiteSpace(options.FileNames[0]);

            var lookup = FileLookup.Create(
                nofn ? Environment.CurrentDirectory! : Path.GetDirectoryName(options.FileNames![0])!, options.Paths);
            Linker = new DyIncrementalLinker(lookup, BuildOptions, options.UserArguments);
        }

        public static BuilderOptions CreateBuildOptions(DyaOptions options)
        {
            var ret = new BuilderOptions
            {
                Debug = options.Debug,
                NoOptimizations = options.NoOptimizations,
                NoLangModule = options.NoLang,
                NoWarnings = options.NoWarnings,
                NoWarningsLinker = options.NoWarningsLinker
            };

            if (options.IgnoreWarnings != null)
                foreach (var i in options.IgnoreWarnings)
                    if (!ret.IgnoreWarnings.Contains(i))
                        ret.IgnoreWarnings.Add(i);

            return ret;
        }

        public BuilderOptions BuildOptions { get; }

        public ExecutionContext? ExecutionContext { get; private set; }

        public DyLinker Linker { get; private set; }

        public DyaOptions Options { get; set; }

        public void Reset()
        {
            ExecutionContext = null;
            Linker = new DyIncrementalLinker(Linker.Lookup, Linker.BuilderOptions, Options.UserArguments);
        }

        public bool Eval(string source)
        {
            var made = Linker.Make(SourceBuffer.FromString(source, "<stdio>"));

            if (made.Messages.Any())
                Printer.PrintErrors(made.Messages);

            if (!made.Success)
                return false;

            if (ExecutionContext == null)
                ExecutionContext = DyMachine.CreateExecutionContext(made.Value!);

            return Eval(measureTime: false);
        }

        public bool Compile(string fileName, out Unit? unit)
        {
            unit = null;
            Result<Unit> made;

            try
            {
                var buffer = SourceBuffer.FromFile(fileName);
                made = Linker.Compile(buffer);
            }
            catch (Exception ex)
            {
                Printer.Error($"Unable to read file \"{fileName}\": {ex.Message}");
                return false;
            }

            if (made.Messages.Any())
                Printer.PrintErrors(made.Messages);

            if (!made.Success)
                return false;

            unit = made.Value!;
            return true;
        }

        public bool Make(string fileName, out UnitComposition? composition)
        {
            composition = null;
            var made = Linker.Make(fileName);

            if (made.Messages.Any())
                Printer.PrintErrors(made.Messages);

            if (!made.Success)
                return false;

            composition = made.Value!;
            return true;
        }

        public bool EvalFile(string fileName, bool measureTime)
        {
            if (!Make(fileName, out var composition))
                return false;

            if (ExecutionContext is null)
                ExecutionContext = DyMachine.CreateExecutionContext(composition!);

            return Eval(measureTime);
        }

        public bool Eval(bool measureTime)
        {
#if !DEBUG
            try
#endif
            {
                var dt = DateTime.Now;
                var res = DyMachine.Execute(ExecutionContext!);
                Printer.Output(res);

                if (measureTime)
                    Printer.SupplementaryOutput($"Time taken: {DateTime.Now - dt}");

                return true;
            }
#if !DEBUG
            catch (DyCodeException ex)
            {
                Printer.Error(ex.ToString());
                return false;
            }
            catch (DyRuntimeException ex)
            {
                Printer.Error(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Printer.Error($"Critical failure: {Environment.NewLine}{ex.ToString()}");
                return false;
            }
#endif
        }
    }
}
