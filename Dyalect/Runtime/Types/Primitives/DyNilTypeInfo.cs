namespace Dyalect.Runtime.Types
{
    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Nil;

        public override int ReflectedTypeId => DyType.Nil;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId ? DyBool.True : DyBool.False;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name is "Nil" or "Default")
                return Func.Static(name, _ => DyNil.Instance);

            return base.InitializeStaticMember(name, ctx);
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Bool => DyBool.False,
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
