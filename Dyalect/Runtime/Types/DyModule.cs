using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public sealed class DyModule : DyObject
    {
        internal static readonly DyModuleTypeInfo Type = new();
        internal readonly DyObject[] Globals;

        internal Unit Unit { get; }

        public DyModule(DyTypeInfo typeInfo, Unit unit, DyObject[] globals) : base(typeInfo)
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
            if (index.DecType.TypeCode != DyTypeCode.String)
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

        public IEnumerator<DyObject> GetEnumerator(ExecutionContext ctx)
        {
            foreach (var (key, sv) in Unit.ExportList)
            {
                if ((sv.Data & VarFlags.Private) != VarFlags.Private)
                    yield return new DyTuple(ctx.RuntimeContext.Tuple, 
                        new DyObject[] {
                            new DyLabel(ctx.RuntimeContext.Label, "key", new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, key)),
                            new DyLabel(ctx.RuntimeContext.Label, "value", Globals[sv.Address >> 9])
                        });
            }
        }

        public override int GetHashCode() => HashCode.Combine((int)Type.TypeCode, Unit.Id);
    }
}