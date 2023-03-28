using Dyalect.Compiler;

namespace Dyalect.Runtime.Types;

internal sealed class DyCollectionMixin : DyMixin<DyCollectionMixin>
{
    public DyCollectionMixin() : base(Dy.Collection)
    {
        AddMixins(Dy.Lookup);
        Members.Add(Builtins.Length, Unary(Builtins.Length, DyLookupMixin.GetLength));
        Members.Add(Builtins.Get, Binary(Builtins.Get, DyLookupMixin.Getter, "index"));
        Members.Add(Builtins.Set, Ternary(Builtins.Set, Setter, "index", "value"));
        SetSupportedOperations(Ops.Set);
    }

    private static DyObject Setter(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyClass)self).Fields.SetItem(ctx, index, value);
        return Nil;
    }
}
