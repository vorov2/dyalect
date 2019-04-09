namespace Dyalect.Parser.Model
{
    public sealed class DyCodeModel
    {
        internal DyCodeModel(DBlock root, string fileName)
        {
            Root = root;
            FileName = fileName;
        }

        public DBlock Root { get; }

        public string FileName { get; }
    }
}
