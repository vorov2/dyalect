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
            AddType("ByteArray", i => new DyByteArrayTypeInfo(i));
            AddType("StringBuilder", i => new DyStringBuilderTypeInfo(i));
        }

        [Function("NewByteArray")]
        public DyObject ByteArray(int[] arr)
        {
            return new DyByteArray(QueryType(nameof(DyByteArray)).Id, arr.Select(i => (byte)i).ToArray());
        }

        [Function("NewStringBuilder")]
        public DyObject NewStringBuilder()
        {
            return new DyStringBuilder(QueryType(nameof(DyStringBuilder)).Id, new System.Text.StringBuilder());
        }
    }
}
