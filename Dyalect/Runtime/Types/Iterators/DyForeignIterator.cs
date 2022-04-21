using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

internal sealed class DyForeignIterator : DyIterator
{
    private readonly IEnumerable<DyObject> seq;

    public DyForeignIterator(IEnumerable<DyObject> seq) => this.seq = seq;

    public override DyFunction GetIteratorFunction() => new DyIteratorFunction(seq);

    public override IEnumerable<DyObject> ToEnumerable(ExecutionContext _) => seq;

    public override object ToObject() => seq;

    public override int GetHashCode() => seq.GetHashCode();
}
