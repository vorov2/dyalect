using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Types
{
    public sealed class DyResult : DyForeignObject<DyResultTypeInfo>
    {
        internal readonly DyObject Value;
        internal readonly string Constructor;

        public DyResult(RuntimeContext rtx, string ctor, DyObject value) 
            : this(GetTypeId(rtx), ctor, value) { }

        public DyResult(int typeId, string ctor, DyObject value) : base(typeId) => 
            (Constructor, Value) = (ctor, value);

        public override object ToObject() => this;

        public override int GetConstructorId(ExecutionContext ctx)
        {
            if (ctx.RuntimeContext.QueryMemberId(Constructor, out var id))
                return id;

            return base.GetConstructorId(ctx);
        }
    }

    [ForeignType("221D6334-76F1-49C3-A46E-B9A361D82849", "Success", "Failure")]
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
                return (long)index.ToObject() == 0 ? ((DyResult)self).Value : ctx.IndexOutOfRange(index);
            else if (index.TypeId == DyType.String)
            {
                var str = index.ToString();
                var s = (DyResult)self;
                return str == "value" && s.Constructor == "Success" ? s.Value
                    : str == "detail" && s.Constructor == "Failure" ? s.Value
                    : ctx.IndexOutOfRange(index);
            }

            return ctx.IndexInvalidType(index);
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

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "unbox")
                return DyForeignFunction.Member(name, TryGet);
            return base.GetMember(name, ctx);
        }

        private DyObject Success(ExecutionContext _, DyObject arg) => new DyResult(TypeCode, "Success", arg);

        private DyObject Failure(ExecutionContext _, DyObject arg) => new DyResult(TypeCode, "Failure", arg);

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Success")
                return DyForeignFunction.Static(name, Success, -1, new Par("arg", DyNil.Instance));
            if (name == "Failure")
                return DyForeignFunction.Static(name, Failure, -1, new Par("arg", DyNil.Instance));

            return base.GetStaticMember(name, ctx);
        }
    }
}
