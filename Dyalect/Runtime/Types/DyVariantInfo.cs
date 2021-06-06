namespace Dyalect.Runtime.Types
{
    internal sealed class DyVariantInfo : DyTypeInfo
    {
        public override string TypeName { get; }

        public DyVariantInfo(int typeCode, string typeName) : base(typeCode) => TypeName = typeName;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyClass)left;

            if (self.TypeId == right.TypeId && right is DyClass t && t.Constructor == self.Constructor)
            {
                var res = ctx.RuntimeContext.Types[self.Privates.TypeId].Eq(ctx, self.Privates, t.Privates);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                return res;
            }

            return DyBool.False;
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyClass)arg;
            var priv = (DyTuple)cust.Privates;

            if (TypeName == cust.Constructor && priv.Count == 0)
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}{priv.ToString(ctx)}");
            else if (priv.Count == 0)
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}{priv.ToString(ctx)}");
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => 
            DyInteger.Get(((DyTuple)((DyClass)arg).Privates).Count);

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => ((DyClass)self).Privates.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            ((DyClass)self).Privates.SetItem(index, value, ctx);
            return DyNil.Instance;
        }
    }
}
