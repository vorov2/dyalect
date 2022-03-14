using System;

namespace Dyalect
{
    [Flags]
    public enum SupportedOperations
    {
        None = 0xFF,
        Add = 0x01,
        Sub = 0x02,
        Mul = 0x04,
        Div = 0x08,
        Rem = 0x10,
        Shl = 0x20,
        Shr = 0x40,
        And = 0x80,
        Or = 0x100,
        Xor = 0x200,
        Eq = 0x400,
        Neq = 0x800,
        Gt = 0x1000,
        Lt = 0x2000,
        Gte = 0x4000,
        Lte = 0x8000,
        Neg = 0x10000,
        BitNot = 0x20000,
        Bit = 0x40000,
        Plus = 0x80000,
        Not = 0x100000,
        Get = 0x200000,
        Set = 0x400000,
        Len = 0x800000,
        Iter = 0x1000000,
        Lit = 0x2000000
    }
}
