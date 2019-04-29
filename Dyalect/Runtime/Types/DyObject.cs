using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyObject
    {
        internal readonly int TypeId;

        internal DyObject(int typeId)
        {
            TypeId = typeId;
        }

        public override string ToString() => $"[type:{TypeId}]";

        protected internal virtual bool GetBool() => true;

        internal protected virtual long GetInteger() => throw new InvalidCastException();

        internal protected virtual double GetFloat() => throw new InvalidCastException();

        internal protected virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        internal protected virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Get, this.TypeName(ctx)).Set(ctx);

        internal protected virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Set, this.TypeName(ctx)).Set(ctx);
    }

    internal static class DyObjectInternalExtensions
    {
        public static DyTypeInfo Type(this DyObject self, ExecutionContext ctx) => ctx.Composition.Types[self.TypeId];

        public static DyString ToString(this DyObject self, ExecutionContext ctx) => ctx.Composition.Types[self.TypeId].ToString(self, ctx);
    }

    public static class DyObjectExtensions
    {
        public static string TypeName(this DyObject self, ExecutionContext ctx) => 
            ctx.Composition.Types[self.TypeId].TypeName;

        public static string Format(this DyObject self, ExecutionContext ctx) => 
            ctx.Composition.Types[self.TypeId].ToString(self, ctx).GetString();
    }
}
