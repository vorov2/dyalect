namespace Dyalect.Runtime.Types;

public abstract class DyForeignObject : DyObject
{
    public override int TypeId => TypeInfo.ReflectedTypeId;

    public override string TypeName => TypeInfo.ReflectedTypeName;

    public DyForeignTypeInfo TypeInfo { get; }

    protected DyForeignObject(DyForeignTypeInfo typeInfo) : base(-1) => TypeInfo = typeInfo;

    public override int GetHashCode() => HashCode.Combine(TypeId, TypeName);
}
