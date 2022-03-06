using Dyalect.Debug;
using System.Globalization;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIntegerTypeInfo : DyTypeInfo
    {
        public DyInteger Zero => new(this, 0L);
        public DyInteger MinusOne => new(this, -1L);
        public DyInteger One => new(this, 1L);
        public DyInteger Two => new(this, 2L);
        public DyInteger Three => new(this, 3L);
        public DyInteger Max => new(this, long.MaxValue);
        public DyInteger Min => new(this, long.MinValue);

        public DyInteger Get(long i) =>
            i switch
            {
                -1 => MinusOne,
                0 => Zero,
                1 => One,
                2 => Two,
                3 => Three,
                _ => new DyInteger(this, i)
            };

        public DyIntegerTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Integer) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus | SupportedOperations.And | SupportedOperations.Or
            | SupportedOperations.Xor | SupportedOperations.BitNot | SupportedOperations.Shl | SupportedOperations.Shr;

        public override string TypeName => DyTypeNames.Integer;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyInteger(this, left.GetInteger() + right.GetInteger());

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return new DyFloat(this, left.GetFloat() + right.GetFloat());

            if (right.DecType.TypeCode == DyTypeCode.String)
                return ctx.RuntimeContext.String.Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyInteger(this, left.GetInteger() - right.GetInteger());

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return new DyFloat(this, left.GetFloat() - right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyInteger(this, left.GetInteger() * right.GetInteger());

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return new DyFloat(this, left.GetFloat() * right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
            {
                var i = right.GetInteger();

                if (i == 0)
                    return ctx.DivideByZero();

                return new DyInteger(this, left.GetInteger() / i);
            }

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return new DyFloat(this, left.GetFloat() / right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyInteger(this, left.GetInteger() % right.GetInteger());

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return new DyFloat(this, left.GetFloat() % right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode != right.DecType.TypeCode)
                return ctx.InvalidType(right);
            return new DyInteger(this, left.GetInteger() << (int)right.GetInteger());
        }

        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode != right.DecType.TypeCode)
                return ctx.InvalidType(right);
            return new DyInteger(this, left.GetInteger() >> (int)right.GetInteger());
        }

        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode != right.DecType.TypeCode)
                return ctx.InvalidType(right);
            return new DyInteger(this, left.GetInteger() & (int)right.GetInteger());
        }

        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode != right.DecType.TypeCode)
                return ctx.InvalidType(right);
            return new DyInteger(this, (int)left.GetInteger() | (int)right.GetInteger());
        }

        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.DecType.TypeCode != right.DecType.TypeCode)
                return ctx.InvalidType(right);
            return new DyInteger(this, left.GetInteger() ^ (int)right.GetInteger());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() == right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() == right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() != right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() != right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() > right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() > right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() < right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() < right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() >= right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() >= right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetInteger() <= right.GetInteger() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            if (right.DecType.TypeCode == DyTypeCode.Float)
                return left.GetFloat() <= right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyInteger(this, -arg.GetInteger());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => new DyInteger(this, ~arg.GetInteger());

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, arg.GetInteger().ToString(CI.NumberFormat));
        #endregion

        private DyObject IsMultiple(ExecutionContext ctx, DyObject self, DyObject other)
        {
            if (other.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(other);

            var a = self.GetInteger();
            var b = other.GetInteger();
            return (a % b) == 0 ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (name == "isMultiple")
                return Func.Member(ctx, name, IsMultiple, -1, new Par("of"));

            return base.InitializeInstanceMember(self, name, ctx);
        }

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.DecType.TypeCode == DyTypeCode.Integer)
                return obj;

            if (obj.DecType.TypeCode == DyTypeCode.Float)
                return Get((long)obj.GetFloat());

            if (obj.DecType.TypeCode == DyTypeCode.Char || obj.DecType.TypeCode == DyTypeCode.String)
            {
                _ = long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
                return Get(i);
            }

            return ctx.InvalidType(obj);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject obj)
        {
            if (obj.DecType.TypeCode == DyTypeCode.Integer)
                return obj;

            if (obj.DecType.TypeCode == DyTypeCode.Float)

                return Get((long)obj.GetFloat());

            if ((obj.DecType.TypeCode == DyTypeCode.Char || obj.DecType.TypeCode == DyTypeCode.String) &&
                long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i))
                return Get(i);

            return ctx.RuntimeContext.Nil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(ctx, name, _ => Max),
                "min" => Func.Static(ctx, name, _ => Min),
                "default" => Func.Static(ctx, name, _ => Zero),
                "parse" => Func.Static(ctx, name, Parse, -1, new Par("value")),
                "Integer" => Func.Static(ctx, name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
