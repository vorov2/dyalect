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

        public override object AsObject() => Unit;

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(Unit, ((DyModule)obj).Unit);
    }

    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public static readonly DyModuleTypeInfo Instance = new DyModuleTypeInfo();

        private DyModuleTypeInfo() : base(StandardType.Nil)
        {

        }

        public override string TypeName => StandardType.ModuleName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            Err.OperationNotSupported(nameof(Create), TypeName).Set(ctx);

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => 
            "[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]";
    }
}
