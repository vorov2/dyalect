using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyCollectionTypeInfo : DyMixin<DyCollectionTypeInfo>
{
    public DyCollectionTypeInfo() : base(Dy.Collection)
    {
        AddMixins(Dy.Lookup);
        Members.Add(Builtins.Length, Unary(Builtins.Length, DyLookupTypeInfo.GetLength));
        Members.Add(Builtins.Get, Binary(Builtins.Get, DyLookupTypeInfo.Getter, "index"));
        Members.Add(Builtins.Set, Ternary(Builtins.Set, Setter, "index", "value"));
        SetSupportedOperations(Ops.Get | Ops.Len | Ops.Set);
    }

    private static DyObject Setter(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyClass)self).Fields.SetItem(ctx, index, value);
        return Nil;
    }
}
