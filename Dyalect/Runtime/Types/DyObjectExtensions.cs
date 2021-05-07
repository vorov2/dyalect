using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public static class DyObjectExtensions
    {
        public static DyTypeInfo GetTypeInfo(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Composition.Types[self.TypeId];

        internal static DyObject GetIterator(this DyObject self, ExecutionContext ctx) =>
            GetTypeInfo(self, ctx).GetInstanceMember(self, Builtins.Iterator, ctx);

        public static string GetTypeName(this DyObject self, ExecutionContext ctx) => GetTypeInfo(self, ctx).TypeName;

        public static string Format(this DyObject self, ExecutionContext ctx) => ToString(self, ctx).GetString();

        public static DyString ToString(this DyObject self, ExecutionContext ctx) =>
            (DyString)ctx.RuntimeContext.Composition.Types[self.TypeId].ToString(ctx, self);
    }
}
