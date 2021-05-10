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

            if (self.TypeId == right.TypeId && right is DyCustomType t && t.Constructor == self.Constructor
                && t.Locals.Length == self.Locals.Length)
            {
                for (var i = 0; i < self.Locals.Length; i++)
                {
                    var res = ctx.RuntimeContext.Types[self.Locals[i].TypeId].Eq(ctx, self.Locals[i], t.Locals[i]);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    if (ReferenceEquals(res, DyBool.False))
                        return res;
                }

                return DyBool.True;
            }

            return DyBool.False;
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var cust = (DyCustomType)arg;
            if (TypeName == cust.Constructor && cust.Locals.Length == 0)
                return new DyString($"{TypeName}()");
            else if (TypeName == cust.Constructor)
                return new DyString($"{TypeName}({LocalsToString(cust, ctx)})");
            else if (cust.Locals.Length == 0)
                return new DyString($"{TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{TypeName}.{cust.Constructor}({LocalsToString(cust, ctx)})");
        }

        private string LocalsToString(DyCustomType t, ExecutionContext ctx)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < t.Locals.Length; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                sb.Append(t.Locals[i].GetLabel());
                sb.Append(':');
                sb.Append(' ');
                sb.Append(t.Locals[i].GetTaggedValue().ToString(ctx));

                if (ctx.HasErrors)
                    return "";
            }

            return sb.ToString();
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            if (autoGenMethods)
                return DyInteger.Get(((DyCustomType)arg).Locals.Length);

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

        public override string TypeName { get; }
    }
}
