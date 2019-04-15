using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyString : DyObject
    {
        public static readonly DyString Empty = new DyString("");
        internal readonly string Value;

        public DyString(string str) : base(StandardType.String)
        {
            Value = str;
        }

        public override object AsObject() => Value;

        public override string ToString() => Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is DyString s)
                return Value.Equals(s.Value);
            else
                return false;
        }

        protected override bool TestEquality(DyObject obj) => Value == obj.AsString();

        public override string AsString() => Value;

        public override long AsInteger()
        {
            long i8;

            if (!long.TryParse(Value, out i8))
                return 0L;

            return i8;
        }

        public override double AsFloat()
        {
            double r8;

            if (!double.TryParse(Value, out r8))
                return 0d;

            return r8;
        }

        public static implicit operator string(DyString str) => str.Value;

        public static implicit operator DyString(string str) => new DyString(str);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.AsInteger();

            if (idx < 0 || idx >= Value.Length)
                return Err.IndexOutOfRange(this.TypeName(ctx), idx).Set(ctx);

            return new DyString(Value[idx].ToString());
        }
    }
}
