using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    public sealed class DyVariantTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Variant;

        public override int ReflectedTypeId => DyType.Variant;

        internal DyVariantTypeInfo() { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Lit;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId || left.GetConstructor(ctx) != right.GetConstructor(ctx))
                return DyBool.False;

            var (xs, ys) = ((DyVariant)left, (DyVariant)right);
            return ctx.RuntimeContext.Tuple.Eq(ctx, xs.Tuple, ys.Tuple);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyVariant)arg).Tuple.Count);

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyVariant)arg;
            var str = ctx.RuntimeContext.Tuple.ToStringDirect(ctx, self.Tuple);

            if (ctx.HasErrors)
                return DyNil.Instance;

            return new DyString($"{TypeName}.{self.Constructor}{str}");
        }

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyVariant)arg;
            var str = ctx.RuntimeContext.Tuple.ToLiteralDirect(ctx, self.Tuple);

            if (ctx.HasErrors)
                return DyNil.Instance;

            return new DyString($"@{self.Constructor}{str}");
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) =>
            ctx.RuntimeContext.Tuple.GetDirect(ctx, ((DyVariant)self).Tuple, index);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.RuntimeContext.Tuple.Set(ctx, ((DyVariant)self).Tuple, index, value);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (!char.IsUpper(name[0]))
                return base.InitializeStaticMember(name, ctx);

            return Func.Static(name, (_, args) => new DyVariant(name, (DyTuple)args), 0, new Par("values"));
        }

        private DyObject GetTuple(ExecutionContext ctx, DyObject self)
        {
            var v = (DyVariant)self;

            if (v.Tuple.Count == 0)
                return ctx.InvalidCast(TypeName, DyTypeNames.Tuple);

            return v.Tuple;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Tuple => GetTuple(ctx, self),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
