namespace Dyalect.Runtime.Types
{
    internal sealed class DyObjectTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Object;

        public override int ReflectedTypeId => DyType.Object;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(arg.ToString()!);
    }
}
