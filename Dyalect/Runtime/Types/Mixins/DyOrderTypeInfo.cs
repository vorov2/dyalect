using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyOrderTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Order);

    public override int ReflectedTypeId => Dy.Order;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte;

    public DyOrderTypeInfo()
    {
        Members.Add(Builtins.Gt, Binary(Builtins.Gt, Greater));
        Members.Add(Builtins.Lt, Binary(Builtins.Lt, Lesser));
    }

    private static DyObject Greater(ExecutionContext ctx, DyObject left, DyObject right)
    {
        try
        {
            return DyTuple.Greater(ctx, ((DyClass)left).Fields, ((DyClass)right).Fields);
        }
        catch(DyCodeException e)
        {
            ctx.Error = e.Error;
            return Nil;
        }
    }

    private static DyObject Lesser(ExecutionContext ctx, DyObject left, DyObject right)
    {
        try
        {
            return DyTuple.Lesser(ctx, ((DyClass)left).Fields, ((DyClass)right).Fields);
        }
        catch (DyCodeException e)
        {
            ctx.Error = e.Error;
            return Nil;
        }
    }
}
