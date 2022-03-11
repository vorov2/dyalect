using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        internal bool Mutable;

        internal DyTypeInfo? TypeAnnotation;

        public string Label { get; }

        public DyObject Value { get; internal set; }

        public DyLabel(string label, DyObject value, bool mutable = false, DyTypeInfo? typeAnnotation = null) : base(DyType.Label) =>
            (Label, Value, Mutable, TypeAnnotation) = (label, value, mutable, typeAnnotation);

        public DyLabel(string label, object value, bool mutable = false, DyTypeInfo? typeAnnotation = null) : base(DyType.Label) =>
            (Label, Value, Mutable, TypeAnnotation) = (label, TypeConverter.ConvertFrom(value), mutable, typeAnnotation);

        protected internal override bool GetBool(ExecutionContext ctx) => Value.GetBool(ctx);

        public override object ToObject() => Value.ToObject();

        protected internal override string GetLabel() => Label;

        protected internal override DyObject GetTaggedValue() => Value;

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if ((index.TypeId == DyType.Integer && index.GetInteger() == 0) || (index.TypeId == DyType.String && index.GetString() == Label))
                return Value;
            else
                return ctx.IndexOutOfRange();
        }

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
