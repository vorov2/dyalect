using System;
namespace Dyalect.Codegen;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class VarArgAttribute : Attribute { }
