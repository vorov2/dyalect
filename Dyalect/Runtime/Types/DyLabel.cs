using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public sealed class DyLabel : DyObject
{
    private List<DyTypeInfo>? typeAnnotations;

    internal bool Mutable;

    public override string TypeName => nameof(Dy.Label);
    
    public string Label { get; }

    public DyObject Value { get; internal set; }

    public DyLabel(string label, DyObject value, bool mutable = false) : base(Dy.Label) =>
        (Label, Value, Mutable) = (label, value, mutable);

    public DyLabel(string label, object value, bool mutable = false) : base(Dy.Label) =>
        (Label, Value, Mutable) = (label, TypeConverter.ConvertFrom(value), mutable);

    public override object ToObject() => Value.ToObject();

    internal DyObject GetItem(DyObject index, ExecutionContext ctx)
    {
        if ((index.TypeId == Dy.Integer && index.GetInteger() == 0) || (index.TypeId == Dy.String && index.GetString() == Label))
            return Value;
        else
            return ctx.IndexOutOfRange(index);
    }

    internal void AddTypeAnnotation(DyTypeInfo ti)
    {
        typeAnnotations ??= new();
        typeAnnotations.Add(ti);
    }

    internal bool VerifyType(int tid)
    {
        if (typeAnnotations is null)
            return true;

        foreach (var t in typeAnnotations)
            if (t.ReflectedTypeId == tid)
                return true;

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Label, Value);

    public override bool Equals(DyObject? other)
    {
        if (other is not DyLabel lab)
            return false;

        if (lab.Label != Label)
            return false;

        return ReferenceEquals(lab.Value, Value) || lab.Value.Equals(Value);
    }

    public override DyObject Clone() => new DyLabel(Label, Value.Clone(), Mutable);
}
