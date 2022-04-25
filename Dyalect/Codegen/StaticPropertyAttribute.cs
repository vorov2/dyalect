using System;
namespace Dyalect.Codegen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class StaticPropertyAttribute : MethodAttribute
{
    public StaticPropertyAttribute(string _) { }
    public StaticPropertyAttribute() { }
}
