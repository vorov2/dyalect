﻿using Dyalect.Debug;
using System.Globalization;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFloatTypeInfo : DyTypeInfo
    {
        public DyFloatTypeInfo() : base(DyType.Float) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus;

        public override string TypeName => DyTypeNames.Float;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return new DyFloat(left.GetFloat() + right.GetFloat());

            if (right.TypeId is DyType.String)
                return ctx.RuntimeContext.Types[DyType.String].Add(ctx, left, right);

            return ctx.InvalidType(right);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return new DyFloat(left.GetFloat() - right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return new DyFloat(left.GetFloat() * right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return new DyFloat(left.GetFloat() / right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return new DyFloat(left.GetFloat() % right.GetFloat());

            return ctx.InvalidType(right);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;

            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;

            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Float or DyType.Integer)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;

            return ctx.InvalidType(right);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.GetFloat());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var f = arg.GetFloat();
            return double.IsNaN(f) ? new DyString("NaN") : (DyString)f.ToString(CI.NumberFormat);
        }
        #endregion

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "isNaN" => Func.Member(name, (c, o) => double.IsNaN(o.GetFloat()) ? DyBool.True : DyBool.False),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId is DyType.Float)
                return obj;

            if (obj.TypeId is DyType.Integer)
                return new DyFloat(obj.GetInteger());

            if (obj.TypeId is DyType.Char or DyType.String)
            {
                _ = double.TryParse(obj.GetString(), NumberStyles.Float, CI.NumberFormat, out var i);
                return new DyFloat(i);
            }

            return ctx.InvalidType(obj);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId is DyType.Integer)
                return new DyFloat(obj.GetInteger());

            if (obj.TypeId is DyType.Float)
                return obj;

            if (obj.TypeId is DyType.Char or DyType.String && double.TryParse(obj.GetString(),
                NumberStyles.Float, CI.NumberFormat, out var i))
                return new DyFloat(i);

            return DyNil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => Func.Static(name, _ => DyFloat.Max),
                "min" => Func.Static(name, _ => DyFloat.Min),
                "inf" => Func.Static(name, _ => DyFloat.PositiveInfinity),
                "default" => Func.Static(name, _ => DyFloat.Zero),
                "parse" => Func.Static(name, Parse, -1, new Par("value")),
                "Float" => Func.Static(name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
