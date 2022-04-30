using System;
namespace Dyalect.Runtime.Types;

public abstract class DyForeignObject : DyObject
{
    public override int TypeId => TypeInfo.ReflectedTypeId;

    public override string TypeName => TypeInfo.ReflectedTypeName;

    public DyForeignTypeInfo TypeInfo { get; }

    public string? Constructor { get; }

    protected DyForeignObject(DyForeignTypeInfo typeInfo) : this(typeInfo, null) { }

    protected DyForeignObject(DyForeignTypeInfo typeInfo, string? ctor) : base(-1) =>
        (TypeInfo, Constructor) = (typeInfo, ctor);

    public override string? GetConstructor() => Constructor;

    public override int GetHashCode() => HashCode.Combine(TypeId, Constructor);
}
