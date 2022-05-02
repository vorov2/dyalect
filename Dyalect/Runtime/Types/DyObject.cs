using Dyalect.Compiler;
using System.IO;
namespace Dyalect.Runtime.Types;

public abstract class DyObject : IEquatable<DyObject>
{
    public virtual int TypeId { get; }

    public abstract string TypeName { get; }

    protected DyObject(int typeCode) => TypeId = typeCode;

    public override string ToString() => $"[type:{TypeId}]";

    protected internal virtual long GetInteger() => throw new NotSupportedException();

    protected internal virtual double GetFloat() => throw new NotSupportedException();

    protected internal virtual char GetChar() => throw new NotSupportedException();

    protected internal virtual string GetString() => throw new NotSupportedException();

    public abstract object ToObject();

    public virtual SupportedOperations Supports() => SupportedOperations.None;

    //TODO: move to separate class
    protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        object? retval;
        
        if (index.TypeId is Dy.Integer)
            retval = GetItem(index.GetInteger());
        else if (index.TypeId is Dy.String)
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
    //read-only fields which are always obtained without any exceptions. Objects with different behavior
    //should override GetItem(DyObject,ExecutionContext)
    //TODO: Consider moving to a separate class
    protected virtual object? GetItem(string key) => null;
    protected virtual object? GetItem(long index) => null;

    public virtual string? GetConstructor() => null;

    public virtual DyObject Clone() => (DyObject)MemberwiseClone();

    public abstract bool Equals(DyObject? other);

    public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);

    public abstract override int GetHashCode();
}
