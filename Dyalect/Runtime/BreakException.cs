using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Runtime
{
    internal sealed class BreakException : Exception
    {
        public DyVariant Error { get; }

        public BreakException(DyVariant error) => Error = error;
    }
}
