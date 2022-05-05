namespace Dyalect.Runtime.Types;

public sealed class DyInterop : DyObject
{
    internal readonly Type Type;
    internal readonly object Object;

    public override string TypeName => $"{nameof(Dy.Interop)}<{Type.FullName ?? Type.Name}>";
    
    public DyInterop(Type type, object obj) : base(Dy.Interop) => (Type, Object) = (type, obj);

    public DyInterop(Type obj) : base(Dy.Interop) => (Type, Object) = (obj, obj);

    public override int GetHashCode() => Object.GetHashCode();

    public override object ToObject() => Object;

    public override string ToString() => Object.ToString() ?? "";

    public override bool Equals(DyObject? other) => other is DyInterop i && ReferenceEquals(Object, i.Object);
}
