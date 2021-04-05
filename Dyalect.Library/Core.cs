using Dyalect.Library.Types;
using Dyalect.Linker;
using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect.Library
{
    [DyUnit("core")]
    public sealed class Core : ForeignUnit
    {
        public Core()
        {
            AddType<DyByteArrayTypeInfo>("ByteArray");
            AddType<DyStringBuilderTypeInfo>("StringBuilder");
        }

        [Function("NewByteArray")]
        public DyObject ByteArray(int[] arr)
        {
            return new DyByteArray(RuntimeContext, arr.Select(i => (byte)i).ToArray());
        }

        [Function("NewStringBuilder")]
        public DyObject NewStringBuilder()
        {
            return new DyStringBuilder(RuntimeContext, new System.Text.StringBuilder());
        }
    }
}
