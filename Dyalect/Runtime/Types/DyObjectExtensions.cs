﻿using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public static class DyObjectExtensions
    {
        internal static bool IsTrue(this DyObject self) =>
            !ReferenceEquals(self, DyBool.False) && !ReferenceEquals(self, DyNil.Instance);

        internal static bool IsFalse(this DyObject self) =>
            ReferenceEquals(self, DyBool.False) || ReferenceEquals(self, DyNil.Instance);

        internal static DyObject GetIterator(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[self.TypeId].GetInstanceMember(self, Builtins.Iterator, ctx);

        public static DyString ToString(this DyObject self, ExecutionContext ctx) => 
            (DyString)ctx.RuntimeContext.Types[self.TypeId].ToString(ctx, self);

        public static DyString ToLiteral(this DyObject self, ExecutionContext ctx) =>
            (DyString)ctx.RuntimeContext.Types[self.TypeId].ToLiteral(ctx, self);

        public static bool Equals(this DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[left.TypeId].Eq(ctx, left, right).IsTrue();

        public static bool NotEquals(this DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[left.TypeId].Neq(ctx, left, right).IsTrue();
        
        public static bool Lesser(this DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[left.TypeId].Lt(ctx, left, right).IsTrue();

        public static bool Greater(this DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[left.TypeId].Gt(ctx, left, right).IsTrue();

        public static string GetTypeName(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[self.TypeId].TypeName;

        public static DyObject Negate(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[self.TypeId].Neg(ctx, self);

        public static bool IsInteger(this DyObject self, ExecutionContext ctx)
        {
            if (self.TypeId != DyType.Integer)
            {
                ctx.InvalidType(DyType.Integer, self);
                return false;
            }

            return true;
        }

        public static bool IsString(this DyObject self, ExecutionContext ctx)
        {
            if (self.TypeId != DyType.String)
            {
                ctx.InvalidType(DyType.String, self);
                return false;
            }

            return true;
        }
    }
}
