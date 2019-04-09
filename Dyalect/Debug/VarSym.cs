namespace Dyalect.Debug
{
    public sealed class VarSym
    {
        public VarSym(string name, int address, int offset, int scope, int flags, int data)
        {
            Name = name;
            Address = address;
            Offset = offset;
            Scope = scope;
            Flags = flags;
            Data = data;
        }

        public VarSym()
        {

        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; set; }

        public int Address { get; set; }

        public int Offset { get; set; }

        public int Scope { get; set; }

        public int Flags { get; set; }

        public int Data { get; set; }
    }
}
