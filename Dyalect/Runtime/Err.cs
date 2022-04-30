using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;

internal static class Err
{
    public static Exception IndexOutOfRange(object obj)
    {
        var err = new DyVariant(DyErrorCode.IndexOutOfRange, obj);
        return new DyCodeException(err);
    }

    public static Exception IndexOutOfRange()
    {
        var err = new DyVariant(DyErrorCode.IndexOutOfRange);
        return new DyCodeException(err);
    }
}
