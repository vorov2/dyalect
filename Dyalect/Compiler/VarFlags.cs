﻿namespace Dyalect.Compiler;

internal sealed class VarFlags
{
    public const int None = 0;
    public const int Const = 0x01;
    public const int Argument = 0x02;
    public const int Function = 0x04;
    public const int External = 0x08;
    public const int Foreign = 0x10;
    public const int Module = 0x20;
    public const int This = 0x40;
    public const int Private = 0x80;
    public const int PreInit = 0x100;
    public const int Type = 0x200;
    public const int Lazy = 0x400;
    public const int StdCall = 0x800;
}
