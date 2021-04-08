namespace Dyalect.Compiler
{
    internal readonly struct TypeHandle
    {
        public readonly int TypeId;

        public readonly bool IsStandard;

        public TypeHandle(int typeId, bool std) => (TypeId, IsStandard) = (typeId, std);
    }
}
