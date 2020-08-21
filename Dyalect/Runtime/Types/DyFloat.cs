using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyFloat : DyObject
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private readonly double from;
            private readonly double start;
            private readonly double? to;
            private readonly double step;
            private bool fst;
            private double current;

            public RangeEnumerator(double from, double start, double? to, double step)
            {
                this.from = from;
                this.start = start;
                this.to = to;
                this.step = step;
                this.fst = true;
                this.current = from;
            }

            public DyObject Current => new DyFloat(current);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (fst)
                {
                    fst = false;
                    return true;
                }

                current += step;

                if (to == null)
                    return true;

                if (to > start)
                    return current <= to;

                return current >= to;
            }

            public void Reset()
            {
                current = from;
                fst = true;
            }
        }

        public static readonly DyFloat Zero = new DyFloat(0D);
        public static readonly DyFloat One = new DyFloat(1D);
        public static readonly DyFloat NaN = new DyFloat(double.NaN);
        public static readonly DyFloat PositiveInfinity = new DyFloat(double.PositiveInfinity);
        public static readonly DyFloat NegativeInfinity = new DyFloat(double.NegativeInfinity);
        public static readonly DyFloat Epsilon = new DyFloat(double.Epsilon);
        public static readonly DyFloat Min = new DyFloat(double.MinValue);
        public static readonly DyFloat Max = new DyFloat(double.MaxValue);

        private readonly double value;

        public DyFloat(double value) : base(DyType.Float)
        {
            this.value = value;
        }

        public override int GetHashCode() => value.GetHashCode();

        public override bool Equals(DyObject obj) =>
            obj is DyFloat f && value == f.value;

        public override string ToString() => value.ToString(CI.NumberFormat);

        public override object ToObject() => value;

        internal protected override double GetFloat() => value;

        protected internal override long GetInteger() => (long)value;

        protected internal override bool GetBool() => value > .00001d;

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(value);
        }
    }

    internal sealed class DyFloatTypeInfo : DyTypeInfo
    {
        public DyFloatTypeInfo() : base(DyType.Float)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
            | SupportedOperations.Sub | SupportedOperations.Div | SupportedOperations.Mul | SupportedOperations.Rem
            | SupportedOperations.Neg | SupportedOperations.Plus;

        public override string TypeName => DyTypeNames.Float;

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() + right.GetFloat());
            else
                return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() - right.GetFloat());
            else
                return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() * right.GetFloat());
            else
                return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() / right.GetFloat());
            else
                return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return new DyFloat(left.GetFloat() % right.GetFloat());
            else
                return base.RemOp(left, right, ctx);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Float || right.TypeId == DyType.Integer)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LteOp(left, right, ctx);
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

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to, DyObject step)
        {
            if (to.TypeId != DyType.Float && to.TypeId != DyType.Integer && to.TypeId != DyType.Nil)
                return ctx.InvalidType(to);

            var ifrom = self.GetFloat();
            var istart = ifrom;
            var ito = to.TypeId == DyType.Nil ? null : (double?)to.GetFloat();
            var istep = step.TypeId == DyType.Nil ? 1.0D : step.GetFloat();

            if (ito <= ifrom)
                istep = -Math.Abs(istep);

            if (istep == 0
                || (istep < 0 && ito != null && ito > ifrom)
                || (istep > 0 && ito != null && ito < ifrom))
                return ctx.InvalidRange();

            return new DyIterator(new DyFloat.RangeEnumerator(ifrom, istart, ito, istep));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "to" => DyForeignFunction.Member(name, Range, -1, new Par("max"), new Par("step", DyNil.Instance)),
                "isNaN" => DyForeignFunction.Member(name, (c, o) => double.IsNaN(o.GetFloat()) ? DyBool.True : DyBool.False),
                _ => base.GetMember(name, ctx)
            };

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Float)
                return obj;
            else if (obj.TypeId == DyType.Integer)
                return new DyFloat(obj.GetInteger());
            else if (obj.TypeId == DyType.Char || obj.TypeId == DyType.String)
            {
                double.TryParse(obj.GetString(), out var i);
                return new DyFloat(i);
            }

            return ctx.InvalidType(obj);
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => DyForeignFunction.Static(name, _ => DyFloat.Max),
                "min" => DyForeignFunction.Static(name, _ => DyFloat.Min),
                "inf" => DyForeignFunction.Static(name, _ => DyFloat.PositiveInfinity),
                "default" => DyForeignFunction.Static(name, _ => DyFloat.Zero),
                "Float" => DyForeignFunction.Static(name, Convert, -1, new Par("value")),
                _ => base.GetStaticMember(name, ctx)
            };
    }
}
