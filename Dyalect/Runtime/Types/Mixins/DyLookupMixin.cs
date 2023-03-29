using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyLookupMixin : DyMixin<DyLookupMixin>
{
    public DyLookupMixin() : base(Dy.Lookup)
    {
        Members.Add(Builtins.Length, Unary(Builtins.Length, GetLength));
        Members.Add(Builtins.Get, Binary(Builtins.Get, Getter, "index"));
        SetSupportedOperations(Ops.Get | Ops.Len);
    }

    public static DyObject GetLength(ExecutionContext ctx, DyObject self) =>
        DyInteger.Get(((DyClass)self).Fields.Count);

    public static DyObject Getter(ExecutionContext ctx, DyObject self, DyObject index) =>
        ((DyClass)self).Fields.GetItem(ctx, index);
}
