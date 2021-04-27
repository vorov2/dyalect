using Dyalect.Compiler;

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

        protected DyForeignObject(RuntimeContext rtx, Unit unit) : this(rtx, unit, null) { }

        protected DyForeignObject(RuntimeContext rtx, Unit unit, string ctor) : base(GetTypeId(rtx)) =>
            (RuntimeContext, DeclaringUnit, Constructor) = (rtx, unit,ctor);

        public RuntimeContext RuntimeContext { get; }

        public Unit DeclaringUnit { get; }

        public string Constructor { get; }

        public override string GetConstructor(ExecutionContext ctx) =>
            Constructor ?? base.GetConstructor(ctx);
    }
}
