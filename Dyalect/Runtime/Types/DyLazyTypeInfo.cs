namespace Dyalect.Runtime.Types
{
    public sealed class DyLazyTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Lazy;

        public override int ReflectedTypeCode => DyType.Lazy;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyTypeInfo? Force(DyObject obj, ExecutionContext ctx)
        {
            obj = obj.Force(ctx);

            if (ctx.HasErrors)
                return null;

            return ctx.RuntimeContext.Types[obj.TypeId];
        }

        internal protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.AddOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.SubOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.MulOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.DivOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.RemOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftLeftOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftRightOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.AndOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.OrOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.XorOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.EqOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.NeqOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.GtOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.LtOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.GteOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.LteOp(left, right, ctx) ?? DyNil.Instance;
        internal protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.NegOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.PlusOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.NotOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.BitwiseNotOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.LengthOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.ToStringOp(arg, ctx) ?? DyNil.Instance;
        internal protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => Force(self, ctx)?.GetOp(self, index, ctx) ?? DyNil.Instance;
        internal protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) => Force(self, ctx)?.SetOp(self, index, value, ctx) ?? DyNil.Instance;
        internal override DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx) => Force(self, ctx)?.GetInstanceMember(self, name, ctx) ?? DyNil.Instance;

        internal override bool HasInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var t = Force(self, ctx);
            return t is not null && t.HasInstanceMember(self, name, ctx);
        }
    }
}