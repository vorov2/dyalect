using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Codegen
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class InstanceMethodAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}
