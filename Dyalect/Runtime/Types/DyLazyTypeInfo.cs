namespace Dyalect.Runtime.Types
{
    public sealed class DyLazyTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Lazy;

        public override int ReflectedTypeId => DyType.Lazy;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyTypeInfo? Force(DyObject obj, ExecutionContext ctx)
        {
            var o = obj.Force(ctx);

            if (o is null || ctx.HasErrors)
                return null;

            return ctx.RuntimeContext.Types[o.TypeId];
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Add(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Sub(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Mul(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Div(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Rem(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftLeft(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.ShiftRight(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.And(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Or(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Xor(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Eq(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Neq(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Gt(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Lt(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Gte(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx) => Force(left, ctx)?.Lte(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.Neg(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.Plus(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.Not(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.BitwiseNot(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.Length(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => Force(arg, ctx)?.ToString(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => Force(self, ctx)?.Get(ctx, self.Force(ctx), index.Force(ctx)) ?? DyNil.Instance;
        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) => Force(self, ctx)?.Set(ctx, self.Force(ctx), index.Force(ctx), value) ?? DyNil.Instance;
        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) => Force(self, ctx)?.Cast(ctx, self, targetType.Force(ctx)) ?? DyNil.Instance;

        internal override DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx) => Force(self, ctx)?.GetInstanceMember(self.Force(ctx), name, ctx) ?? DyNil.Instance;

        internal override bool HasInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var t = Force(self, ctx);
            return t is not null && t.HasInstanceMember(self, name, ctx);
        }
    }
}