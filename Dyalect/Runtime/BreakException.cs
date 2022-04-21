using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Runtime
{
    internal sealed class BreakException : Exception, IError
    {
        public DyVariant Error { get; }

        public BreakException(DyVariant error) => Error = error;
    }
}
