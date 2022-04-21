using Dyalect.Runtime.Types;

namespace Dyalect.Runtime;

internal interface IError
{
    DyVariant Error { get; }
}
