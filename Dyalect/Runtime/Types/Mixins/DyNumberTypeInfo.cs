using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyNumberTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Number);

    public override int ReflectedTypeId => Dy.Number;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Add | SupportedOperations.Sub | SupportedOperations.Div
        | SupportedOperations.Mul | SupportedOperations.Rem | SupportedOperations.Neg
        | SupportedOperations.Plus;

    public DyNumberTypeInfo()
    {
        Members.Add(Builtins.Add, Binary(Builtins.Add, Sum));
        Members.Add(Builtins.Sub, Binary(Builtins.Sub, Subtract));
        Members.Add(Builtins.Mul, Binary(Builtins.Mul, Multiply));
        Members.Add(Builtins.Div, Binary(Builtins.Div, Divide));
        Members.Add(Builtins.Rem, Binary(Builtins.Rem, Remainder));
        Members.Add(Builtins.Neg, Unary(Builtins.Neg, Negate));
        Members.Add(Builtins.Plus, Unary(Builtins.Plus, MakePlus));
    }

    private static DyObject Sum(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.NotImplemented(Builtins.Add);

    private static DyObject Subtract(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.NotImplemented(Builtins.Sub);

    private static DyObject Multiply(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.NotImplemented(Builtins.Mul);

    private static DyObject Divide(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.NotImplemented(Builtins.Div);

    private static DyObject Remainder(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.NotImplemented(Builtins.Rem);

    private static DyObject Negate(ExecutionContext ctx, DyObject left) =>
        ctx.NotImplemented(Builtins.Neg);

    private static DyObject MakePlus(ExecutionContext ctx, DyObject left) =>
        ctx.NotImplemented(Builtins.Plus);
}
