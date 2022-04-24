using System;
namespace Dyalect.Runtime.Types;

public sealed class DyInteropObject : DyObject
{
    internal readonly Type Type;
    internal readonly object Object;

    public override string TypeName => $"{nameof(DyType.Interop)}<{Type.FullName ?? Type.Name}>";
    
    public DyInteropObject(Type type, object obj) : base(DyType.Interop) =>
        (Type, Object) = (type, obj);

    public DyInteropObject(Type obj) : base(DyType.Interop) =>
        (Type, Object) = (obj, obj);

    public override int GetHashCode() => Object.GetHashCode();

    public override object ToObject() => Object;

    public override string ToString() => Object.ToString() ?? "";
}
