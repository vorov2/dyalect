using Dyalect.Compiler;
using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject : IEquatable<DyObject>
    {
        public virtual int TypeId { get; }

        protected DyObject(int typeCode) => TypeId = typeCode;

        public override string ToString() => $"[type:{TypeId}]";

        protected internal virtual bool GetBool(ExecutionContext ctx) => true;

        protected internal virtual long GetInteger() => throw new InvalidCastException();

        protected internal virtual double GetFloat() => throw new InvalidCastException();

        protected internal virtual char GetChar() => throw new InvalidCastException();

        protected internal virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        public virtual SupportedOperations Supports() => SupportedOperations.None;

        protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            object? retval;
            
            if (index.TypeId == DyType.Integer)
                retval = GetItem(index.GetInteger());
            else if (index.TypeId == DyType.String)
                retval = GetItem(index.GetString());
            else
                return ctx.InvalidType(index);

            if (retval is null)
                return ctx.IndexOutOfRange(index);

            return TypeConverter.ConvertFrom(retval);
        }

        protected internal virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, ctx.RuntimeContext.Types[TypeId].TypeName);

        protected internal virtual object? GetItem(string key) => null;
        protected internal virtual object? GetItem(long index) => null;

        protected internal virtual bool HasItem(string name, ExecutionContext ctx) => false;

        protected internal virtual string? GetLabel() => null;

        protected internal virtual DyObject GetTaggedValue() => this;

        protected internal virtual DyObject Unbox() => this;

        public virtual string GetConstructor(ExecutionContext ctx) => "";

        public virtual DyObject Clone() => (DyObject)MemberwiseClone();

        internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

        public virtual bool Equals(DyObject? other) => ReferenceEquals(this, other);

        public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);

        public abstract override int GetHashCode();

        protected int CalculateSimpleHashCode() => base.GetHashCode();

        public virtual DyTypeInfo GetTypeInfo(ExecutionContext ctx) => ctx.RuntimeContext.Types[TypeId];

        internal virtual DyObject Force(ExecutionContext ctx) => this;
    }
}
