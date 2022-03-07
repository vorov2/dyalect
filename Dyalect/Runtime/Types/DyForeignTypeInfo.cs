using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignTypeInfo : DyTypeInfo
    {
        private int _reflectedTypeCode;
        public override sealed int ReflectedTypeCode => _reflectedTypeCode;

        internal void SetReflectedTypeCode(int code) => _reflectedTypeCode = code;

        public Unit DeclaringUnit { get; internal set; } = null!;

        protected DyForeignTypeInfo() { }
    }
}
