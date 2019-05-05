using System;
using System.IO;

namespace Dyalect.Linker
{
    public sealed class FileLookup
    {
        private const string LIBDIR = "lib";
        private static readonly string[] empty = new string[0];

        private string[] startupPaths;
        private string[] standardPaths;
        private string[] systemPaths;
        private string[] additionalPaths;

        private FileLookup(string[] startupPaths, string[] standardPaths, string[] systemPaths, string[] additionalPaths)
        {
            this.startupPaths = startupPaths;
            this.standardPaths = standardPaths;
            this.systemPaths = systemPaths;
            this.additionalPaths = additionalPaths;
        }

        public static FileLookup Create(string startupPath, string[] additionalPaths = null)
        {
            var codeBase = typeof(FileLookup).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var systemPath = Uri.UnescapeDataString(uri.Path);
            var systemPaths = new string[] { systemPath, Path.Combine(systemPath, LIBDIR) };

            var dir = Path.GetDirectoryName(startupPath);
            var startupPaths = new string[] { dir, Path.Combine(dir, LIBDIR) };

            return new FileLookup
            (
                startupPaths,
                GetBasePaths(),
                systemPaths,
                additionalPaths ?? empty
            );
        }

        public bool Find(string fileName, out string fullPath)
        {
            if (LookIn(fileName, startupPaths, out fullPath)
                || LookIn(fileName, standardPaths, out fullPath)
                || LookIn(fileName, systemPaths, out fullPath)
                || LookIn(fileName, additionalPaths, out fullPath))
                return true;

            return false;
        }

        private bool LookIn(string fileName, string[] dirs, out string path)
        {
            path = null;

            foreach (var p in startupPaths)
                if (File.Exists(path = Path.Combine(p, fileName)))
                    return true;

            return false;
        }

        private static string[] GetBasePaths()
        {
            var var = Environment.GetEnvironmentVariable("DYALECT_PATH");

            if (string.IsNullOrEmpty(var))
                return new string[0];
            else
                return var.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
