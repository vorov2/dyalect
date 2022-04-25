using System;
namespace Dyalect.Runtime.Codegen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class StaticPropertyAttribute : MethodAttribute
{
    public StaticPropertyAttribute(string _) { }
    public StaticPropertyAttribute() { }
}
