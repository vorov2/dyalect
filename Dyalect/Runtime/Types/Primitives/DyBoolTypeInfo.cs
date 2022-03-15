using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Lit;

        public override string TypeName => DyTypeNames.Bool;

        public override int ReflectedTypeId => DyType.Bool;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.GetBool(ctx) == right.GetBool(ctx) ? DyBool.True : DyBool.False;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(ReferenceEquals(arg, DyBool.True) ? "true" : "false");

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, ctx);

        private DyObject Convert(ExecutionContext ctx, DyObject val) => val.GetBool(ctx) ? DyBool.True : DyBool.False;

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Bool => Func.Static(name, Convert, -1, new Par("value")),
                Method.Default => Func.Static(name, _ => DyBool.False),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
