namespace Dyalect.Runtime.Types;

public abstract class DyObject : IEquatable<DyObject>
{
    public virtual int TypeId { get; }

    public abstract string TypeName { get; }

    protected DyObject(int typeCode) => TypeId = typeCode;

    public override string ToString() => $"[type:{TypeId}]";

    protected internal virtual long GetInteger() => throw new NotSupportedException();

    protected internal virtual double GetFloat() => throw new NotSupportedException();

    public abstract object ToObject();

    public virtual DyObject Clone() => (DyObject)MemberwiseClone();

    public abstract bool Equals(DyObject? other);

    public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);

    public abstract override int GetHashCode();
}
