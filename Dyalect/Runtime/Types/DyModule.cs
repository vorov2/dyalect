using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject, IEnumerable<DyObject>
    {
        internal static readonly DyModuleTypeInfo Type = new();
        internal readonly DyObject[] Globals;

        public override DyTypeCode TypeCode => DyTypeCode.Module;

        internal Unit Unit { get; }

        public DyModule(Unit unit, DyObject[] globals)
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

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            base.SetItem(index, value, ctx);
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeCode != DyTypeCode.String)
                return ctx.InvalidType(index);

            if (!TryGetMember(index.GetString(), ctx, out var value))
                return ctx.IndexOutOfRange();

            return value!;
        }

        internal bool TryGetMember(string name, ExecutionContext ctx, out DyObject? value)
        {
            value = null;

            if (Unit.ExportList.TryGetValue(name, out var sv))
            {
                if ((sv.Data & VarFlags.Private) == VarFlags.Private)
                    ctx.PrivateNameAccess(name);

                value = Globals[sv.Address >> 8];
                return true;
            }

            return false;
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

        public override int GetHashCode() => HashCode.Combine((int)Type.TypeCode, Unit.Id);
    }
}