using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal sealed class DyMetaTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.TypeInfo);

    public override int ReflectedTypeId => Dy.TypeInfo;

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var ret = ctx.RuntimeContext.Types[((DyTypeInfo)arg).ReflectedTypeId].GetStaticMember(Builtins.String, ctx);

        if (ctx.HasErrors || ret is null)
            return Nil;

        return ret.Invoke(ctx);
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var ret = ctx.RuntimeContext.Types[((DyTypeInfo)arg).ReflectedTypeId].GetStaticMember(Builtins.Length, ctx);

        if (ctx.HasErrors || ret is null)
            return Nil;

        return ret.Invoke(ctx);
    }
}
