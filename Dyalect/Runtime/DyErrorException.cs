using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Runtime;

internal sealed class DyErrorException : Exception, IError
{
    public DyVariant Error { get; }

    public DyErrorException(DyVariant error) => Error = error;
}
