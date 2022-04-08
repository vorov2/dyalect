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
            | SupportedOperations.Neg | SupportedOperations.Plus | SupportedOperations.Lit;

        public override string TypeName => DyTypeNames.Float;

        public override int ReflectedTypeId => DyType.Float;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() + right.GetFloat());

            if (right.TypeId == DyType.String)
                return ctx.RuntimeContext.Types[DyType.String].Add(ctx, left, right);

            return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() - right.GetFloat());

            return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() * right.GetFloat());

            return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() / right.GetFloat());

            return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() % right.GetFloat());

            return base.RemOp(left, right, ctx);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;

            return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;

            return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;

            return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;

            return base.LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.GetFloat());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            var f = arg.GetFloat();
            return new DyString(f.ToString(CI.NumberFormat));
        }

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch 
            {
                Method.IsNaN => Func.Member(name, (c, o) => double.IsNaN(o.GetFloat()) ? DyBool.True : DyBool.False),
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

            return ctx.InvalidType(DyType.Float, DyType.Integer, DyType.Char, DyType.String, obj);
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
                Method.Max => Func.Static(name, _ => DyFloat.Max),
                Method.Min => Func.Static(name, _ => DyFloat.Min),
                Method.Inf => Func.Static(name, _ => DyFloat.PositiveInfinity),
                Method.Default => Func.Static(name, _ => DyFloat.Zero),
                Method.Parse => Func.Static(name, Parse, -1, new Par("value")),
                Method.Float => Func.Static(name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get((long)self.GetFloat()),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
