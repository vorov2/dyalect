using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Linker;

public sealed class FileLookup
{
    private const string LIBDIR = "lib";
    private const string LOGFILE = "dyalect_error.log";

    private readonly BuilderOptions options;
    private readonly string[] startupPaths;
    private readonly string[] standardPaths;
    private readonly string[] systemPaths;
    private readonly string[] additionalPaths;

    internal BuilderOptions BuilderOptions => options;

    internal static readonly FileLookup Default = new(BuilderOptions.Default(), Array.Empty<string>(),
        Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

    private FileLookup(BuilderOptions options, string[] startupPaths, string[] standardPaths, string[] systemPaths, string[] additionalPaths)
    {
        this.options = options;
        this.startupPaths = startupPaths;
        this.standardPaths = standardPaths;
        this.systemPaths = systemPaths;
        this.additionalPaths = additionalPaths;
    }

    public static FileLookup Create(BuilderOptions options, string startupPath, string[]? additionalPaths = null)
    {
        var systemPath = FileProbe.GetExecutableDirectory();
        var systemPaths = new string[] { systemPath, Path.Combine(systemPath, LIBDIR) };
        var startupPaths = startupPath is not null
            ? new string[] { startupPath, Path.Combine(startupPath, LIBDIR) }
            : Array.Empty<string>();

        return new
        (
            options,
            startupPaths,
            GetBasePaths(),
            systemPaths,
            additionalPaths ?? Array.Empty<string>()
        );
    }

    public bool Find(string? currentPath, string fileName, out string fullPath)
    {
        if (options.LinkerLog is not null)
        {
            WriteToLog(new string('=', 80));
            WriteToLog($"Resolving module or assembly \"{fileName}\":");
        }

        if (currentPath is not null && File.Exists(fullPath = Path.Combine(currentPath, fileName)))
        {
            if (options.LinkerLog is not null)
            {
                WriteToLog($"Found");
                WriteToLog($"Load from \"{fullPath}\"");
            }

            return true;
        }

        if (LookIn(fileName, startupPaths, out fullPath!)
            || LookIn(fileName, standardPaths, out fullPath!)
            || LookIn(fileName, systemPaths, out fullPath!)
            || LookIn(fileName, additionalPaths, out fullPath!))
            return true;

        if (options.LinkerLog is not null)
            WriteToLog($"Not found");

        return false;
    }

    private bool LookIn(string fileName, string[] dirs, out string? path)
    {
        path = null;

        foreach (var p in dirs)
        {
            path = Path.Combine(p, fileName);

            if (options.LinkerLog is not null)
                WriteToLog($"Probing {path}");

            if (File.Exists(path))
            {
                if (options.LinkerLog is not null)
                {
                    WriteToLog($"Found");
                    WriteToLog($"Load from \"{path}\"");
                }

                return true;
            }
        }

        return false;
    }

    private static string[] GetBasePaths()
    {
        var var = Environment.GetEnvironmentVariable("DYALECT_LIBS");

        if (string.IsNullOrEmpty(var))
            return Array.Empty<string>();
        
        return var.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private void WriteToLog(string str)
    {
        var fn = options.LinkerLog!;

        if (!Path.IsPathRooted(fn))
            fn = Path.Combine(Environment.CurrentDirectory, fn);

        try
        {
            File.AppendAllText(fn, str + Environment.NewLine);
        }
        catch (Exception ex)
        {
            //Attempt to log this error
            try
            {
                File.AppendAllLines(Path.Combine(Environment.CurrentDirectory, LOGFILE),
                    new[] {
                        new string('=', 80),
                        $"Unable to write log file \"{fn}\" because of an error:",
                        ex.Message
                    });
            }
            catch { } //If it doesn't work, don't fail
        }
    }
}
