﻿using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyContainerMixin : DyMixin<DyContainerMixin>
{
    public DyContainerMixin() : base(Dy.Container)
    {
        Members.Add(Builtins.In, Binary(Builtins.In, IsIn, "value"));
        SetSupportedOperations(Ops.In);
    }

    private static DyObject IsIn(ExecutionContext _, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return False;

        return ((DyClass)self).Fields.GetOrdinal(field.ToString()) is not -1 ? True : False;
    }
}
