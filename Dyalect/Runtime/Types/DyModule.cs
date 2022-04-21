using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

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

    protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
    {
        base.SetItem(index, value, ctx);
    }

    protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId != DyType.String)
            return ctx.IndexOutOfRange(index);

        if (!TryGetMember(index.GetString(), ctx, out var value))
            return ctx.IndexOutOfRange(index);

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

    public override int GetHashCode() => HashCode.Combine(TypeId, Unit.Id);
}
