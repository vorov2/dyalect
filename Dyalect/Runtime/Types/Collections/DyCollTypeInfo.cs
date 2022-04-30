﻿using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyCollTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Collection);

    public override int ReflectedTypeId => Dy.Collection;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    public DyCollTypeInfo()
    {
        Closed = true;
        AddDefaultMixin2(Builtins.Get, "index");
        AddDefaultMixin1(Builtins.Length);
    }
}
