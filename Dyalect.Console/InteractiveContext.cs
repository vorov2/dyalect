using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dyalect;

internal sealed class InteractiveContext
{
    public InteractiveContext(DyaOptions options)
    {
        Options = options;
        BuildOptions = CreateBuildOptions(options);
        var nofn = options.FileNames is null || options.FileNames.Length == 0 || string.IsNullOrWhiteSpace(options.FileNames[0]);

        var lookup = FileLookup.Create(BuildOptions,
            nofn ? Environment.CurrentDirectory! : Path.GetDirectoryName(options.FileNames![0])!, options.Paths);
        Linker = new DyIncrementalLinker(lookup, BuildOptions, options.UserArguments);
    }

    public static BuilderOptions CreateBuildOptions(DyaOptions options)
    {
        var ret = new BuilderOptions
        {
            Debug = options.Debug,
            LinkerLog = options.LinkerLog,
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

    public RuntimeContext? RuntimeContext { get; private set; }

    public DyIncrementalLinker Linker { get; private set; }

    public DyaOptions Options { get; set; }

    public void Reset()
    {
        Linker = new DyIncrementalLinker(Linker.Lookup, Linker.BuilderOptions, Options.UserArguments);
    }

    private ExecutionContext GetExecutionContext(UnitComposition composition)
    {
        ExecutionContext ctx;

        if (RuntimeContext is null)
        {
            ctx = DyMachine.CreateExecutionContext(composition);
            RuntimeContext = ctx.RuntimeContext;
        }
        else
        {
            RuntimeContext.Refresh(composition);
            ctx = DyMachine.CreateExecutionContext(RuntimeContext);
        }

        return ctx;
    }

    public bool Eval(string source)
    {
        var made = Linker.Make(SourceBuffer.FromString(source, "<stdio>"));

        if (made.Messages.Any())
            Printer.PrintErrors(made.Messages);

        if (!made.Success || made.Value is null)
            return false;

        var ctx = GetExecutionContext(made.Value!);
        return Eval(ctx, measureTime: false);
    }

    public bool Compile(string fileName, out Unit unit)
    {
        unit = null!;
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

        var ctx = GetExecutionContext(composition!);
        return Eval(ctx, measureTime);
    }

    public bool Eval(ExecutionContext ctx, bool measureTime)
    {
#if !DEBUG
        try
#endif
        {
            var sw = new Stopwatch();
            sw.Start();
            var res = DyMachine.Execute(ctx);
            sw.Stop();
            Printer.Output(res);

            if (measureTime)
                Printer.SupplementaryOutput($"Time taken: {sw.Elapsed:mm\\:ss\\.fffffff}");

            Linker.Commit();
            return true;
        }
#if !DEBUG
        catch (DyCodeException ex)
        {
            Linker.Rollback();
            Printer.Error(ex.ToString());
            return false;
        }
        catch (DyRuntimeException ex)
        {
            Linker.Rollback();
            Printer.Error(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Linker.Rollback();
            Printer.Error($"Critical failure: {Environment.NewLine}{ex}");
            return false;
        }
#endif
    }
}
