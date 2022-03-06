using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        public DyBool True => new DyBool.True(this);
        public DyBool False => new DyBool.False(this);

        public DyBoolTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Bool) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Bool;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.GetBool() == right.GetBool() ? True : False;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, ReferenceEquals(arg, True) ? "true" : "false");

        private DyObject Convert(ExecutionContext ctx, DyObject val) => val.GetBool() ? True : False;

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name is "Bool")
                return Func.Static(ctx, name, Convert, -1, new Par("value"));

            if (name is "default")
                return Func.Static(ctx, name, _ => False);

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
