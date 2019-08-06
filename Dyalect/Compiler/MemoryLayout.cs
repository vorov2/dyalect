namespace Dyalect.Compiler
{
    //Represents a memory structure for an actual (runtime) lexical scope (e.g. global
    //or function). It is used for addressing.
    public sealed class MemoryLayout
    {
        internal MemoryLayout(int size, int stackSize, int address)
        {
            Size = size;
            StackSize = stackSize;
            Address = address;
        }

        //Size of operational stack
        public int StackSize { get; }

        //Number of local variables
        public int Size { get; }

        //Address (ASM code offset)
        public int Address { get; internal set; }
    }
}