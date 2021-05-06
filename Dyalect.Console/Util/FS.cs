using System;
using System.IO;

namespace Dyalect.Util
{
    internal static class FS
    {
        class Dya { }

        public static string GetStartupPath() => Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;

        public static string GetPathByType<T>()
        {
            var codeBase = typeof(T).Assembly.Location;
            var uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        public static DateTime GetAssembyTimeStamp() => File.GetLastWriteTime(GetPathByType<Dya>());
    }
}
