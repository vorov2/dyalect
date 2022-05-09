namespace Dyalect;

[Flags]
public enum Ops
{
    None   = 0xFF,
    Add    = 0x01,
    Sub    = 0x02,
    Mul    = 0x04,
    Div    = 0x08,
    Rem    = 0x10,
    Shl    = 0x20,
    Shr    = 0x40,
    And    = 0x80,
    Or     = 0x100,
    Xor    = 0x200,
    Gt     = 0x400,
    Lt     = 0x800,
    Gte    = 0x1000,
    Lte    = 0x2000,
    Neg    = 0x4000,
    BitNot = 0x8000,
    Bit    = 0x10000,
    Plus   = 0x20000,
    Get    = 0x40000,
    Set    = 0x80000,
    Len    = 0x100000,
    Iter   = 0x200000,
    In     = 0x400000
}
