using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;
using E = DyCodeException;

internal static class Err
{
    public static E IndexOutOfRange(object index) => new (DyError.IndexOutOfRange, index);
}
