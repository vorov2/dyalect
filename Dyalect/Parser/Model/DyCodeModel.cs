namespace Dyalect.Parser.Model
{
    public sealed class DyCodeModel
    {
        public DyCodeModel(DBlock root, DImport[] imports, string fileName) =>
            (Root, Imports, FileName) = (root, imports, fileName);

        public DImport[] Imports { get; }

        public DBlock Root { get; }

        public string FileName { get; }
    }
}
