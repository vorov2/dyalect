using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;

internal static class Err
{
    public static Exception IndexOutOfRange(object obj)
    {
        var err = new DyVariant(DyError.IndexOutOfRange, obj);
        return new DyCodeException(err);
    }

    public static Exception IndexOutOfRange()
    {
        var err = new DyVariant(DyError.IndexOutOfRange);
        return new DyCodeException(err);
    }
}
