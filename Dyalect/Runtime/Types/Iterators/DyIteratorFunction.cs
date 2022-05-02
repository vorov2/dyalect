using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal sealed class DyIteratorFunction : DyForeignFunction
{
    private readonly IEnumerable<DyObject> enumerable;
    private IEnumerator<DyObject>? enumerator;

    public DyIteratorFunction(IEnumerable<DyObject> enumerable) : base(Builtins.Iterator, Array.Empty<Par>(), -1) =>
        this.enumerable = enumerable;

    internal override DyObject CallWithMemoryLayout(ExecutionContext ctx, params DyObject[] args)
    {
        if (enumerator is null)
            enumerator = enumerable.GetEnumerator();

        if (enumerator.MoveNext())
            return enumerator.Current;

        enumerator = null;
        return DyNil.Terminator;
    }

    public override int GetHashCode() => enumerable.GetHashCode();

    protected override bool Equals(DyFunction func) => func is DyIteratorFunction f && f.enumerable.Equals(enumerator);

    public override DyObject Clone() => new DyIteratorFunction(enumerable);
}
