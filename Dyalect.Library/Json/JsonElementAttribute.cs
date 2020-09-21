using System;

namespace Dyalect.Library.Json
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class JsonElementAttribute : Attribute
    {
        public JsonElementAttribute(string elementName)
        {
            ElementName = elementName;
        }

        public string ElementName { get; }

        public override string ToString()
        {
            return ElementName;
        }
    }
}