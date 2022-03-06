using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        internal static readonly DyLabelTypeInfo Type = new();

        internal bool Mutable;

        internal DyTypeInfo? TypeAnnotation;

        public string Label { get; }

        public DyObject Value { get; internal set; }

        public DyLabel(DyTypeInfo typeInfo, string label, DyObject value) : base(typeInfo) => (Label, Value) = (label, value);

        protected internal override bool GetBool() => Value.GetBool();

        public override object ToObject() => Value.ToObject();

        protected internal override string GetLabel() => Label;

        protected internal override DyObject GetTaggedValue() => Value;

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            (index.DecType.TypeCode == DyTypeCode.Integer && index.GetInteger() == 0) 
            || (index.DecType.TypeCode == DyTypeCode.String && index.GetString() == Label)
                ? Value : ctx.IndexOutOfRange();

        protected internal override bool HasItem(string name, ExecutionContext ctx) => name == Label;

        public override int GetHashCode() => HashCode.Combine(Label, Value);

        public override bool Equals(DyObject? other)
        {
            if (other is not DyLabel lab)
                return false;

            if (lab.Label != Label)
                return false;

            return ReferenceEquals(lab.Value, Value) || lab.Value.Equals(Value);
        }
    }
}
