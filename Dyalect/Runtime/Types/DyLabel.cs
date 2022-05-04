using Dyalect.Parser;
using System.Collections.Generic;
using System.Text;

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

    internal IEnumerable<DyTypeInfo> EnumerateAnnotations()
    {
        if (typeAnnotations is not null)
            foreach (var ta in typeAnnotations)
                yield return ta;
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
