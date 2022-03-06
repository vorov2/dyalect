using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public abstract class ForeignTypeInfo : DyTypeInfo
    {
        protected ForeignTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Foreign) { }

        public Unit DeclaringUnit { get; internal set; } = null!;
    }
}
