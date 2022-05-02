using Dyalect.Runtime.Types;
using System.Runtime.CompilerServices;
namespace Dyalect.Runtime;

internal static class ImplicitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetInt(this DyObject self)
    {
        if (self is DyInteger i8)
            return i8.Value;

        if (self is DyFloat r8)
            return (long)r8.Value;

        throw new InvalidCastException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetStr(this DyObject self)
    {
        if (self is DyString str)
            return str.Value;

        if (self is DyChar c)
            return c.Value.ToString();

        throw new InvalidCastException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char GetChar(this DyObject self)
    {
        if (self is DyChar c)
            return c.Value;

        if (self is DyString str)
            return str.Value.Length > 0 ? str.Value[0] : '\0';

        throw new InvalidCastException();
    }
}
