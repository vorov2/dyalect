﻿using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    public sealed class DyFloat : DyObject
    {
        public static readonly DyFloat Zero = new DyFloat(0D);
        public static readonly DyFloat One = new DyFloat(1D);
        public static readonly DyFloat NaN = new DyFloat(double.NaN);
        public static readonly DyFloat PositiveInfinity = new DyFloat(double.PositiveInfinity);
        public static readonly DyFloat NegativeInfinity = new DyFloat(double.NegativeInfinity);
        public static readonly DyFloat Epsilon = new DyFloat(double.Epsilon);
        public static readonly DyFloat Min = new DyFloat(double.MinValue);
        public static readonly DyFloat Max = new DyFloat(double.MaxValue);

        private readonly double value;

        public DyFloat(double value) : base(StandardType.Float)
        {
            this.value = value;
        }

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is DyFloat f)
                return value == f.value;
            else
                return false;
        }

        public override string ToString() => value.ToString(CI.NumberFormat);

        public override object ToObject() => value;

        internal protected override double GetFloat() => value;

        protected internal override bool GetBool() => value > .00001d;
    }

    internal sealed class DyFloatTypeInfo : DyTypeInfo
    {
        public DyFloatTypeInfo() : base(StandardType.Float, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus;

        public override string TypeName => StandardType.FloatName;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.GetFloat() + right.GetFloat());
            else
                return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.GetFloat() - right.GetFloat());
            else
                return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.GetFloat() * right.GetFloat());
            else
                return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.GetFloat() / right.GetFloat());
            else
                return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.GetFloat() % right.GetFloat());
            else
                return base.RemOp(left, right, ctx);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.GetFloat());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)arg.GetFloat().ToString(CI.NumberFormat);
        #endregion

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to)
        {
            if (to.TypeId != StandardType.Float)
                return Err.InvalidType(StandardType.FloatName, to.TypeName(ctx)).Set(ctx);

            var ifrom = self.GetFloat();
            var istart = ifrom;
            var ito = to.GetFloat();
            var fst = true;
            var step = ito > ifrom ? 1.0 : -1.0;

            double current = ifrom;
            return new DyIterator(new DyIterator.RangeEnumerator(
                () => new DyFloat(current),
                () =>
                {
                    if (fst)
                    {
                        fst = false;
                        return true;
                    }

                    current = current + step;

                    if (ito > istart)
                        return current <= ito;

                    return current >= ito;
                }));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "to")
                return DyForeignFunction.Member(name, Range, -1, new Par("value"));

            return null;
        }
    }
}
