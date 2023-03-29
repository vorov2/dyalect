using Dyalect.Runtime.Types;

namespace Dyalect.Util;

public interface IOptionBag
{
    DyTuple? UserArguments { get; set; }
}
