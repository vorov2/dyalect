namespace Dyalect.Runtime.Types
{
    public sealed class DyLazyTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Lazy;

        public override int ReflectedTypeCode => DyType.Lazy;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyTypeInfo? Force(DyObject obj, ExecutionContext ctx)
        {
            var o = obj.Force(ctx);

            if (o is null || ctx.HasErrors)
                return null;

            return ctx.RuntimeContext.Types[o.TypeId];
        }

        internal protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.AddOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.SubOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.MulOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.DivOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.RemOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftLeftOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftRightOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.AndOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.OrOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.XorOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.EqOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.NeqOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.GtOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.LtOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.GteOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.LteOp(left.Force(ctx), right.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.NegOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.PlusOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.NotOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.BitwiseNotOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.LengthOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.ToStringOp(arg.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => Force(self, ctx)?.GetOp(self.Force(ctx), index.Force(ctx), ctx) ?? DyNil.Instance;
        internal protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) => Force(self, ctx)?.SetOp(self.Force(ctx), index.Force(ctx), value, ctx) ?? DyNil.Instance;
        internal override DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx) => Force(self, ctx)?.GetInstanceMember(self.Force(ctx), name, ctx) ?? DyNil.Instance;

        internal override bool HasInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var t = Force(self, ctx);
            return t is not null && t.HasInstanceMember(self, name, ctx);
        }
    }
}