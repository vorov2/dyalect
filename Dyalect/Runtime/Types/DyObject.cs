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

        public string SafeToString(ExecutionContext ctx)
        {
            var nctx = ctx.Clone();
            var str = ctx.RuntimeContext.Types[TypeId].ToString(nctx, this);
            return nctx.HasErrors ? ToString() : str.ToString()?.Trim('\"');
        }

        protected internal virtual bool GetBool() => true;

        internal protected virtual long GetInteger() => throw new InvalidCastException();

        internal protected virtual double GetFloat() => throw new InvalidCastException();

        internal protected virtual char GetChar() => throw new InvalidCastException();

        internal protected virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        internal protected virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            index.TypeId == DyType.Integer && index.GetInteger() == 0 ? this : ctx.IndexOutOfRange();

        internal protected virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, this.GetTypeName(ctx));

        internal protected virtual bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value)
        {
            value = null;
            ctx.InvalidType(index);
            return false;
        }

        internal protected virtual bool HasItem(string name, ExecutionContext ctx) => false;

        internal protected virtual string GetLabel() => null;

        internal protected virtual DyObject GetTaggedValue() => this;

        public virtual string GetConstructor(ExecutionContext ctx) => null;

        public virtual DyObject Clone() => (DyObject)MemberwiseClone();

        internal virtual DyObject GetSelf() => this;

        internal virtual int GetCount() => 1;

        internal virtual void Serialize(BinaryWriter writer) => throw new NotSupportedException();

        public virtual bool Equals(DyObject other) => ReferenceEquals(this, other);

        public override sealed bool Equals(object obj) => obj is DyObject dyo && Equals(dyo);

        public override int GetHashCode() => HashCode.Combine(TypeId);
    }

    public static class DyObjectExtensions
    {
        internal static DyObject GetIterator(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId].GetInstanceMember(self, Builtins.Iterator, ctx);

        public static DyTypeInfo GetTypeInfo(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId];

        public static string GetTypeName(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId].TypeName;

        public static string Format(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId].ToString(ctx, self).GetString();

        public static DyString ToString(this DyObject self, ExecutionContext ctx) =>
            (DyString)ctx.RuntimeContext.Composition.Types[self.TypeId].ToString(ctx, self);
    }
}
