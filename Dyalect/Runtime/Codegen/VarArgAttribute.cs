using System;
namespace Dyalect.Runtime.Codegen;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class VarArgAttribute : Attribute { }
