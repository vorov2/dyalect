using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public DyModuleTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyType.Module) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len
            | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Module;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var count = 0;

            foreach (var g in ((DyModule)arg).Unit.ExportList)
                if ((g.Value.Data & VarFlags.Private) != VarFlags.Private)
                    count++;

            return DyInteger.Get(count);
        }

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            return base.SetOp(self, index, value, ctx);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyModule mod)
                return ((DyModule)left).Unit.Id == mod.Unit.Id ? DyBool.True : DyBool.False;

            return DyBool.False;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);
    }
}
