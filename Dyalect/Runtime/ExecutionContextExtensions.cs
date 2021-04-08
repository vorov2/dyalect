using Dyalect.Linker;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static partial class ExecutionContextExtensions
    {
        public static DyTypeInfo QueryType<T>(this ExecutionContext self) where T : DyTypeInfo
        {
            var key = typeof(T).GetAttribute<ForeignTypeAttribute>()?.Guid;

            if (key is null)
                throw new DyException($"Invalid type for an object: {nameof(T)}.");

            if (!self.RuntimeContext.Composition.TypeCodes.TryGetValue(key.Value, out var id))
                throw new DyException($"Type {nameof(T)} is not registered.");

            return self.RuntimeContext.Composition.Types[id];
        }

    }
}
