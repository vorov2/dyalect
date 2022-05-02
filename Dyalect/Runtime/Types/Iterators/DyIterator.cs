using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyIterator : DyObject
{
    public override string TypeName => nameof(Dy.Iterator); 
    
    protected DyIterator() : base(Dy.Iterator) { }

    internal static DyIterator Create(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals) =>
        new DyNativeIterator(unitId, handle, captures, locals);

    public static DyIterator Create(IEnumerable<DyObject> seq) => new DyForeignIterator(seq);

    public abstract DyFunction GetIteratorFunction();

    public abstract IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx);

    public static IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx, DyObject val)
    {
        if (val is IEnumerable<DyObject> seq)
            return seq;
        else
        {
            var iter = val.GetIterator(ctx);
            return InternalRun(ctx, iter);
        }
    }

    private static IEnumerable<DyObject> InternalRun(ExecutionContext ctx, DyFunction? iter)
    {
        if (iter is null)
            yield break;

        while (true)
        {
            var res = iter.Call(ctx);

            if (!ReferenceEquals(res, DyNil.Terminator))
                yield return res;
            else
                yield break;
        }
    }
}
