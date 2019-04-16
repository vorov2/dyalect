using Dyalect.Util;
using System.Collections.Generic;

namespace Dyalect
{
    public sealed class DyaOptions
    {
        private const string COMPILER = "Compiler settings";
        private const string LINKER = "Linker settings";
        private const string GENERAL = "General settings";

        [Binding(Help = "A full path to the .dy file which should be executed.", Category = COMPILER)]
        public string FileName { get; set; }

        [Binding("debug", Help = "Compile in debug mode.", Category = COMPILER)]
        public bool Debug { get; set; }

        [Binding("nolang", Help = "Do not import \"lang\" module that includes basic primitives and operations.", Category = COMPILER)]
        public bool NoLang { get; set; }

        [Binding("path", Help = "A path where linker would look for referenced modules. You can specify this switch multiple times.", Category = LINKER)]
        public string[] Paths { get; set; }
    }
}
