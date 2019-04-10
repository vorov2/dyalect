using System;

namespace Dyalect.Command
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class BindingAttribute : Attribute, IBindingInfo
    {
        public BindingAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }

        public string Help { get; set; }
    }
}
