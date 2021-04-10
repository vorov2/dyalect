using Dyalect.Compiler;
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

        internal protected virtual long GetInteger() => throw new InvalidCastException();

        internal protected virtual double GetFloat() => throw new InvalidCastException();

        internal protected virtual char GetChar() => throw new InvalidCastException();

        internal protected virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        internal protected virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            index.TypeId == DyType.Integer && index.GetInteger() == 0 ? this : ctx.IndexOutOfRange(index);

        internal protected virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, this);

        internal protected virtual DyObject GetItem(string name, ExecutionContext ctx) =>
            GetItem(new DyString(name), ctx);

        internal protected virtual DyObject GetItem(int index, ExecutionContext ctx) =>
            index == 0 ? this : ctx.IndexOutOfRange(index);

        internal protected virtual bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            value = null;
            return false;
        }

        internal protected virtual bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value)
        {
            if (index.TypeId == DyType.Integer)
                return TryGetItem((int)index.GetInteger(), ctx, out value);
            else if (index.TypeId == DyType.String)
                return TryGetItem(index.GetString(), ctx, out value);
            else
            {
                value = null;
                ctx.IndexInvalidType(index);
                return false;
            }
        }

        internal protected virtual bool TryGetItem(int index, ExecutionContext ctx, out DyObject value)
        {
            value = null;
            return false;
        }

        internal protected virtual void SetItem(string name, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Get, this);

        internal protected virtual void SetItem(int index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, this);

        internal protected virtual bool HasItem(string name, ExecutionContext ctx) => false;

        internal protected virtual string GetLabel() => null;

        internal protected virtual DyObject GetTaggedValue() => null;

        internal virtual int GetConstructorId(ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.MembersMap.TryGetValue(ctx.RuntimeContext.Types[TypeId].TypeName, out var id) ? id : 0;

        public virtual DyObject Clone() => (DyObject)MemberwiseClone();

        internal virtual DyObject GetSelf() => this;

        internal virtual int GetCount() => 1;

        internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

        public virtual bool Equals(DyObject other) => ReferenceEquals(this, other);

        public override sealed bool Equals(object obj) => obj is DyObject dyo && Equals(dyo);

        public override int GetHashCode() => HashCode.Combine(TypeId);
    }

    internal static class DyObjectInternalExtensions
    {
        public static bool IsNil(this DyObject self) => ReferenceEquals(self, DyNil.Instance);

        public static DyString ToString(this DyObject self, ExecutionContext ctx) => 
            (DyString)ctx.RuntimeContext.Composition.Types[self.TypeId].ToString(ctx, self);

        public static DyObject GetIterator(this DyObject self, ExecutionContext ctx)
        {
            var nameId = ctx.RuntimeContext.Composition.MembersMap[Builtins.Iterator];
            var value = ctx.RuntimeContext.Composition.Types[self.TypeId].GetMemberDirect(self, nameId, ctx);

            if (value == null)
                return ctx.OperationNotSupported(Builtins.Iterator, self);

            return value;
        }
    }

    public static class DyObjectExtensions
    {
        public static DyTypeInfo GetTypeInfo(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId];

        public static string GetTypeName(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId].TypeName;

        public static string Format(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId].ToString(ctx, self).GetString();
    }
}
