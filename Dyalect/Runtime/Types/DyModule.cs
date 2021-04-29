using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject, IEnumerable<DyObject>
    {
        internal readonly DyObject[] Globals;

        internal Unit Unit { get; }

        public DyModule(Unit unit, DyObject[] globals) : base(DyType.Module)
        {
            Unit = unit;
            Globals = globals;
        }

        public override object ToObject() => Unit;

        protected internal override bool HasItem(string name, ExecutionContext ctx)
        {
            if (!Unit.ExportList.TryGetValue(name, out var sv))
                return false;

            return (sv.Data & VarFlags.Private) != VarFlags.Private;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId is not DyType.String)
                return ctx.InvalidType(index);

            if (!TryGetMember(index.GetString(), ctx, out var value))
                return ctx.IndexOutOfRange();

            return value;
        }

        private bool TryGetMember(string name, ExecutionContext ctx, out DyObject value)
        {
            value = null;

            if (!Unit.ExportList.TryGetValue(name, out var sv))
            {
                if (!Unit.TypeMap.TryGetValue(name, out var td))
                    return false;
                else
                {
                    value = ctx.RuntimeContext.Types[td.Id];
                    return true;
                }
            }
            else
            {
                if ((sv.Data & VarFlags.Private) == VarFlags.Private)
                    ctx.PrivateNameAccess(name);

                value = Globals[sv.Address >> 8];
                return true;
            }
        }

        public IEnumerator<DyObject> GetEnumerator()
        {
            foreach (var (key, sv) in Unit.ExportList)
            {
                if ((sv.Data & VarFlags.Private) != VarFlags.Private)
                    yield return new DyTuple(new DyObject[] {
                        new DyLabel("key", new DyString(key)),
                        new DyLabel("value", Globals[sv.Address >> 9])
                        });
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override int GetHashCode() => HashCode.Combine(TypeId, Unit.Id);
    }

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

        protected override DyFunction InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Module")
                return DyForeignFunction.Static(name, c => new DyModule(c.RuntimeContext.Composition.Units[0], c.RuntimeContext.Units[0]));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}