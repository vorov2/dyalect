namespace Dyalect.Runtime.Types
{
    internal sealed class DyClassInfo : DyTypeInfo
    {
        private readonly bool privateCons;

        public override string TypeName { get; }

        public override int ReflectedTypeCode { get; }

        public DyClassInfo(string typeName, int typeCode)
        {
            TypeName = typeName;
            ReflectedTypeCode = typeCode;
            privateCons = !string.IsNullOrEmpty(typeName) && typeName.Length > 0 && char.IsLower(typeName[0]);
        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyClass)left;

            if (self.TypeId == right.TypeId && right is DyClass t && t.Constructor == self.Constructor)
            {
                var res = ctx.RuntimeContext.Types[self.Fields.TypeId].Eq(ctx, self.Fields, t.Fields);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                return res;
            }

            return DyBool.False;
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyClass)arg;
            var priv = cust.Fields;

            if (TypeName == cust.Constructor && priv.Count == 0)
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}{priv.ToString(ctx)}");
            else if (priv.Count == 0)
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}{priv.ToString(ctx)}");
        }

        protected override DyObject LengthOp(DyObject self, ExecutionContext ctx)
        {
            var cls = (DyClass)self;

            if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
                return base.LengthOp(self, ctx);

            return DyInteger.Get(cls.Fields.Count);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            var cls = (DyClass)self;

            if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
                return base.GetOp(self, index, ctx);

            return cls.Fields.GetItem(index, ctx);
        }

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            var cls = (DyClass)self;

            if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
                return base.SetOp(self, index, value, ctx);

            cls.Fields.SetItem(index, value, ctx);
            return DyNil.Instance;
        }
    }
}
