using Dyalect.Library.Types;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect.Library
{
    [DyUnit("core")]
    public sealed class Core : ForeignUnit
    {
        public DyByteArrayTypeInfo ByteArray { get; private set; } = null!;
        public DyStringBuilderTypeInfo StringBuilder { get; private set; } = null!;
        public DyRegexTypeInfo Regex { get; private set; } = null!;
        public DyResultTypeInfo Result { get; private set; } = null!;

        protected override void InitializeTypes()
        {
            ByteArray = AddType<DyByteArrayTypeInfo>();
            StringBuilder = AddType<DyStringBuilderTypeInfo>();
            Regex = AddType<DyRegexTypeInfo>();
            Result = AddType<DyResultTypeInfo>();
        }
    }
}
