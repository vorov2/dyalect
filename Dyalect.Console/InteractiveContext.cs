using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Util;
using System;
using System.Collections.Generic;
using System.IO;
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

        public DyMachine Machine { get; private set; }

        public DyLinker Linker { get; private set; }

        public DyaOptions Options { get; set; }

        public void Reset()
        {
            Machine = null;
            Linker = new DyIncrementalLinker(Linker.Lookup, Linker.BuilderOptions);
        }

        public bool Eval(string source)
        {
            var made = Linker.Make(SourceBuffer.FromString(source, "<stdio>"));

            if (!made.Success)
            {
                Printer.PrintErrors(made.Messages);
                return false;
            }

            if (Machine == null)
                Machine = new DyMachine(made.Value);

            return Eval();
        }

        public bool EvalFile(string fileName)
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

            if (!made.Success)
            {
                Printer.PrintErrors(made.Messages);
                return false;
            }

            composition = made.Value;

            if (Machine == null)
                Machine = new DyMachine(composition);

            return Eval();
        }

        public bool Eval()
        {
            try
            {
                var res = Machine.Execute();
                Printer.Output(res);
                return true;
            }
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
        }
    }
}
