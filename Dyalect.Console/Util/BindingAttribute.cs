using System;

namespace Dyalect.Util
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BindingAttribute : Attribute
    {
        public BindingAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }

        public string Help { get; set; }

        public string Category { get; set; }
    }
}
