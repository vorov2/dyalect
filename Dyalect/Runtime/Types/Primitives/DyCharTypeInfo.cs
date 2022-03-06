using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        public DyChar Empty => new(this, '\0');
        public DyChar Max => new(this, char.MaxValue);
        public DyChar Min => new(this, char.MinValue);

        public DyCharTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Char) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Add | SupportedOperations.Sub
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte;

        public override string TypeName => DyTypeNames.Char;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.GetString());

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyChar(this, (char)(left.GetChar() + right.GetInteger()));

            if (right.DecType.TypeCode == DyTypeCode.Char)
                return new DyString(left.GetString() + right.GetString());

            if (right.DecType.TypeCode == DyTypeCode.String)
                return DyString.Type.Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyChar(this, (char)(left.GetChar() - right.GetInteger()));

            if (right.DecType.TypeCode == DyTypeCode.Char)
                return ctx.RuntimeContext.Integer.Get(left.GetChar() - right.GetChar());

            return ctx.InvalidType(right);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode == right.DecType.TypeCode)
                return left.GetChar() == right.GetChar() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.String)
            {
                var str = right.GetString();
                return str.Length == 1 && left.GetChar() == str[0] ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
            }

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode == right.DecType.TypeCode)
                return left.GetChar() != right.GetChar() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.String)
            {
                var str = right.GetString();
                return str.Length != 1 || left.GetChar() != str[0] ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
            }

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode == right.DecType.TypeCode)
                return left.GetChar().CompareTo(right.GetChar()) > 0 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.String)
                return left.GetString().CompareTo(right.GetString()) > 0 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode == right.DecType.TypeCode)
                return left.GetChar().CompareTo(right.GetChar()) < 0 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.String)
                return left.GetString().CompareTo(right.GetString()) < 0 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "isLower" => Func.Member(ctx, name, (_, c) => char.IsLower(c.GetChar()) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isUpper" => Func.Member(ctx, name, (_, c) => char.IsUpper(c.GetChar()) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isControl" => Func.Member(ctx, name, (_, c) => char.IsControl(c.GetChar()) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isDigit" => Func.Member(ctx, name, (_, c) => char.IsDigit(c.GetChar())? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isLetter" => Func.Member(ctx, name, (_, c) => char.IsLetter(c.GetChar())? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isLetterOrDigit" => Func.Member(ctx, name, (_, c) => char.IsLetterOrDigit(c.GetChar())? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "isWhiteSpace" => Func.Member(ctx, name, (_, c) => char.IsWhiteSpace(c.GetChar())? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                "lower" => Func.Member(ctx, name, (_, c) => new DyChar(this, char.ToLower(c.GetChar()))),
                "upper" => Func.Member(ctx, name, (_, c) => new DyChar(this, char.ToUpper(c.GetChar()))),
                "order" => Func.Member(ctx, name, (_, c) => ctx.RuntimeContext.Integer.Get(c.GetChar())),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        private DyObject CreateChar(ExecutionContext ctx, DyObject obj)
        {
            if (obj.DecType.TypeCode == DyTypeCode.Char)
                return obj;

            if (obj.DecType.TypeCode == DyTypeCode.String)
            {
                var str = obj.ToString();
                return str.Length > 0 ? new(this, str[0]) : Empty;
            }

            if (obj.DecType.TypeCode == DyTypeCode.Integer)
                return new DyChar(this, (char)obj.GetInteger());

            if (obj.DecType.TypeCode == DyTypeCode.Float)
                return new DyChar(this, (char)obj.GetFloat());

            return ctx.InvalidType(obj);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(ctx, name, _ => Max),
                "min" => Func.Static(ctx, name, _ => Min),
                "default" => Func.Static(ctx, name, _ => Empty),
                "Char" => Func.Static(ctx, name, CreateChar, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
