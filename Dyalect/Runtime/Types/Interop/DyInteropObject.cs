namespace Dyalect.Runtime.Types;

public sealed class DyInteropObject : DyObject
{
    internal readonly Type Type;
    internal readonly object Object;

    public override string TypeName => $"{nameof(Dy.Interop)}<{Type.FullName ?? Type.Name}>";
    
    public DyInteropObject(Type type, object obj) : base(Dy.Interop) => (Type, Object) = (type, obj);

    public DyInteropObject(Type obj) : base(Dy.Interop) => (Type, Object) = (obj, obj);

    public override int GetHashCode() => Object.GetHashCode();

    public override object ToObject() => Object;

    public override string ToString() => Object.ToString() ?? "";
}
