using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyCustomTypeInfo : DyTypeInfo
    {
        private readonly bool autoGenMethods;

        public DyCustomTypeInfo(int typeCode, string typeName, bool autoGenMethods) : base(typeCode)
        {
            TypeName = typeName;
            this.autoGenMethods = autoGenMethods;
        }

        protected override SupportedOperations GetSupportedOperations() =>
            (SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not)
            | (autoGenMethods ? (SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len)
                : SupportedOperations.None);

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var self = (DyCustomType)left;

            if (self.TypeId == right.TypeId && right is DyCustomType t && t.Constructor == self.Constructor)
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
            var cust = (DyCustomType)arg;
            if (TypeName == cust.Constructor && cust.Privates.Count == 0)
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}{ctx.RuntimeContext.Types[cust.Privates.TypeId].ToString(ctx)}");
            else if (cust.Privates.Count == 0)
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}{ctx.RuntimeContext.Types[cust.Privates.TypeId].ToString(ctx)}");
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return DyInteger.Get(((DyCustomType)arg).Privates.Count);

            return base.LengthOp(arg, ctx);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return self.GetItem(index, ctx);

            return base.GetOp(self, index, ctx);
        }

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (autoGenMethods)
            {
                self.SetItem(index, value, ctx);
                return DyNil.Instance;
            }

            return base.SetOp(self, index, value, ctx);
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (name == "$privates")
                return ((DyCustomType)self).Privates;

            return base.InitializeInstanceMember(self, name, ctx);
        }

        public override string TypeName { get; }
    }
}
