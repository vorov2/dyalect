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

        public override DyObject Add(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Add(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Sub(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Mul(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Div(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Div(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Rem(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.ShiftLeft(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.ShiftRight(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject And(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.And(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Or(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Or(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Xor(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Xor(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Eq(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Eq(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Neq(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Neq(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Gt(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Lt(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Lt(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Gte(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Gte(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Lte(ExecutionContext ctx, DyObject left, DyObject right) => Force(left, ctx)?.Lte(ctx, left.Force(ctx), right.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Neg(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.Neg(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Plus(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.Plus(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Not(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.Not(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject BitwiseNot(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.BitwiseNot(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Length(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.Length(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject ToString(ExecutionContext ctx, DyObject arg) => Force(arg, ctx)?.ToString(ctx, arg.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Get(ExecutionContext ctx, DyObject self, DyObject index) => Force(self, ctx)?.Get(ctx, self.Force(ctx), index.Force(ctx)) ?? DyNil.Instance;
        public override DyObject Set(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) => Force(self, ctx)?.Set(ctx, self.Force(ctx), index.Force(ctx), value) ?? DyNil.Instance;
        public override DyObject Cast(ExecutionContext ctx, DyObject self, DyObject targetType) => Force(self, ctx)?.Cast(ctx, self, targetType.Force(ctx)) ?? DyNil.Instance;

        internal override DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx) => Force(self, ctx)?.GetInstanceMember(self.Force(ctx), name, ctx) ?? DyNil.Instance;

        internal override bool HasInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var t = Force(self, ctx);
            return t is not null && t.HasInstanceMember(self, name, ctx);
        }
    }
}