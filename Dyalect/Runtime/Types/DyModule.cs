using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject
    {
        private readonly DyObject[] globals;

        internal Unit Unit { get; }

        public DyModule(Unit unit, DyObject[] globals) : base(DyType.Module)
        {
            this.Unit = unit;
            this.globals = globals;
        }

        public override object ToObject() => Unit;

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            Unit.ExportList.ContainsKey(name);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.String)
                return ctx.IndexInvalidType(DyTypeNames.String, index);

            if (!Unit.ExportList.TryGetValue(index.GetString(), out var sv))
                return ctx.IndexOutOfRange(DyTypeNames.Module, index);

            if ((sv.Data & VarFlags.Private) == VarFlags.Private)
                return ctx.PrivateNameAccess(index);

            return globals[sv.Address >> 8];
        }

        protected internal override DyObject GetItem(int index, ExecutionContext ctx) =>
                ctx.IndexInvalidType(DyTypeNames.String, DyInteger.Get(index));

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            if (!Unit.ExportList.TryGetValue(name, out var sv))
            {
                value = null;
                return false;
            }

            value = globals[sv.Address >> 8];
            return true;
        }
    }

    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public DyModuleTypeInfo() : base(DyType.Module)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Module;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => 
            (DyString)("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return DyForeignFunction.Auto(AutoKind.Generated, (c, self) =>
            {
                if (!self.TryGetItem(name, c, out var value))
                    return ctx.IndexOutOfRange(DyTypeNames.Tuple, name);
                return value;
            });
        }
    }
}