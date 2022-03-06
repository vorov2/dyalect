using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public abstract class ForeignTypeInfo : DyTypeInfo
    {
        protected ForeignTypeInfo() { }

        public Unit DeclaringUnit { get; internal set; } = null!;

        private int _reflectedTypeCode;
        public override sealed int ReflectedTypeCode => _reflectedTypeCode;

        internal void SetReflectedTypeCode(int code) => _reflectedTypeCode = code;
    }
}
