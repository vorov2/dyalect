using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Types
{
    public sealed class DyResultTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "Result";

        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyResult)arg;
            return new DyString(self.Constructor + " ("
                + (self.Value.TypeId is not DyType.Nil ? self.Value.ToString(ctx) : "") + ")");
        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len;

        internal protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyResult)arg;
            return DyInteger.Get(self.Value.TypeId is DyType.Nil ? 0 : 1);
        }

        internal protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.Integer)
                return (long)index.ToObject() is 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange(index);

            if (index.TypeId is DyType.String)
            {
                var str = index.GetString();
                var s = (DyResult)self;
                return str is "value" && s.Constructor is "Success" ? s.Value
                    : str is "detail" && s.Constructor is "Failure" ? s.Value
                    : ctx.IndexOutOfRange(str);
            }

            return ctx.InvalidType(index);
        }

        private DyObject TryGet(ExecutionContext ctx, DyObject self)
        {
            var s = (DyResult)self;

            if (s.Constructor is not "Failure")
                return s.Value;

            var newctx = ctx.Clone();
            var res = s.Value.ToString(newctx);
            return newctx.HasErrors ? ctx.Fail(s.Value.ToString()) : ctx.Fail(res.ToString());
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Value" => Func.Member(name, TryGet),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Success(ExecutionContext ctx, DyObject arg) => new DyResult(this, "Success", arg);

        private DyObject Failure(ExecutionContext ctx, DyObject arg) => new DyResult(this, "Failure", arg);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Success")
                return Func.Static(name, Success, -1, new Par("arg", DyNil.Instance));
            if (name == "Failure")
                return Func.Static(name, Failure, -1, new Par("arg", DyNil.Instance));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
