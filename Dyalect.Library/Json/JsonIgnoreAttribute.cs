using System;

namespace Dyalect.Library.Json
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class JsonIgnoreAttribute : Attribute
    {
        
    }
}