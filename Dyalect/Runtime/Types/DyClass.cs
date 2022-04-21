using Dyalect.Compiler;
using System;
namespace Dyalect.Runtime.Types;

public sealed class DyClass : DyObject
{
    internal Unit DeclaringUnit { get; }

    internal string Constructor { get; }

    internal DyTuple Fields { get; }

    internal DyTypeInfo DecType { get; }

    internal DyClass(DyTypeInfo type, string ctor, DyTuple privates, Unit unit) : base(type.ReflectedTypeId) =>
        (DecType, Constructor, Fields, DeclaringUnit) = (type, ctor, privates, unit);

    public override object ToObject() => this;

    public override string? GetConstructor() => Constructor;

    public override int GetHashCode() => HashCode.Combine(Constructor, Fields);

    public override bool Equals(DyObject? other) =>
        other is not null && DecType.TypeId == other.TypeId && other is DyClass t 
            && t.Constructor == Constructor && t.Fields.Equals(Fields);

    protected internal override DyObject GetInitValue() => Fields;

    public override DyObject Clone() => new DyClass(DecType, Constructor, Fields, DeclaringUnit);
}
