using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dyalect
{
    internal sealed class InteractiveContext
    {
        public InteractiveContext(DyaOptions options)
        {
            Options = options;
            var buildOptions = new BuilderOptions
            {
                Debug = options.Debug,
                NoLangModule = options.NoLang
            };

            var lookup = FileLookup.Create(FS.GetStartupPath(), options.Paths);
            Linker = new DyIncrementalLinker(lookup, buildOptions);
        }

        public ExecutionContext ExecutionContext { get; private set; }

        public DyLinker Linker { get; private set; }

        public DyaOptions Options { get; set; }

        public void Reset()
        {
            ExecutionContext = null;
            Linker = new DyIncrementalLinker(Linker.Lookup, Linker.BuilderOptions);
        }

        public bool Eval(string source)
        {
            var made = Linker.Make(SourceBuffer.FromString(source, "<stdio>"));

            if (made.Messages.Any())
                Printer.PrintErrors(made.Messages);

            if (!made.Success)
                return false;

            if (ExecutionContext == null)
                ExecutionContext = DyMachine.CreateExecutionContext(made.Value);

            return Eval(measureTime: false);
        }

        public bool EvalFile(string fileName, bool measureTime)
        {
            UnitComposition composition = null;
            SourceBuffer buffer = null;

            try
            {
                if (!File.Exists(fileName))
                {
                    Printer.Error($"File {fileName} doesn't exist.");
                    return false;
                }

                buffer = SourceBuffer.FromFile(fileName);
            }
            catch (Exception ex)
            {
                Printer.Error($"Error reading file: {ex.Message}");
            }

            var made = Linker.Make(buffer);

            if (made.Messages.Any())
                Printer.PrintErrors(made.Messages);

            if (!made.Success)
                return false;

            composition = made.Value;

            if (ExecutionContext == null)
                ExecutionContext = DyMachine.CreateExecutionContext(composition);

            return Eval(measureTime);
        }

        public bool Eval(bool measureTime)
        {
#if !DEBUG
            try
#endif
            {
                var dt = DateTime.Now;
                var res = DyMachine.Execute(ExecutionContext);
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
