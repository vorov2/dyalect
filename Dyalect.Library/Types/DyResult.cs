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
            return new DyString(self.Constructor + " (" 
                + (self.Value.TypeId is not DyType.Nil ? self.Value.ToString(ctx) : "") + ")");
        }

        public override string TypeName => "Result";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var self = (DyResult)arg;
            return DyInteger.Get(self.Value.TypeId is DyType.Nil ? 0 : 1);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.Integer)
                return (long)index.ToObject() is 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange();
            
            if (index.TypeId is DyType.String)
            {
                var str = index.ToString();
                var s = (DyResult)self;
                return str is "value" && s.Constructor is "Success" ? s.Value
                    : str is "detail" && s.Constructor is "Failure" ? s.Value
                    : ctx.IndexOutOfRange();
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

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (name is "value")
                return Func.Member(name, TryGet);
            return base.InitializeInstanceMember(self, name, ctx);
        }

        private DyObject Success(ExecutionContext ctx, DyObject arg) =>
            new DyResult(ctx.RuntimeContext, DeclaringUnit, "Success", arg);

        private DyObject Failure(ExecutionContext ctx, DyObject arg) =>
            new DyResult(ctx.RuntimeContext, DeclaringUnit, "Failure", arg);

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Success")
                return Func.Static(name, Success, -1, new Par("arg", DyNil.Instance));
            if (name == "Failure")
                return Func.Static(name, Failure, -1, new Par("arg", DyNil.Instance));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
