using Dyalect.Runtime.Types;
namespace Dyalect.Runtime;

public struct RuntimeVar
{
    public readonly string Name;
    public readonly DyObject Value;

    public RuntimeVar(string name, DyObject value) => (Name, Value) = (name, value);
}
