namespace Dyalect.Runtime.Types
{
    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public DyNil Instance => new(this);
        
        internal DyNil Terminator => new DyNil.Terminator(this);

        public DyNilTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Nil) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Nil;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.DecType.TypeCode == right.DecType.TypeCode ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => ctx.RuntimeContext.Bool.True;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, "nil");

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name is "Nil" or "default")
                return Func.Static(ctx, name, _ => Instance);

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
