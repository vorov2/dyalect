namespace Dyalect.Compiler
{
    public static class OpExtensions
    {
        public static int GetSize(this OpCode op)
        {
            return OpSizeHelper.Op[(int)op];
        }

        public static int GetStack(this OpCode op)
        {
            return OpStackHelper.Op[(int)op];
        }
    }
}
