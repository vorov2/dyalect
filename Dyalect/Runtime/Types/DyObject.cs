using Dyalect.Compiler;
using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject : IEquatable<DyObject>
    {
        internal readonly DyTypeInfo DecType;

        public DyTypeInfo TypeInfo => DecType;

        protected DyObject(DyTypeInfo typeInfo) => DecType = typeInfo;

        public override string ToString() => $"[type:{DecType.TypeName}]";

        protected internal virtual bool GetBool() => true;

        protected internal virtual long GetInteger() => throw new InvalidCastException();

        protected internal virtual double GetFloat() => throw new InvalidCastException();

        protected internal virtual char GetChar() => throw new InvalidCastException();

        protected internal virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            Is(index, DyInteger.Type) && index.GetInteger() == 0 ? this : ctx.IndexOutOfRange();

        protected internal virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, DecType.TypeName);

        protected internal virtual bool HasItem(string name, ExecutionContext ctx) => false;

        protected internal virtual string? GetLabel() => null;

        protected internal virtual DyObject GetTaggedValue() => this;

        protected internal virtual DyObject Unbox() => this;
        
        public virtual void GetConstructor(ExecutionContext ctx, out string ctor, out bool priv)
        {
            ctor = "";
            priv = false;
        }

        public virtual DyObject Clone() => (DyObject)MemberwiseClone();

        internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

        public virtual bool Equals(DyObject? other) => ReferenceEquals(this, other);

        public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);

        public abstract override int GetHashCode();

        protected int CalculateSimpleHashCode() => base.GetHashCode();

        public static bool Is(DyTypeInfo type, DyObject obj) => ReferenceEquals(type, obj.DecType);
        public static bool Is(DyTypeInfo type, DyTypeInfo otherType) => ReferenceEquals(type, otherType);
        public static bool Is(DyObject obj, DyTypeInfo type) => ReferenceEquals(obj.DecType, type);
        public static bool Is(DyObject obj, DyObject other) => ReferenceEquals(obj.DecType, other.DecType);

        #region Pattern Match
        protected virtual bool Match_CheckLength(int _) => false;
        protected virtual DyObject Match_GetByIndex(int _) => DyNil.Instance;
        protected virtual DyObject Match_GetByName(string _) => DyNil.Instance;
        #endregion
    }
}
