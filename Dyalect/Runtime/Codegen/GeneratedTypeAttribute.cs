﻿using System;
namespace Dyalect.Runtime.Codegen;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class GeneratedTypeAttribute : Attribute
{
}
