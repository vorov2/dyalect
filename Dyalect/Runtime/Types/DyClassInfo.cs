namespace Dyalect.Runtime.Types
{
    internal sealed class DyClassInfo : DyTypeInfo
    {
        private readonly bool privateCons;

        public override string TypeName { get; }

        public DyClassInfo(DyTypeInfo typeInfo, string typeName) : base(typeInfo, DyTypeCode.Class)
        {
            TypeName = typeName;
            privateCons = !string.IsNullOrEmpty(typeName) && typeName.Length > 0 && char.IsLower(typeName[0]);
        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyClass)left;

            if (self.DecType.TypeCode == right.DecType.TypeCode && right is DyClass t && t.Constructor == self.Constructor)
            {
                var res = self.Fields.DecType.Eq(ctx, self.Fields, t.Fields);

                if (ctx.HasErrors)
                    return ctx.RuntimeContext.Nil.Instance;

                return res;
            }

            return ctx.RuntimeContext.Bool.False;
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyClass)arg;
            var priv = cust.Fields;

            if (TypeName == cust.Constructor && priv.Count == 0)
                return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, $"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, $"{TypeName}{priv.ToString(ctx)}");
            else if (priv.Count == 0)
                return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, $"{TypeName}.{cust.Constructor}()");
            else
                return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, $"{TypeName}.{cust.Constructor}{priv.ToString(ctx)}");
        }

        protected override DyObject LengthOp(DyObject self, ExecutionContext ctx)
        {
            var cls = (DyClass)self;

            if (privateCons && ctx.UnitId != cls.DeclaringUnit.Id)
                return base.LengthOp(self, ctx);

            return ctx.RuntimeContext.Integer.Get(cls.Fields.Count);
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
            return ctx.RuntimeContext.Nil.Instance;
        }
    }
}
