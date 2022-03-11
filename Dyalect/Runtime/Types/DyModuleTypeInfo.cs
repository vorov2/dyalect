﻿using Dyalect.Compiler;
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

        public override int ReflectedTypeCode => DyType.Module;

        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString("[module " + Path.GetFileName(((DyModule)arg).Unit.FileName) + "]");

        internal protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var count = 0;

            foreach (var g in ((DyModule)arg).Unit.ExportList)
                if ((g.Value.Data & VarFlags.Private) != VarFlags.Private)
                    count++;

            return DyInteger.Get(count);
        }

        internal protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            return base.SetOp(self, index, value, ctx);
        }

        internal protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyModule mod)
                return ((DyModule)left).Unit.Id == mod.Unit.Id ? DyBool.True : DyBool.False;

            return DyBool.False;
        }

        internal protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);
    }
}
