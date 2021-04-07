namespace Dyalect.Compiler
{
    public readonly struct ScopeVar
    {
        public static readonly ScopeVar Empty = new(-1, 0);

        public readonly int Address;

        public readonly int Data;

        public ScopeVar(int address) : this(address, 0) { }

        public ScopeVar(int address, int data) => (Address, Data) = (address, data);

        public bool IsEmpty() => Address == -1;
    }
}
