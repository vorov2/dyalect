using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Command
{
    public sealed class CommandLine
    {
        public string StartupPath { get; set; }

        public string DefaultArgument { get; set; }

        public List<Option> Options { get; } = new List<Option>();
    }
}
