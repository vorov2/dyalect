namespace Dyalect.Runtime.Types
{
    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public DyNilTypeInfo() : base(DyType.Nil) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Nil;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId ? DyBool.True : DyBool.False;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Nil")
                return Func.Static(name, _ => DyNil.Instance);

            if (name == "default")
                return Func.Static(name, _ => DyNil.Instance);

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
