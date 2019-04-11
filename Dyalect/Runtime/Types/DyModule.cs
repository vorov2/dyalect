using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject
    {
        private readonly Unit unit;
        private readonly DyObject[] globals;

        public DyModule(Unit unit, DyObject[] globals) : base(StandardType.Module)
        {
            this.unit = unit;
            this.globals = globals;
        }

        public override object AsObject() => unit;

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(unit, ((DyModule)obj).unit);

        public override DyTypeInfo GetTypeInfo() => DyModuleTypeInfo.Instance;
    }
}
