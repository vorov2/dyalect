using Dyalect.Debug;
using Dyalect.Parser;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
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
            var fst = true;
            var step = ito > ifrom ? 1 : -1;

            char current = ifrom;
            return new DyIterator(new DyIterator.RangeEnumerator(
                () => new DyChar(current),
                () =>
                {
                    if (fst)
                    {
                        fst = false;
                        return true;
                    }

                    current = (char)(current + step);

                    if (ito > istart)
                        return current <= ito;

                    return current >= ito;
                }));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "to")
                return DyForeignFunction.Member(name, Range, -1, new Par("value"));

            if (name == "isLower")
                return DyForeignFunction.Member(name, (_,c) => (DyBool)char.IsLower(c.GetChar()));

            if (name == "isUpper")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsUpper(c.GetChar()));

            if (name == "isControl")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsControl(c.GetChar()));

            if (name == "isDigit")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsDigit(c.GetChar()));

            if (name == "isLetter")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetter(c.GetChar()));

            if (name == "isLetterOrDigit")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsLetterOrDigit(c.GetChar()));

            if (name == "isWhiteSpace")
                return DyForeignFunction.Member(name, (_, c) => (DyBool)char.IsWhiteSpace(c.GetChar()));

            if (name == "lower")
                return DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToLower(c.GetChar())));

            if (name == "upper")
                return DyForeignFunction.Member(name, (_, c) => new DyChar(char.ToUpper(c.GetChar())));

            return null;
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "max")
                return DyForeignFunction.Static(name, c => new DyChar(char.MaxValue));

            if (name == "min")
                return DyForeignFunction.Static(name, c => new DyChar(char.MinValue));

            return null;
        }
    }
}
