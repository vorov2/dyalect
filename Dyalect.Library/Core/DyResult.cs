using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core;

public sealed class DyResult : DyForeignObject, IProduction, IMeasurable
{
    internal readonly DyObject Value;

    public string Constructor { get; }

    public int Count => Value.TypeId is Dy.Nil ? 0 : 1;

    public DyResult(DyForeignTypeInfo typeInfo, string ctor, DyObject value) : base(typeInfo) =>
        (Value, Constructor) = (value, ctor);

    public override object ToObject() => Value.ToObject();

    public override bool Equals(DyObject? other) => Value.Equals(other);
}
