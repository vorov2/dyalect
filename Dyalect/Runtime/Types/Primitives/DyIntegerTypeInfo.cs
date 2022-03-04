using Dyalect.Debug;
using System.Globalization;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIntegerTypeInfo : DyTypeInfo
    {
        public DyIntegerTypeInfo() : base(DyTypeCode.Integer) { }

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
            if (Is(right, DyInteger.Type))
                return new DyInteger(left.GetInteger() + right.GetInteger());

            if (Is(right, DyFloat.Type))
                return new DyFloat(left.GetFloat() + right.GetFloat());

            if (Is(right, DyString.Type))
                return DyString.Type.Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return new DyInteger(left.GetInteger() - right.GetInteger());

            if (Is(right, DyFloat.Type))
                return new DyFloat(left.GetFloat() - right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return new DyInteger(left.GetInteger() * right.GetInteger());

            if (Is(right, DyFloat.Type))
                return new DyFloat(left.GetFloat() * right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
            {
                var i = right.GetInteger();

                if (i == 0)
                    return ctx.DivideByZero();

                return new DyInteger(left.GetInteger() / i);
            }

            if (Is(right, DyFloat.Type))
                return new DyFloat(left.GetFloat() / right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return new DyInteger(left.GetInteger() % right.GetInteger());

            if (Is(right, DyFloat.Type))
                return new DyFloat(left.GetFloat() % right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (!Is(left, right))
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() << (int)right.GetInteger());
        }

        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (!Is(left, right))
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() >> (int)right.GetInteger());
        }

        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (!Is(left, right))
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() & (int)right.GetInteger());
        }

        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (!Is(left, right))
                return ctx.InvalidType(right);
            return new DyInteger((int)left.GetInteger() | (int)right.GetInteger());
        }

        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (!Is(left, right))
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() ^ (int)right.GetInteger());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() == right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() != right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() > right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() < right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() >= right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (Is(right, DyInteger.Type))
                return left.GetInteger() <= right.GetInteger() ? DyBool.True : DyBool.False;

            if (Is(right, DyFloat.Type))
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyInteger(-arg.GetInteger());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => new DyInteger(~arg.GetInteger());

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(arg.GetInteger().ToString(CI.NumberFormat));
        #endregion

        private DyObject IsMultiple(ExecutionContext ctx, DyObject self, DyObject other)
        {
            if (!Is(other, DyInteger.Type))
                return ctx.InvalidType(other);

            var a = self.GetInteger();
            var b = other.GetInteger();
            return (DyBool)((a % b) == 0);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (name == "isMultiple")
                return Func.Member(name, IsMultiple, -1, new Par("of"));

            return base.InitializeInstanceMember(self, name, ctx);
        }

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (Is(obj, DyInteger.Type))
                return obj;

            if (Is(obj, DyFloat.Type))
                return DyInteger.Get((long)obj.GetFloat());

            if (Is(obj, DyChar.Type) || Is(obj, DyString.Type))
            {
                _ = long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
                return DyInteger.Get(i);
            }

            return ctx.InvalidType(obj);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject obj)
        {
            if (Is(obj, DyInteger.Type))
                return obj;

            if (Is(obj, DyFloat.Type))
                return DyInteger.Get((long)obj.GetFloat());

            if ((Is(obj, DyChar.Type) || Is(obj, DyString.Type)) &&
                long.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i))
                return DyInteger.Get(i);

            return DyNil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(name, _ => DyInteger.Max),
                "min" => Func.Static(name, _ => DyInteger.Min),
                "default" => Func.Static(name, _ => DyInteger.Zero),
                "parse" => Func.Static(name, Parse, -1, new Par("value")),
                "Integer" => Func.Static(name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
