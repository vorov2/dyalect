using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyResultTypeInfo : DyForeignTypeInfo
{
    private const string SUCCESS = "Success";
    private const string FAILURE = "Failure";

    public override string ReflectedTypeName => "Result";

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Len;

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var self = (DyResult)arg;
        return new DyString(self.Constructor + " ("
            + (self.Value.TypeId is not Dy.Nil ? self.Value.ToString(ctx) : "") + ")");
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var self = (DyResult)arg;
        return DyInteger.Get(self.Value.TypeId is Dy.Nil ? 0 : 1);
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (index.TypeId is Dy.Integer)
            return index.GetInteger() is 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange(index);

        if (index.TypeId is Dy.String)
        {
            var str = index.GetString();
            var s = (DyResult)self;
            return str is "value" && s.Constructor is SUCCESS ? s.Value
                : str is "detail" && s.Constructor is FAILURE ? s.Value
                : ctx.IndexOutOfRange(str);
        }

        return ctx.InvalidType(index);
    }
    #endregion

    [InstanceMethod]
    internal static DyObject GetValue(ExecutionContext ctx, DyResult self)
    {
        if (self.Constructor is not FAILURE)
            return self.Value;

        var newctx = ctx.Clone();
        var res = self.Value.ToString(newctx);
        return newctx.HasErrors ? ctx.Failure(self.Value.ToString()) : ctx.Failure(res.ToString());
    }

    [StaticMethod(SUCCESS)]
    internal static DyObject Success(ExecutionContext ctx, DyObject arg) => new DyResult(ctx.Type<DyResultTypeInfo>(), SUCCESS, arg);

    [StaticMethod(FAILURE)]
    internal static DyObject Failure(ExecutionContext ctx, DyObject arg) => new DyResult(ctx.Type<DyResultTypeInfo>(), FAILURE, arg);
}
