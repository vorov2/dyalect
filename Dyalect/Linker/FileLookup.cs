using System;
using System.IO;

namespace Dyalect.Linker
{
    public sealed class FileLookup
    {
        private const string LIBDIR = "lib";

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
            var asm = typeof(FileLookup).Assembly;
            var codeBase = asm.CodeBase;
            var uri = new UriBuilder(codeBase);
            var systemPath = Uri.UnescapeDataString(uri.Path);

            if (!File.Exists(systemPath))
                systemPath = asm.Location;

            systemPath = Path.GetDirectoryName(systemPath);
            var systemPaths = new string[] { systemPath, Path.Combine(systemPath, LIBDIR) };

            var startupPaths = new string[] { startupPath, Path.Combine(startupPath, LIBDIR) };

            return new FileLookup
            (
                startupPaths,
                GetBasePaths(),
                systemPaths,
                additionalPaths ?? Statics.EmptyStrings
            );
        }

        public bool Find(string currentPath, string fileName, out string fullPath)
        {
            if (File.Exists(fullPath = Path.Combine(currentPath, fileName)))
                return true;

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
            var var = Environment.GetEnvironmentVariable("DYALECT_LIBS");

            if (string.IsNullOrEmpty(var))
                return new string[0];
            else
                return var.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
