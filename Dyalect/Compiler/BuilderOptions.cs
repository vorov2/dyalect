namespace Dyalect.Compiler
{
    public sealed class BuilderOptions
    {
        public readonly static BuilderOptions Default = new BuilderOptions
        {
            Debug = false,
            NoLangModule = false,
            NoWarnings = false
        };

        public bool Debug { get; set; }

        public bool NoLangModule { get; set; }

        public bool NoWarnings { get; set; }
    }
}
