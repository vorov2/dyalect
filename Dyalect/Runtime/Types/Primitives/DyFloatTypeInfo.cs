using Dyalect.Debug;
using System.Globalization;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFloatTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus;

        public override string TypeName => DyTypeNames.Float;

        public override int ReflectedTypeCode => DyType.Float;

        #region Binary Operations
        internal protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() + right.GetFloat());

            if (right.TypeId == DyType.String)
                return ctx.RuntimeContext.Types[DyType.String].Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        internal protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() - right.GetFloat());

            return ctx.InvalidType(right);
        }

        internal protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() * right.GetFloat());

            return ctx.InvalidType(right);
        }

        internal protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() / right.GetFloat());

            return ctx.InvalidType(right);
        }

        internal protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() % right.GetFloat());

            return ctx.InvalidType(right);
        }

        internal protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        internal protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        internal protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        internal protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        internal protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        internal protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        #region Unary Operations
        internal protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.GetFloat());

        internal protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var f = arg.GetFloat();
            return double.IsNaN(f) ? new DyString("NaN")
                : new DyString(f.ToString(CI.NumberFormat));
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch 
            {
                "IsNaN" => Func.Member(name, (c, o) => double.IsNaN(o.GetFloat()) ? DyBool.True : DyBool.False),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Float)
                return obj;

            if (obj.TypeId == DyType.Integer)
                return new DyFloat(obj.GetInteger());

            if (obj.TypeId == DyType.Char || obj.TypeId == DyType.String)
            {
                _ = double.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
                return new DyFloat(i);
            }

            return ctx.InvalidType(obj);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Integer)
                return new DyFloat(obj.GetInteger());

            if (obj.TypeId == DyType.Float)
                return obj;

            if ((obj.TypeId == DyType.Char || obj.TypeId == DyType.String) && double.TryParse(obj.GetString(),
                NumberStyles.Float, CI.NumberFormat, out var i))
                return new DyFloat(i);

            return DyNil.Instance;
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Max" => Func.Static(name, _ => DyFloat.Max),
                "Min" => Func.Static(name, _ => DyFloat.Min),
                "Inf" => Func.Static(name, _ => DyFloat.PositiveInfinity),
                "Default" => Func.Static(name, _ => DyFloat.Zero),
                "Parse" => Func.Static(name, Parse, -1, new Par("value")),
                "Float" => Func.Static(name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
