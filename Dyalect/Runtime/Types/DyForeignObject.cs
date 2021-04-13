using Dyalect.Linker;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignObject<T> : DyObject where T : ForeignTypeInfo
    {
        private static int GetTypeId(RuntimeContext rtx)
        {
            var guid = typeof(T).GUID;

            if (!rtx.Composition.TypeCodes.TryGetValue(guid, out var id))
                throw new DyException($"Unable to find type {nameof(T)}.");

            return id;
        }

        protected DyForeignObject(RuntimeContext rtx) : base(GetTypeId(rtx)) { }

        protected DyForeignObject(RuntimeContext rtx, string ctor) : base(GetTypeId(rtx)) =>
            (RuntimeContext, Constructor) = (rtx, ctor);

        public RuntimeContext RuntimeContext { get; }

        public string Constructor { get; }

        public override int GetConstructorId(ExecutionContext ctx) =>
            string.IsNullOrEmpty(Constructor) ? base.GetConstructorId(ctx) : RuntimeContext.GetMemberId(Constructor);
    }
}
