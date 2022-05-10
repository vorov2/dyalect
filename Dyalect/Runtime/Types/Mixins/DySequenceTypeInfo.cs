using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DySequenceTypeInfo : DyMixin<DySequenceTypeInfo>
{
    public DySequenceTypeInfo() : base(Dy.Sequence)
    {
        Members.Add(Builtins.Iterate, Unary(Builtins.Iterate, Iterate));
        SetSupportedOperations(Ops.Iter);
    }

    private static DyObject Iterate(ExecutionContext ctx, DyObject self) =>
        DyIterator.Create(((DyClass)self).Fields);
}
