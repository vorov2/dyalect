namespace Dyalect.Runtime.Types
{
    internal sealed class DyAssemblageTypeInfo : DyCollectionTypeInfo
    {
        public DyAssemblageTypeInfo() : base(DyType.Assemblage) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Assemblage;

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (self is DyAssemblage amb)
            {
                var idx = amb.GetOrdinal(name);
                return new DyGetterFunction(idx);
            }

            return base.InitializeInstanceMember(self, name, ctx);
        }
    }
}
