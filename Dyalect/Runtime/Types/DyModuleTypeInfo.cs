using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len
            | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Module;

        public override int ReflectedTypeId => DyType.Module;

        public DyModuleTypeInfo() => AddMixin(DyType.Collection);

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
            new DyString("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var count = 0;

            foreach (var g in ((DyModule)arg).Unit.ExportList)
                if ((g.Value.Data & VarFlags.Private) != VarFlags.Private)
                    count++;

            return DyInteger.Get(count);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyModule mod)
                return ((DyModule)left).Unit.Id == mod.Unit.Id ? DyBool.True : DyBool.False;

            return DyBool.False;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject ContainsOp(DyObject self, HashString field, ExecutionContext ctx)
        {
            var mod = (DyModule)self;

            if (!mod.Unit.ExportList.TryGetValue(field, out var sv))
                return DyBool.False;

            return (sv.Data & VarFlags.Private) != VarFlags.Private ? DyBool.True : DyBool.False;
        }
    }
}
