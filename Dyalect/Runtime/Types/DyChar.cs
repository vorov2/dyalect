using Dyalect.Debug;
using Dyalect.Parser;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private char from;
            private char start;
            private char to;
            private int step;
            private bool fst;
            private char current;

            public RangeEnumerator(char from, char start, char to, int step)
            {
                this.from = from;
                this.start = start;
                this.to = to;
                this.step = step;
                this.fst = true;
                this.current = from;
            }

            public DyObject Current => new DyChar(current);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (fst)
                {
                    fst = false;
                    return true;
                }

                current = (char)(current + step);

                if (to > start)
                    return current <= to;

                return current >= to;
            }

            public void Reset()
            {
                current = from;
                fst = true;
            }
        }

        public static readonly DyChar Empty = new DyChar('\0');
        internal readonly char Value;

        public DyChar(char value) : base(DyType.Char)
        {
            Value = value;
        }

        public override object ToObject() => Value;

        protected internal override char GetChar() => Value;

        protected internal override string GetString() => Value.ToString();

        public override string ToString() => Value.ToString();

        public override DyObject Clone() => this;
    }

    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        public DyCharTypeInfo() : base(DyType.Char, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte;

        public override string TypeName => DyTypeNames.Char;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)StringUtil.Escape(arg.GetString(), "'");

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            return new DyString(left.GetChar().ToString() + right.GetChar());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() == right.GetChar() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.String)
                return right.GetString().Length == 1 && left.GetChar() == right.GetString()[0] ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId || right.TypeId == DyType.String)
                return left.GetChar() != right.GetChar() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.String)
                return right.GetString().Length != 1 || left.GetChar() != right.GetString()[0] ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) > 0 ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) < 0 ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }
        #endregion

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to)
        {
            if (to.TypeId != DyType.Char)
                return ctx.InvalidType(DyTypeNames.Char, to);

            var ifrom = self.GetChar();
            var istart = ifrom;
            var ito = to.GetChar();
            var step = ito > ifrom ? 1 : -1;
            return new DyIterator(new DyChar.RangeEnumerator(ifrom, istart, ito, step));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name) {
                case "to":
                    return DyForeignFunction.Member(name, Range, -1, new Par("value"));
                case "isLower":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLower(c.GetChar()));
                case "isUpper":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsUpper(c.GetChar()));
                case "isControl":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsControl(c.GetChar()));
                case "isDigit":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsDigit(c.GetChar()));
                case "isLetter":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetter(c.GetChar()));
                case "isLetterOrDigit":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetterOrDigit(c.GetChar()));
                case "isWhiteSpace":
                    return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsWhiteSpace(c.GetChar()));
                case "lower":
                    return DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToLower(c.GetChar())));
                case "upper":
                    return DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToUpper(c.GetChar())));
                case "order":
                    return DyForeignFunction.Member(name, (_, c) => DyInteger.Get(c.GetChar()));
                default:
                    return null;
            }
        }

        private DyObject CreateChar(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Char)
                return obj;

            if (obj.TypeId == DyType.String)
            {
                var str = obj.ToString();
                return str.Length > 0 ? new DyChar(str[0]) : DyChar.Empty;
            }

            if (obj.TypeId == DyType.Integer)
                return new DyChar((char)obj.GetInteger());

            return ctx.InvalidType(DyTypeNames.Integer, obj);
        }

        protected override DyObject GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "max")
                return DyForeignFunction.Static(name, c => new DyChar(char.MaxValue));

            if (name == "min")
                return DyForeignFunction.Static(name, c => new DyChar(char.MinValue));

            if (name == "new")
                return DyForeignFunction.Static(name, CreateChar, -1, new Par("value"));

            return null;
        }
    }
}
