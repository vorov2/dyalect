﻿using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DySequenceTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Sequence);

    public override int ReflectedTypeId => Dy.Sequence;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.Iter; 

    public DySequenceTypeInfo()
    {
        Members.Add(Builtins.Iterate, Unary(Builtins.Iterate, Iterate));
    }

    private static DyObject Iterate(ExecutionContext ctx, DyObject self) =>
        DyIterator.Create(((DyClass)self).Fields);
}
