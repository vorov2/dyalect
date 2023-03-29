using Dyalect.Parser.Model;

namespace Dyalect.Compiler;

internal sealed class TypeInfo
{
    public TypeInfo(DTypeDeclaration dec, UnitInfo unit) => (Declaration, Unit) = (dec, unit);

    public UnitInfo Unit { get; }

    public DTypeDeclaration Declaration { get; }
}
