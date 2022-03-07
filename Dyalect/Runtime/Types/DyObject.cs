﻿using Dyalect.Compiler;
using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject : IEquatable<DyObject>
    {
        public virtual int TypeId { get; }

        protected DyObject(int typeCode) => TypeId = typeCode;

        public override string ToString() => $"[type:{TypeId}]";

        protected internal virtual bool GetBool() => true;

        protected internal virtual long GetInteger() => throw new InvalidCastException();

        protected internal virtual double GetFloat() => throw new InvalidCastException();

        protected internal virtual char GetChar() => throw new InvalidCastException();

        protected internal virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            index.TypeId == DyType.Integer && index.GetInteger() == 0 ? this : ctx.IndexOutOfRange();

        protected internal virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, ctx.RuntimeContext.Types[TypeId].TypeName);

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

        public DyTypeInfo GetTypeInfo(ExecutionContext ctx) => ctx.RuntimeContext.Types[TypeId];
    }
}
