﻿using Dyalect.Compiler;
using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject : IEquatable<DyObject>
    {
        public readonly int TypeId;

        protected DyObject(int typeId) => TypeId = typeId;

        public override string ToString() => $"[type:{DyType.GetTypeNameByCode(TypeId)}]";

        protected internal virtual bool GetBool() => true;

        protected internal virtual long GetInteger() => throw new InvalidCastException();

        protected internal virtual double GetFloat() => throw new InvalidCastException();

        protected internal virtual char GetChar() => throw new InvalidCastException();

        protected internal virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        protected internal virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            index.TypeId == DyType.Integer && index.GetInteger() == 0 ? this : ctx.IndexOutOfRange();

        protected internal virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, this.GetTypeName(ctx));

        protected internal virtual bool HasItem(string name, ExecutionContext ctx) => false;

        protected internal virtual string? GetLabel() => null;

        protected internal virtual DyObject GetTaggedValue() => this;

        public virtual string? GetConstructor(ExecutionContext ctx) => null;

        public virtual DyObject Clone() => (DyObject)MemberwiseClone();

        internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

        public virtual bool Equals(DyObject? other) => ReferenceEquals(this, other);

        public sealed override bool Equals(object? obj) => obj is DyObject dyo && Equals(dyo);

        public abstract override int GetHashCode();

        protected int CalculateSimpleHashCode() => base.GetHashCode();
    }
}
