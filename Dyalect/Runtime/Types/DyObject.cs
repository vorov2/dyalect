﻿using Dyalect.Compiler;
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

        internal protected virtual char GetChar() => throw new InvalidCastException();

        internal protected virtual string GetString() => throw new InvalidCastException();

        public abstract object ToObject();

        internal protected virtual DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Get, this.TypeName(ctx)).Set(ctx);

        internal protected virtual void SetItem(DyObject index, DyObject value, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Set, this.TypeName(ctx)).Set(ctx);

        internal protected virtual DyObject GetItem(string name, ExecutionContext ctx) => null;

        internal protected virtual void SetItem(string name, DyObject value, ExecutionContext ctx) { }

        internal protected virtual string GetLabel() => null;

        internal protected virtual DyObject GetTaggedValue() => null;
    }

    internal static class DyObjectInternalExtensions
    {
        public static bool IsNil(this DyObject self) => ReferenceEquals(self, DyNil.Instance);

        public static DyTypeInfo Type(this DyObject self, ExecutionContext ctx) => ctx.Composition.Types[self.TypeId];

        public static DyString ToString(this DyObject self, ExecutionContext ctx) => ctx.Composition.Types[self.TypeId].ToString(self, ctx);

        public static DyObject GetIterator(this DyObject self, ExecutionContext ctx)
        {
            var nameId = ctx.Composition.MembersMap[Builtins.Iterator];
            var value = ctx.Composition.Types[self.TypeId].GetMemberDirect(self, nameId, ctx);

            if (value == null)
                return Err.OperationNotSupported(Builtins.Iterator, self.TypeName(ctx)).Set(ctx);

            return value;
        }
    }

    public static class DyObjectExtensions
    {
        public static string TypeName(this DyObject self, ExecutionContext ctx) => 
            ctx.Composition.Types[self.TypeId].TypeName;

        public static string Format(this DyObject self, ExecutionContext ctx) => 
            ctx.Composition.Types[self.TypeId].ToString(self, ctx).GetString();
    }
}
