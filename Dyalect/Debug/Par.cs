using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;

namespace Dyalect.Debug;

public readonly struct Par
{
    public readonly string Name;
    public readonly bool IsVarArg;
    public readonly DyObject? Value;
    public readonly TypeAnnotation? TypeAnnotation;

    internal Par(string name, DyObject? val, bool isVarArg, TypeAnnotation? ta) =>
        (Name, Value, IsVarArg, TypeAnnotation) = (name, val, isVarArg, ta);

    internal Par(string name, DyObject? val, bool isVarArg) =>
        (Name, Value, IsVarArg, TypeAnnotation) = (name, val, isVarArg, null);

    public Par(string name, ParKind kind) => (Name, Value, IsVarArg, TypeAnnotation) = (name, null, kind == ParKind.VarArg, null);

    public Par(string name, ParKind kind, DyObject? value)
    {
        value ??= DyNil.Instance;
        (Name, Value, IsVarArg, TypeAnnotation) = (name, value, kind == ParKind.VarArg, null);
    }

    public Par(string name, DyObject? value)
    {
        value ??= DyNil.Instance;
        (Name, Value, IsVarArg, TypeAnnotation) = (name, value, false, null);
    }

    public Par(string name) => (Name, Value, IsVarArg, TypeAnnotation) = (name, null, false, null);

    public Par(string name, int value) => (Name, Value, IsVarArg, TypeAnnotation) = (name, DyInteger.Get(value), false, null);

    public Par(string name, double value) => (Name, Value, IsVarArg, TypeAnnotation) = (name, new DyFloat(value), false, null);

    public Par(string name, string value) => (Name, Value, IsVarArg, TypeAnnotation) = (name, new DyString(value), false, null);

    public Par(string name, char value) => (Name, Value, IsVarArg, TypeAnnotation) = (name, new DyChar(value), false, null);

    public Par(string name, bool value) => (Name, Value, IsVarArg, TypeAnnotation) = (name, value ? True : False, false, null);

    public override string ToString() => Name;

    public static implicit operator Par(string name) => new(name);
}
