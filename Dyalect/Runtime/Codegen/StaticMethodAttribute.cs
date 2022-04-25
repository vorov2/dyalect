using System;
namespace Dyalect.Runtime.Codegen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class StaticMethodAttribute : MethodAttribute
{
    public StaticMethodAttribute(string _) { }
    public StaticMethodAttribute() { }
}
