using Dyalect.Linker;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
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

        [Function("ByteArray")]
        public DyObject ByteArray()
        {
            return new ByteArray(QueryType(nameof(ByteArray)).Id);
        }
    }
}
