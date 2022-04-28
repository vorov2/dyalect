using System;
namespace Dyalect.Codegen;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class InstancePropertyAttribute : MethodAttribute
{
    public InstancePropertyAttribute(string _) { }
    public InstancePropertyAttribute() { }
}
