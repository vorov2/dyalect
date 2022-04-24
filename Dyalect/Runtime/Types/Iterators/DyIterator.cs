using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyIterator : DyObject
{
    public override string TypeName => nameof(DyType.Iterator); 
    
    protected DyIterator() : base(DyType.Iterator) { }

    internal static DyIterator Create(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals) =>
        new DyNativeIterator(unitId, handle, captures, locals);

    public static DyIterator Create(IEnumerable<DyObject> seq) => new DyForeignIterator(seq);

    public abstract DyFunction GetIteratorFunction();

    public abstract IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx);

    public static IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx, DyObject val) =>
        val is IEnumerable<DyObject> seq ? seq : InternalRun(ctx, val);

    private static IEnumerable<DyObject> InternalRun(ExecutionContext ctx, DyObject val)
    {
        var iter = val.GetIterator(ctx)!;

        if (ctx.HasErrors)
            yield break;

        while (true)
        {
            var res = iter.Call(ctx);

            if (ctx.HasErrors)
            {
                iter.Reset(ctx);
                yield break;
            }

            if (!ReferenceEquals(res, DyNil.Terminator))
                yield return res;
            else
            {
                iter.Reset(ctx);
                yield break;
            }
        }
    }
}
