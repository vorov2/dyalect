using System;
namespace Dyalect.Runtime.Codegen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class InstanceMethodAttribute : MethodAttribute
{
    public InstanceMethodAttribute(string _) { }
    public InstanceMethodAttribute() { }
}
