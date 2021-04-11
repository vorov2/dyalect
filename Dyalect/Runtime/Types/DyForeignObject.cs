using Dyalect.Linker;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignObject<T> : DyObject where T : ForeignTypeInfo
    {
        private static int GetTypeId(RuntimeContext rtx)
        {
            var guid = typeof(T).GetAttribute<ForeignTypeAttribute>()?.Guid;

            if (guid is null)
                throw new DyException($"Missing required [{nameof(ForeignTypeAttribute)}].");

            if (!rtx.Composition.TypeCodes.TryGetValue(guid.Value, out var id))
                throw new DyException($"Unable to find type {nameof(T)}.");

            return id;
        }

        protected DyForeignObject(RuntimeContext rtx) : base(GetTypeId(rtx)) { }

        protected DyForeignObject(int typeId) : base(typeId) { }
    }
}
