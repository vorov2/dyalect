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

        [Binding("nocol", Help = "Disable colors in console.", Category = GENERAL)]
        public bool NoColors { get; set; }

        [Binding("nologo", Help = "Do not show logo.", Category = GENERAL)]
        public bool NoLogo { get; set; }

        [Binding("theme", Help = "Sets a name of Dyalect Console visual theme.", Category = GENERAL)]
        public string Theme { get; set; }
    }
}
