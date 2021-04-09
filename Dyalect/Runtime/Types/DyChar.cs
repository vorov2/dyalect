using Dyalect.Debug;
using Dyalect.Parser;
using System;
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
            private readonly bool inf;
            private bool fst;
            private char current;

            public RangeEnumerator(char from, char start, char? to, int step)
            {
                this.from = from;
                this.start = start;
                this.to = to ?? '\0';
                this.step = step;
                this.inf = to == null;
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

                if (inf)
                    return true;

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

        public static readonly DyChar Empty = new('\0');
        public static readonly DyChar Max = new(char.MaxValue);
        public static readonly DyChar Min = new(char.MinValue);
        internal readonly char Value;

        public DyChar(char value) : base(DyType.Char) => Value = value;

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
        public DyCharTypeInfo() : base(DyType.Char) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte;

        public override string TypeName => DyTypeNames.Char;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)StringUtil.Escape(arg.GetString(), "'");

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            new DyString(left.GetChar().ToString() + right.GetChar());

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
            if (to.TypeId != DyType.Char && to.TypeId != DyType.Nil)
                return ctx.InvalidType(to);

            var ifrom = self.GetChar();
            var istart = ifrom;
            var istep = step.TypeId == DyType.Nil ? 1 : (int)step.GetInteger();

            if (to == DyNil.Instance)
                return new DyIterator(new DyChar.RangeEnumerator(ifrom, istart, null, istep));

            var ito = to.GetChar();

            if (ito <= ifrom)
                istep = -Math.Abs(istep);
            
            if (istep == 0
                || (istep < 0 && ito > ifrom)
                || (istep > 0 && ito < ifrom))
                return ctx.InvalidRange();

            return new DyIterator(new DyChar.RangeEnumerator(ifrom, istart, ito, istep));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return name switch
            {
                "range" => DyForeignFunction.Member(name, Range, -1, new Par("to", DyNil.Instance), new Par("step", DyNil.Instance)),
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
