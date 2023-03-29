using Dyalect.Runtime.Types;
using Dyalect.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dyalect;

public sealed class DyaOptions : IOptionBag
{
    private const string CompilerHeader = "Compiler settings";
    private const string LinkerHeader = "Linker settings";
    private const string GeneralHeader = "General settings";
    private const string TestingHeader = "Testing settings";

    [Binding(Help = "A full path to the .dy file which should be executed, tested or compiled (or to the directory with files). Several files or directories can be specified.", Category = CompilerHeader)]
    public string[]? FileNames { get; set; }

    [Binding("out", Help = "Specifies an output directory (e.g. for a compiled file).", Category = CompilerHeader)]
    public string? OutputDirectory { get; set; }

    [Binding("c", "compile", Help = "Compiles all provided files. By default an object file is placed in the same directory as compiled file (with a .dyo extension). In order to change an output directory, use -out switch.", Category = CompilerHeader)]
    public bool Compile { get; set; }

    [Binding("il", Help = "Compiles all provided files and outputs IL (intermediate assembly). If an output directory (-out switch) is given IL is saved to files, otherwise - printed to standard output.", Category = CompilerHeader)]
    public bool GenerateIL { get; set; }

    [Binding("debug", Help = "Compile in debug mode.", Category = CompilerHeader)]
    public bool Debug { get; set; }

    [Binding("nopt", Help = "Disable all compiler optimizations.", Category = CompilerHeader)]
    public bool NoOptimizations { get; set; }

    [Binding("nowarn", Help = "Do not generate warnings.", Category = CompilerHeader)]
    public bool NoWarnings { get; set; }

    [Binding("ignore", Help = "Ignore specific warnings (works for both compiler and linker). You can specify this switch multiple times, e.g.: -ignore 301 -ignore 302.", Category = GeneralHeader)]
    public int[]? IgnoreWarnings { get; set; }

    [Binding("nowarnlinker", Help = "Do not generate warnings by linker.", Category = LinkerHeader)]
    public bool NoWarningsLinker { get; set; }

    [Binding("nolang", Help = "Do not import \"lang\" module that includes basic primitives and operations.", Category = CompilerHeader)]
    public bool NoLang { get; set; }

    [Binding("linklog", Help = "Specifies a file where linker would log information loading of modules and assemblies. If a file is not specified logging is not performed. This settings seriously affects performance.", Category = LinkerHeader)]
    public string? LinkerLog { get; set; }

    [Binding("path", Help = "A path where linker would look for referenced modules. You can specify this switch multiple times.", Category = LinkerHeader)]
    public string[]? Paths { get; set; }

    [Binding("nologo", Help = "Do not show logo.", Category = GeneralHeader)]
    public bool NoLogo { get; set; }

    [Binding("time", Help = "Measure execution time.", Category = GeneralHeader)]
    public bool MeasureTime { get; set; }

    [Binding("test", Help = "Run unit tests from a file (or files if a path to a directory is specified). Usage: dya [path to a file or directory] -test.", Category = TestingHeader)]
    public bool Test { get; set; }

    [Binding("onlyfailed", Help = "Show only failed tests (the default behavior is to report about all executed tests).", Category = TestingHeader)]
    public bool ShowOnlyFailedTests { get; set; }

    [Binding("testresults", Help = "Specifies a file to save test results. If file is not specified test results are only printed to console.", Category = TestingHeader)]
    public string? SaveTestResults { get; set; }

    [Binding("useMarkdown", Help = "Generate test results in Markdown format. This setting is only applied when test results are save to a file specified by \"testresults\" settings.", Category = TestingHeader)]
    public bool UseMarkdown { get; set; }

    [Binding("i", Help = "Stay in interactive mode after executing a file.", Category = GeneralHeader)]
    public bool StayInInteractive { get; set; }

    public DyTuple? UserArguments { get; set; }

    public override string ToString()
    {
        var list = new List<(string, string)>();

        foreach (var pi in typeof(DyaOptions).GetProperties())
        {
            if (Attribute.GetCustomAttribute(pi, typeof(BindingAttribute)) is BindingAttribute attr)
            {
                var val = pi.GetValue(this)!;
                var byt = pi.PropertyType == typeof(bool);
                var i4 = pi.PropertyType == typeof(int);

                if ((byt && !(bool)val) || (i4 && (int)val == 0) || val is null)
                    continue;

                var key = attr.Names?.Length > 0 ? attr.Names[0] : "<file name>";
                list.Add((key, byt ? ""
                    : val is not string && val is System.Collections.IEnumerable seq
                        ? string.Join(';', seq.OfType<object>().Select(v => v.ToString()))
                    : val.ToString()!));
            }
        }

        var max = list.Max(e => e.Item1.Length) + 1;
        var sb = new StringBuilder();

        foreach (var (k,v) in list)
        {
            if (sb.Length > 0)
                sb.AppendLine();

            sb.Append(("-" + k ).PadRight(max, ' ') + " ");
            sb.Append(v);
        }

        return sb.ToString();
    }

    public IEnumerable<string> GetFileNames()
    {
        if (FileNames is null || FileNames.Length == 0)
            yield break;

        foreach (var item in FileNames)
        {
            if (Directory.Exists(item))
            {
                foreach (var f in Directory.GetFiles(item, "*.dy"))
                    yield return f;
            }
            else
                yield return item;
        }
    }
}
