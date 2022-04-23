using System;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal sealed class DyNativeIterator : DyIterator
{
    private readonly int unitId;
    private readonly int handle;
    private readonly FastList<DyObject[]> captures;

    public DyNativeIterator(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals)
    {
        var vars = new FastList<DyObject[]>(captures) { locals };
        (this.unitId, this.handle, this.captures) = (unitId, handle, vars);
    }

    public override DyFunction GetIteratorFunction() => new DyNativeIteratorFunction(unitId, handle, captures);

    public override object ToObject() => this;

    public override IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx) => new MultiPartEnumerable(ctx, GetIteratorFunction());

    public override int GetHashCode() => HashCode.Combine(unitId, handle, captures);
}
