namespace Dyalect.Compiler
{
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
    }
}
