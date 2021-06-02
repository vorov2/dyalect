namespace Dyalect.Runtime.Types
{
    internal sealed class DyClassInfo : DyTypeInfo
    {
        public override string TypeName { get; }

        public DyClassInfo(int typeCode, string typeName) : base(typeCode) => TypeName = typeName;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;
    }
}
 