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
        public static readonly DyModuleTypeInfo Instance = new DyModuleTypeInfo();

        private DyModuleTypeInfo() : base(StandardType.Nil, false)
        {

        }

        public override string TypeName => StandardType.ModuleName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => 
            "[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]";
    }
}
