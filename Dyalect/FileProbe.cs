using System.Diagnostics;
using System.IO;
namespace Dyalect;

public static class FileProbe
{
    public static string GetExecutablePath()
    {
        var mod = Process.GetCurrentProcess().MainModule?.FileName;
        return mod is null ? GetFromCommandLine() : mod;
    }

    public static string GetExecutableDirectory() =>
        Path.GetDirectoryName(GetExecutablePath()) ?? string.Empty;

    private static string GetFromCommandLine()
    {
        var arr = Environment.GetCommandLineArgs();

        if (arr is null || arr.Length == 0)
            throw new DyException("Unable to get executable assembly path.");

        return arr[0].Trim('"');
    }

    public static DateTime GetAssembyTimeStamp() => File.GetLastWriteTime(GetExecutablePath());
}
