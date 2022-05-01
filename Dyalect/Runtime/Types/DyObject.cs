using Dyalect.Compiler;
using System.IO;
namespace Dyalect.Runtime.Types;

public abstract class DyObject : IEquatable<DyObject>
{
    public virtual int TypeId { get; }

    public abstract string TypeName { get; }

    protected DyObject(int typeCode) => TypeId = typeCode;

    public override string ToString() => $"[type:{TypeId}]";

    protected internal virtual long GetInteger() => throw new InvalidCastException();

    protected internal virtual double GetFloat() => throw new InvalidCastException();

    protected internal virtual char GetChar() => throw new InvalidCastException();

    protected internal virtual string GetString() => ToString();

    public abstract object ToObject();

    public virtual SupportedOperations Supports() => SupportedOperations.None;

    protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        object? retval;
        
        if (index.TypeId == Dy.Integer)
            retval = GetItem(index.GetInteger());
        else if (index.TypeId == Dy.String)
            retval = GetItem(index.GetString());
        else
            return ctx.IndexOutOfRange(index);

        if (retval is null)
            return ctx.IndexOutOfRange(index);

        return TypeConverter.ConvertFrom(retval);
    }

    protected internal virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Set, TypeId);

    //These functions are used by default and can be utilized by objects that expose a fixed set of
    //read-only fields which are always obtained without any exceptions. Object with different behavior
    //should override GetItem(DyObject,ExecutionContext)
    protected internal virtual object? GetItem(string key) => null;
    protected internal virtual object? GetItem(long index) => null;

    public virtual DyTypeInfo GetTypeInfo(ExecutionContext ctx) => ctx.RuntimeContext.Types[TypeId];

    internal virtual bool IsMutable() => false;

    internal virtual DyObject MakeImmutable() => this;

    protected internal virtual string? GetLabel() => null;

    protected internal virtual DyObject GetTaggedValue() => this;

    protected internal virtual DyObject GetInitValue() => this;

    public virtual string? GetConstructor() => null;

    public virtual DyObject Clone() => (DyObject)MemberwiseClone();

    //It's OK to leave this unimplemented as soon as serialization is normally supported by built-in types.
    internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

    //These methods are used by hash tables
    public virtual bool Equals(DyObject? other) => ReferenceEquals(this, other);
    public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);
    public abstract override int GetHashCode();
    protected int CalculateSimpleHashCode() => base.GetHashCode();
}
