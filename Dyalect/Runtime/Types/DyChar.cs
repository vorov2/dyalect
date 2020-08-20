using Dyalect.Debug;
using Dyalect.Parser;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private readonly char from;
            private readonly char start;
            private readonly char to;
            private readonly int step;
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
        public static readonly DyChar Max = new DyChar(char.MaxValue);
        public static readonly DyChar Min = new DyChar(char.MinValue);
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

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(Value);
        }

        public override int GetHashCode() => Value.GetHashCode();
    }

    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        public DyCharTypeInfo() : base(DyType.Char)
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

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to, DyObject step)
        {
            if (to.TypeId != DyType.Char)
                return ctx.InvalidType(to);

            var ifrom = self.GetChar();
            var istart = ifrom;
            var ito = to.GetChar();
            var istep = step is DyNil ? 1 : (int)step.GetInteger();

            if (ito <= ifrom)
                istep = -istep;

            return new DyIterator(new DyChar.RangeEnumerator(ifrom, istart, ito, istep));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return name switch
            {
                "to" => DyForeignFunction.Member(name, Range, -1, new Par("max"), new Par("step", DyNil.Instance)),
                "isLower" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLower(c.GetChar())),
                "isUpper" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsUpper(c.GetChar())),
                "isControl" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsControl(c.GetChar())),
                "isDigit" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsDigit(c.GetChar())),
                "isLetter" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetter(c.GetChar())),
                "isLetterOrDigit" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetterOrDigit(c.GetChar())),
                "isWhiteSpace" => DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsWhiteSpace(c.GetChar())),
                "lower" => DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToLower(c.GetChar()))),
                "upper" => DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToUpper(c.GetChar()))),
                "order" => DyForeignFunction.Member(name, (_, c) => DyInteger.Get(c.GetChar())),
                _ => base.GetMember(name, ctx),
            };
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

            if (obj.TypeId == DyType.Float)
                return new DyChar((char)obj.GetFloat());

            return ctx.InvalidType(obj);
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => DyForeignFunction.Static(name, _ => DyChar.Max),
                "min" => DyForeignFunction.Static(name, _ => DyChar.Min),
                "default" => DyForeignFunction.Static(name, _ => DyChar.Empty),
                "Char" => DyForeignFunction.Static(name, CreateChar, -1, new Par("value")),
                _ => base.GetStaticMember(name, ctx)
            };
    }
}
