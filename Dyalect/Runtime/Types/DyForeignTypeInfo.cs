using Dyalect.Linker;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignTypeInfo : DyTypeInfo
    {
        private int _reflectedTypeCode;
        public override sealed int ReflectedTypeId => _reflectedTypeCode;

        public ForeignUnit DeclaringUnit { get; internal set; } = null!;

        internal void SetReflectedTypeCode(int code) => _reflectedTypeCode = code;

        protected DyForeignTypeInfo() { }
    }

    public abstract class DyForeignTypeInfo<T> : DyForeignTypeInfo where T : ForeignUnit
    {
        protected DyForeignTypeInfo() { }

        public new T DeclaringUnit { get; internal set; } = null!;
    }
}
