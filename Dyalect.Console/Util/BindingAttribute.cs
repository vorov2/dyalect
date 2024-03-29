﻿using System;

namespace Dyalect.Util;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
public sealed class BindingAttribute : Attribute
{
    public string[] Names { get; }

    public string? Help { get; set; }

    public string? Category { get; set; }

    public BindingAttribute(params string[] names) => Names = names;
}
