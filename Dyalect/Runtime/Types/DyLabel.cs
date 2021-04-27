﻿namespace Dyalect.Runtime.Types
{
    public sealed class DyLabel : DyObject
    {
        public string Label { get; }

        public DyObject Value { get; internal set; }

        public DyLabel(string label, DyObject value) : base(DyType.Label) =>
            (Label, Value) = (label, value);

        protected internal override bool GetBool() => Value.GetBool();

        public override object ToObject() => Value.ToObject();

        protected internal override string GetLabel() => Label;

        protected internal override DyObject GetTaggedValue() => Value;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            (index.TypeId == DyType.Integer && index.GetInteger() == 0) ||
                (index.TypeId == DyType.String && index.GetString() == Label)
                ? Value : ctx.IndexOutOfRange();

        protected internal override bool HasItem(string name, ExecutionContext ctx) => name == Label;

        protected internal override bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value)
        {
            if (index.TypeId == DyType.Integer)
            {
                if (index.GetInteger() != 0)
                {
                    value = null;
                    return false;
                }

                value = Value;
                return true;
            }

            if (index.TypeId == DyType.String)
            {
                if (index.GetString() != Label)
                {
                    value = null;
                    return false;
                }

                value = Value;
                return true;
            }

            ctx.InvalidType(index);
            value = null;
            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(DyObject other)
        {
            if (other is not DyLabel lab)
                return false;

            if (lab.Label != Label)
                return false;

            if (ReferenceEquals(lab.Value, Value)
                || (!(lab.Value is null) && lab.Value.Equals(Value)))
                return true;

            return false;
        }
    }

    internal sealed class DyLabelTypeInfo : DyTypeInfo
    {
        public DyLabelTypeInfo() : base(DyType.Label) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Label;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var lab = (DyLabel)arg;
            return (DyString)(lab.Label + ": " + lab.Value.ToString(ctx).Value);
        }
    }
}
