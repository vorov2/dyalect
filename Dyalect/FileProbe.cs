using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dyalect;

public static class FileProbe
{
    public static string GetExecutablePath() => Assembly.GetExecutingAssembly().Location;

    public static string GetExecutableDirectory() =>
        Path.GetDirectoryName(GetExecutablePath()) ?? string.Empty;

    public static DateTime GetAssembyTimeStamp() => File.GetLastWriteTime(GetExecutablePath());
}
