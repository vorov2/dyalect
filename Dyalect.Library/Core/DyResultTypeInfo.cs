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
    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        var self = (DyResult)arg;
        return new DyString(self.Constructor + " ("
            + (self.Value.TypeId is not DyType.Nil ? self.Value.ToString(ctx) : "") + ")");
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var self = (DyResult)arg;
        return DyInteger.Get(self.Value.TypeId is DyType.Nil ? 0 : 1);
    }

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId is DyType.Integer)
            return index.GetInteger() is 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange(index);

        if (index.TypeId is DyType.String)
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

    [InstanceMethod("Value")]
    internal static DyObject TryGet(ExecutionContext ctx, DyResult self)
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
