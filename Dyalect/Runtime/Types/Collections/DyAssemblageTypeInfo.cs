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
                int idx;

                if (name.Length > 4 && name[..3] == "set")
                {
                    idx = amb.GetOrdinal(name[4..]);
                    return new DySetterFunction(idx, name);
                }

                idx = amb.GetOrdinal(name);
                return new DyGetterFunction(idx);
            }

            return base.InitializeInstanceMember(self, name, ctx);
        }
    }
}
