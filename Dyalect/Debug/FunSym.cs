namespace Dyalect.Debug
{
    public sealed class FunSym
    {
        public FunSym(string name, int offset, FunctionParameter[] pars)
        {
            Name = name;
            StartOffset = offset;
            Parameters = pars;
        }

        public FunSym()
        {

        }

        public string Name { get; set; }

        public FunctionParameter[] Parameters { get; set; }

        public int Handle { get; set; }

        public int StartOffset { get; set; }

        public int EndOffset { get; set; }
    }
}
