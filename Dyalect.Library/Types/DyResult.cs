using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Types
{
    public sealed class DyResult : DyForeignObject<DyResultTypeInfo>
    {
        internal readonly DyObject Value;

        public DyResult(RuntimeContext rtx, Unit unit, string ctor, DyObject value) : base(rtx, unit, ctor) =>
            Value = value;

        public override object ToObject() => this;
    }

    public sealed class DyResultTypeInfo : ForeignTypeInfo
    {
        public DyResultTypeInfo() { }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyResult)arg;
            return new DyString(self.Constructor + "(" 
                + (self.Value.TypeId != DyType.Nil ? self.Value.ToString(ctx) : "") + ")");
        }

        public override string TypeName => "Result";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyResult)arg;
            return DyInteger.Get(self.Value.TypeId == DyType.Nil ? 0 : 1);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return (long)index.ToObject() == 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange();
            else if (index.TypeId == DyType.String)
            {
                var str = index.ToString();
                var s = (DyResult)self;
                return str == "value" && s.Constructor == "Success" ? s.Value
                    : str == "detail" && s.Constructor == "Failure" ? s.Value
                    : ctx.IndexOutOfRange();
            }

            return ctx.InvalidType(index);
        }

        private DyObject TryGet(ExecutionContext ctx, DyObject self)
        {
            var s = (DyResult)self;
            
            if (s.Constructor != "Failure")
                return s.Value;
            else
            {
                var newctx = ctx.Clone();
                var res = s.Value.ToString(newctx);
                return newctx.HasErrors ? ctx.Fail(s.Value.ToString()) : ctx.Fail(res.ToString());
            }
        }

        protected override DyObject InitializeInstanceMember(string name, ExecutionContext ctx)
        {
            if (name == "value")
                return DyForeignFunction.Member(name, TryGet);
            return base.InitializeInstanceMember(name, ctx);
        }

        private DyObject Success(ExecutionContext ctx, DyObject arg) =>
            new DyResult(ctx.RuntimeContext, DeclaringUnit, "Success", arg);

        private DyObject Failure(ExecutionContext ctx, DyObject arg) =>
            new DyResult(ctx.RuntimeContext, DeclaringUnit, "Failure", arg);

        protected override DyObject InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Success")
                return DyForeignFunction.Static(name, Success, -1, new Par("arg", DyNil.Instance));
            if (name == "Failure")
                return DyForeignFunction.Static(name, Failure, -1, new Par("arg", DyNil.Instance));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
