namespace Dyalect.Compiler
{
    public readonly struct ScopeVar
    {
        public static readonly ScopeVar Empty = new ScopeVar(-1, 0);

        public ScopeVar(int address) : this(address, 0)
        {

        }

        public ScopeVar(int address, int data)
        {
            Address = address;
            Data = data;
        }

        public bool IsEmpty()
        {
            return Address == -1;
        }

        public readonly int Address;

        public readonly int Data;
    }
}
