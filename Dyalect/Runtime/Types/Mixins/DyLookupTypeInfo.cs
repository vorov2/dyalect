using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyLookupTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Lookup);

    public override int ReflectedTypeId => Dy.Lookup;

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.Get | SupportedOperations.Len;

    public DyLookupTypeInfo()
    {
        Members.Add(Builtins.Length, Unary(Builtins.Length, GetLength));
        Members.Add(Builtins.Get, Binary(Builtins.Get, Getter, "index"));
        Members.Add(Builtins.In, Binary(Builtins.In, IsIn, "value"));
    }

    private static DyObject GetLength(ExecutionContext ctx, DyObject self) =>
        DyInteger.Get(((DyClass)self).Fields.Count);

    private static DyObject Getter(ExecutionContext ctx, DyObject self, DyObject index) =>
        ((DyClass)self).Fields.GetItem(ctx, index);

    private static DyObject IsIn(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return False;

        return ((DyClass)self).Fields.GetOrdinal(field.ToString()) is not -1 ? True : False;
    }
}
