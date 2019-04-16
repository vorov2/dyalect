namespace Dyalect.Command
{
    public sealed class OptionBag : IOptionBag
    {
        private const string COMPILER = "Compiler settings";
        private const string LINKER = "Linker settings";
        private const string GENERAL = "General settings";

        [Binding("debug", Help = "Compile in debug mode.", Category = COMPILER)]
        public bool Debug { get; set; }

        [Binding("nolang", Help = "Do not import \"lang\" module that includes basic primitives and operations.", Category = COMPILER)]
        public bool NoLang { get; set; }

        [Binding("path", Help = "A path for linker where to look referenced modules. You can specify this switch multiple times.", Category = LINKER)]
        public string[] Paths { get; set; }

        public string StartupPath { get; set; }

        public string DefaultArgument { get; set; }
    }
}
