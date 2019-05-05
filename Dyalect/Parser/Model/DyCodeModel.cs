using System.Collections.Generic;

namespace Dyalect.Parser.Model
{
    public sealed class DyCodeModel
    {
        public DyCodeModel(DBlock root, DImport[] imports, string fileName)
        {
            Root = root;
            Imports = imports;
            FileName = fileName;
        }

        public DImport[] Imports { get; }

        public DBlock Root { get; }

        public string FileName { get; }
    }
}
