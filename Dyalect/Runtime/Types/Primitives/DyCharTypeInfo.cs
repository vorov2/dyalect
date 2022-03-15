using Dyalect.Debug;
using Dyalect.Parser;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Add | SupportedOperations.Sub
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Lit;

        public override string TypeName => DyTypeNames.Char;

        public override int ReflectedTypeId => DyType.Char;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(arg.GetString());

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => new DyString(StringUtil.Escape(arg.GetString(), "'"));

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return new DyChar((char)(left.GetChar() + right.GetInteger()));

            if (right.TypeId == DyType.Char)
                return new DyString(left.GetString() + right.GetString());

            if (right.TypeId == DyType.String)
                return ctx.RuntimeContext.String.Add(ctx, left, right);

            return ctx.InvalidType(DyType.Integer, DyType.Char, DyType.String, right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return new DyChar((char)(left.GetChar() - right.GetInteger()));

            if (right.TypeId == DyType.Char)
                return DyInteger.Get(left.GetChar() - right.GetChar());

            return ctx.InvalidType(DyType.Integer, DyType.Char, right);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() == right.GetChar() ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
            {
                var str = right.GetString();
                return str.Length == 1 && left.GetChar() == str[0] ? DyBool.True : DyBool.False;
            }

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() != right.GetChar() ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
            {
                var str = right.GetString();
                return str.Length != 1 || left.GetChar() != str[0] ? DyBool.True : DyBool.False;
            }

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) > 0 ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;

            return ctx.InvalidType(DyType.Char, DyType.String, right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) < 0 ? DyBool.True : DyBool.False;

            if (right.TypeId == DyType.String)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;

            return ctx.InvalidType(DyType.Char, DyType.String, right);
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                Method.IsLower => Func.Member(name, (_, c) => char.IsLower(c.GetChar()) ? DyBool.True : DyBool.False),
                Method.IsUpper => Func.Member(name, (_, c) => char.IsUpper(c.GetChar()) ? DyBool.True : DyBool.False),
                Method.IsControl => Func.Member(name, (_, c) => char.IsControl(c.GetChar()) ? DyBool.True : DyBool.False),
                Method.IsDigit => Func.Member(name, (_, c) => char.IsDigit(c.GetChar())? DyBool.True : DyBool.False),
                Method.IsLetter => Func.Member(name, (_, c) => char.IsLetter(c.GetChar())? DyBool.True : DyBool.False),
                Method.IsLetterOrDigit => Func.Member(name, (_, c) => char.IsLetterOrDigit(c.GetChar())? DyBool.True : DyBool.False),
                Method.IsWhiteSpace => Func.Member(name, (_, c) => char.IsWhiteSpace(c.GetChar())? DyBool.True : DyBool.False),
                Method.Lower => Func.Member(name, (_, c) => new DyChar(char.ToLower(c.GetChar()))),
                Method.Upper => Func.Member(name, (_, c) => new DyChar(char.ToUpper(c.GetChar()))),
                Method.Order => Func.Member(name, (_, c) => DyInteger.Get(c.GetChar())),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        private DyObject CreateChar(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Char)
                return obj;

            if (obj.TypeId == DyType.String)
            {
                var str = obj.ToString();
                return str is not null && str.Length > 0 ? new(str[0]) : DyChar.Empty;
            }

            if (obj.TypeId == DyType.Integer)
                return new DyChar((char)obj.GetInteger());

            if (obj.TypeId == DyType.Float)
                return new DyChar((char)obj.GetFloat());

            return ctx.InvalidCast(obj.GetTypeInfo(ctx).TypeName, DyTypeNames.Char);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Max => Func.Static(name, _ => DyChar.Max),
                Method.Min => Func.Static(name, _ => DyChar.Min),
                Method.Default => Func.Static(name, _ => DyChar.Empty),
                Method.Char => Func.Static(name, CreateChar, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get(self.GetChar()),
                DyType.Float => new DyFloat(self.GetChar()),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
