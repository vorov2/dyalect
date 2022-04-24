﻿namespace Dyalect.Runtime.Types;

internal sealed class DyObjectTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string ReflectedTypeName => DyTypeNames.Object;

    public override int ReflectedTypeId => DyType.Object;

    public DyObjectTypeInfo() => Closed = true;
}
