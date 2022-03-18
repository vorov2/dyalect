using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public static class DyObjectExtensions
    {
        internal static DyObject GetIterator(this DyObject self, ExecutionContext ctx) =>
            ctx.RuntimeContext.Types[self.TypeId].GetInstanceMember(self, Builtins.Iterator, ctx);

        public static DyString ToString(this DyObject self, ExecutionContext ctx) => 
            (DyString)ctx.RuntimeContext.Types[self.TypeId].ToString(ctx, self);

        public static DyString ToLiteral(this DyObject self, ExecutionContext ctx) =>
            (DyString)ctx.RuntimeContext.Types[self.TypeId].ToLiteral(ctx, self);
    }
}
