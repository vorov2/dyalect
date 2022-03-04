using System;
using System.IO;

namespace Dyalect.Compiler
{
    public sealed class FileLookup
    {
        private const string LIBDIR = "lib";

        private readonly string[] startupPaths;
        private readonly string[] standardPaths;
        private readonly string[] systemPaths;
        private readonly string[] additionalPaths;

        internal static readonly FileLookup Default = new(Array.Empty<string>(), Array.Empty<string>(), 
            Array.Empty<string>(), Array.Empty<string>());

        private FileLookup(string[] startupPaths, string[] standardPaths, string[] systemPaths, string[] additionalPaths)
        {
            this.startupPaths = startupPaths;
            this.standardPaths = standardPaths;
            this.systemPaths = systemPaths;
            this.additionalPaths = additionalPaths;
        }

        public static FileLookup Create(string startupPath, string[]? additionalPaths = null)
        {
            var asm = typeof(FileLookup).Assembly;
            var codeBase = asm.Location;
            var uri = new UriBuilder(codeBase);
            var systemPath = Uri.UnescapeDataString(uri.Path);

            if (!File.Exists(systemPath))
                systemPath = asm.Location;

            systemPath = Path.GetDirectoryName(systemPath)!;
            var systemPaths = new string[] { systemPath, Path.Combine(systemPath, LIBDIR) };
            var startupPaths = startupPath != null ? new string[] { startupPath, Path.Combine(startupPath, LIBDIR) }
                : Array.Empty<string>();

            return new
            (
                startupPaths,
                GetBasePaths(),
                systemPaths,
                additionalPaths ?? Array.Empty<string>()
            );
        }

        public bool Find(string? currentPath, string fileName, out string fullPath)
        {
            if (currentPath is not null && File.Exists(fullPath = Path.Combine(currentPath, fileName)))
                return true;

            if (LookIn(fileName, startupPaths, out fullPath!)
                || LookIn(fileName, standardPaths, out fullPath!)
                || LookIn(fileName, systemPaths, out fullPath!)
                || LookIn(fileName, additionalPaths, out fullPath!))
                return true;

            return false;
        }

        private bool LookIn(string fileName, string[] dirs, out string? path)
        {
            path = null;

            foreach (var p in dirs)
                if (File.Exists(path = Path.Combine(p, fileName)))
                    return true;

            return false;
        }

        private static string[] GetBasePaths()
        {
            var var = Environment.GetEnvironmentVariable("DYALECT_LIBS");

            if (string.IsNullOrEmpty(var))
                return Array.Empty<string>();
            
            return var.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
