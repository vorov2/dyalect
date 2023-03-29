using Dyalect.Runtime.Types;
using System.Runtime.CompilerServices;

namespace Dyalect.Runtime;

public static class ImplicitConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetFloat(this DyObject self)
    {
        if (self is DyFloat r8)
            return r8.Value;

        if (self is DyInteger i8)
            return i8.Value;

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
