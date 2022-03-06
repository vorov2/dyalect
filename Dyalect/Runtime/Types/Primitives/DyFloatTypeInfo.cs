using Dyalect.Debug;
using System.Globalization;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFloatTypeInfo : DyTypeInfo
    {
        public DyFloat Zero => new(this, 0D);
        public DyFloat One => new(this, 1D);
        public DyFloat NaN => new(this, double.NaN);
        public DyFloat PositiveInfinity => new(this, double.PositiveInfinity);
        public DyFloat NegativeInfinity => new(this, double.NegativeInfinity);
        public DyFloat Epsilon => new(this, double.Epsilon);
        public DyFloat Min => new(this, double.MinValue);
        public DyFloat Max => new(this, double.MaxValue);

        public DyFloatTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Float) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus;

        public override string TypeName => DyTypeNames.Float;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, left.GetFloat() + right.GetFloat());

            if (right.DecType.TypeCode == DyTypeCode.String)
                return ctx.RuntimeContext.String.Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, left.GetFloat() - right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, left.GetFloat() * right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, left.GetFloat() / right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, left.GetFloat() % right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() == right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() != right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() > right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() < right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() >= right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.DecType.TypeCode == DyTypeCode.Float || right.DecType.TypeCode == DyTypeCode.Integer)
                return left.GetFloat() <= right.GetFloat() ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(this, -arg.GetFloat());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var f = arg.GetFloat();
            return double.IsNaN(f) ? new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, "NaN")
                : new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, f.ToString(CI.NumberFormat));
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch 
            {
                "isNaN" => Func.Member(ctx, name, (c, o) => double.IsNaN(o.GetFloat()) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.DecType.TypeCode == DyTypeCode.Float)
                return obj;

            if (obj.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, obj.GetInteger());

            if (obj.DecType.TypeCode == DyTypeCode.Char || obj.DecType.TypeCode == DyTypeCode.String)
            {
                _ = double.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
                return new DyFloat(this, i);
            }

            return ctx.InvalidType(obj);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject obj)
        {
            if (obj.DecType.TypeCode == DyTypeCode.Integer)
                return new DyFloat(this, obj.GetInteger());

            if (obj.DecType.TypeCode == DyTypeCode.Float)
                return obj;

            if ((obj.DecType.TypeCode == DyTypeCode.Char || obj.DecType.TypeCode == DyTypeCode.String) && double.TryParse(obj.GetString(),
                NumberStyles.Float, CI.NumberFormat, out var i))
                return new DyFloat(this, i);

            return ctx.RuntimeContext.Nil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(ctx, name, _ => Max),
                "min" => Func.Static(ctx, name, _ => Min),
                "inf" => Func.Static(ctx, name, _ => PositiveInfinity),
                "default" => Func.Static(ctx, name, _ => Zero),
                "parse" => Func.Static(ctx, name, Parse, -1, new Par("value")),
                "Float" => Func.Static(ctx, name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
