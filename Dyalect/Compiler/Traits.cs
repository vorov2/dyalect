namespace Dyalect.Compiler
{
    internal static class Traits
    {
        public const int Nul = 0;
        public const int Add = 1;
        public const int Sub = 2;
        public const int Mul = 3;
        public const int Div = 4;
        public const int Rem = 5;
        public const int Shl = 6;
        public const int Shr = 7;
        public const int And = 8;
        public const int Or = 9;
        public const int Xor = 10;
        public const int Eq = 11;
        public const int Neq = 12;
        public const int Gt = 13;
        public const int Lt = 14;
        public const int Gte = 15;
        public const int Lte = 16;
        public const int Neg = 17;
        public const int Not = 18;
        public const int Bit = 19;
        public const int Len = 20;
        public const int Get = 21;
        public const int Set = 22;
        public const int Tos = 23;

        public const string AddName = "+";
        public const string SubName = "-";
        public const string MulName = "*";
        public const string DivName = "/";
        public const string RemName = "%";
        public const string ShlName = "<<";
        public const string ShrName = ">>";
        public const string AndName = "&";
        public const string OrName = "|";
        public const string XorName = "^";
        public const string EqName = "==";
        public const string NeqName = "!=";
        public const string GtName = ">";
        public const string LtName = "<";
        public const string GteName = ">=";
        public const string LteName = "<=";
        public const string NegName = "negate";
        public const string NotName = "!";
        public const string BitName = "~";
        public const string LenName = "#";
        public const string GetName = "get";
        public const string SetName = "set";
        public const string TosName = "toString";

        public const string BoolAndName = "&&";
        public const string BoolOrName = "||";
    }
}
