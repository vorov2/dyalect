using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Library.Core;

public sealed class AbstractDateTypeInfo<T> : AbstractSpanTypeInfo<T>
    where T : DyObject, IDateTime, IFormattable
{
    protected AbstractDateTypeInfo(string typeName) : base(typeName)
    {

    }

    protected override SupportedOperations GetSupportedOperations() => base.GetSupportedOperations();

    protected override DyObject Parse(string format, string input) => throw new NotImplementedException();
}
