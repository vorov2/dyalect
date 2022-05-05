using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyLookupTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Lookup);

    public override int ReflectedTypeId => Dy.Lookup;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.Get | SupportedOperations.Len;

    public DyLookupTypeInfo()
    {
        Members.Add(Builtins.Length, Unary(Builtins.Length, LengthOp));
        Members.Add(Builtins.Get, Binary(Builtins.Get, GetOp, "index"));
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject self) =>
        DyInteger.Get(((DyClass)self).Fields.Count);

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) =>
        ((DyClass)self).Fields.GetItem(ctx, index);
}
