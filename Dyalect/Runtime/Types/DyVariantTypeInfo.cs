using Dyalect.Debug;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

internal sealed class DyVariantTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Variant);

    public override int ReflectedTypeId => Dy.Variant;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Len | SupportedOperations.Get | SupportedOperations.Set;

    public DyVariantTypeInfo() => AddMixins(Dy.Lookup);

    #region Operations
    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return False;

        var (xs, ys) = ((DyVariant)left, (DyVariant)right);

        if (xs.Constructor != ys.Constructor)
            return False;

        return xs.Fields.Equals(ys.Fields, ctx) ? True : False;
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        DyInteger.Get(((DyVariant)arg).Fields.Count);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        var self = (DyVariant)arg;

        IEnumerable<DyObject> Iterate()
        {
            var xs = self.Fields.UnsafeAccess();
            for (var i = 0; i < self.Fields.Count; i++)
                yield return xs[i];
        }

        try
        {
            return new DyString("@" + self.Constructor + "(" + Iterate().ToLiteral(ctx) + ")");
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) =>
        ((DyVariant)self).Fields.GetItem(ctx, index);

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) =>
        ctx.RuntimeContext.Tuple.Set(ctx, ((DyVariant)self).Fields, index, value);

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => self is DyVariant v && v.Fields.Count > 0 ? v.Fields : Nil,
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
    {
        if (!char.IsUpper(name[0]))
            return base.InitializeStaticMember(name, ctx);

        return new DyVariantConstructor(name, (_, args) => new DyVariant(name, args), new("values", ParKind.VarArg));
    }
}
