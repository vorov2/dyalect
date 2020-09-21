using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Library
{
    [DyUnit("core")]
    public sealed class Core : ForeignUnit
    {
        public Core()
        {
            AddType<ByteArray>(i => new ByteArrayTypeInfo(i));
        }

        [Function("NewByteArray")]
        public DyObject ByteArray(int[] arr)
        {
            return new ByteArray(QueryType(nameof(ByteArray)).Id, arr.Select(i => (byte)i).ToArray());
        }
    }
}
