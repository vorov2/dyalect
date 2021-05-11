using Dyalect.Parser.Model;

namespace Dyalect.Compiler
{
    internal sealed class TypeInfo
    {
        public TypeInfo(int handle, DTypeDeclaration dec, UnitInfo unit) =>
            (TypeId, Declaration, Unit) = (handle, dec, unit);

        public int TypeId { get; }

        public UnitInfo Unit { get; }

        public DTypeDeclaration Declaration { get; }
    }
}
