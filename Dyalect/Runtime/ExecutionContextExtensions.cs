using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static partial class ExecutionContextExtensions
    {
        public static DyTypeInfo QueryType<T>(this ExecutionContext self) where T : DyTypeInfo
        {
            if (!self.RuntimeContext.Composition.TypeCodes.TryGetValue(typeof(T).GUID, out var id))
                throw new DyException($"Type {nameof(T)} is not registered.");

            return self.RuntimeContext.Composition.Types[id];
        }
    }
}
