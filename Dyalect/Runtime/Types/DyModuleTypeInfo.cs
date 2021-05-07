using Dyalect.Compiler;
using System.IO;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyModuleTypeInfo : DyTypeInfo
    {
        public DyModuleTypeInfo() : base(DyType.Module) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Module;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            (DyString)("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");

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
                return (DyBool)(((DyModule)left).Unit.Id == mod.Unit.Id);

            return DyBool.False;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Module")
                return Func.Static(name, c => new DyModule(c.RuntimeContext.Composition.Units[0], c.RuntimeContext.Units[0]));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
