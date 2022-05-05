using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyCollectionTypeInfo : DyLookupTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Collection);

    public override int ReflectedTypeId => Dy.Collection;

    protected override SupportedOperations GetSupportedOperations() =>
        base.GetSupportedOperations() | SupportedOperations.Set;

    public DyCollectionTypeInfo()
    {
        AddMixin(Dy.Lookup);
        Members.Add(Builtins.Set, Ternary(Builtins.Set, SetOp, "index", "value"));
    }

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyClass)self).Fields.SetItem(ctx, index, value);
        return Nil;
    }
}
