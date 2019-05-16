using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject
    {
        private readonly DyObject[] globals;

        internal Unit Unit { get; }

        public DyModule(Unit unit, DyObject[] globals) : base(StandardType.Module)
        {
            this.Unit = unit;
            this.globals = globals;
        }

        public override object ToObject() => Unit;
    }

    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public DyModuleTypeInfo() : base(StandardType.Nil, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => StandardType.ModuleName;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => 
            (DyString)("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");
    }
}
