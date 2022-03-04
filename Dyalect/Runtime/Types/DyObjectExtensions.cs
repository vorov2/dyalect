using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public static class DyObjectExtensions
    {
        internal static DyObject GetIterator(this DyObject self, ExecutionContext ctx) =>
            self.DecType.GetInstanceMember(self, Builtins.Iterator, ctx);

        public static string Format(this DyObject self, ExecutionContext ctx) => ToString(self, ctx).GetString();

        public static DyString ToString(this DyObject self, ExecutionContext ctx) =>
            (DyString)self.DecType.ToString(ctx, self);
    }
}
