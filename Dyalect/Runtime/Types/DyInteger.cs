using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private long from;
            private long start;
            private long to;
            private long step;
            private bool fst;
            private long current;

            public RangeEnumerator(long from, long start, long to, long step)
            {
                this.from = from;
                this.start = start;
                this.to = to;
                this.step = step;
                this.fst = true;
                this.current = from;
            }

            public DyObject Current => Get(current);

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (fst)
                {
                    fst = false;
                    return true;
                }

                current = current + step;

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

        public static readonly DyInteger Zero = new DyInteger(0L);
        public static readonly DyInteger MinusOne = new DyInteger(-1L);
        public static readonly DyInteger One = new DyInteger(1L);
        public static readonly DyInteger Two = new DyInteger(2L);
        public static readonly DyInteger Three = new DyInteger(3L);
        public static readonly DyInteger Max = new DyInteger(long.MaxValue);
        public static readonly DyInteger Min = new DyInteger(long.MinValue);

        private readonly long value;

        public DyInteger(long value) : base(DyType.Integer)
        {
            this.value = value;
        }

        public static DyInteger Get(long i)
        {
            if (i == -1)
                return MinusOne;
            if (i == 0)
                return Zero;
            if (i == 1)
                return One;
            if (i == 2)
                return Two;
            if (i == 3)
                return Three;
            return new DyInteger(i);
        }

        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => value.ToString(CI.Default);

        public override bool Equals(object obj)
        {
            if (obj is DyInteger i)
                return value == i.value;
            else
                return false;
        }

        public override object ToObject() => value;

        protected internal override bool GetBool() => value != 0;

        internal protected override double GetFloat() => value;

        internal protected override long GetInteger() => value;

        public override DyObject Clone() => this;
    }


    internal sealed class DyIntegerTypeInfo : DyTypeInfo
    {
        public DyIntegerTypeInfo() : base(DyType.Integer, false)
        {

        }

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
            if (right.TypeId == DyType.Integer)
                return new DyInteger(left.GetInteger() + right.GetInteger());
            else if (right.TypeId == DyType.Float)
                return new DyFloat(left.GetFloat() + right.GetFloat());
            else
                return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return new DyInteger(left.GetInteger() - right.GetInteger());
            else if (right.TypeId == DyType.Float)
                return new DyFloat(left.GetFloat() - right.GetFloat());
            else
                return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return new DyInteger(left.GetInteger() * right.GetInteger());
            else if (right.TypeId == DyType.Float)
                return new DyFloat(left.GetFloat() * right.GetFloat());
            else
                return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
            {
                var i = right.GetInteger();

                if (i == 0)
                    return ctx.DivideByZero();

                return new DyInteger(left.GetInteger() / i);
            }
            else if (right.TypeId == DyType.Float)
                return new DyFloat(left.GetFloat() / right.GetFloat());
            else
                return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return new DyInteger(left.GetInteger() % right.GetInteger());
            else if (right.TypeId == DyType.Float)
                return new DyFloat(left.GetFloat() % right.GetFloat());
            else
                return base.RemOp(left, right, ctx);
        }

        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.ShiftLeftOp(left, right, ctx);
            else
                return new DyInteger(left.GetInteger() << (int)right.GetInteger());
        }

        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.ShiftRightOp(left, right, ctx);
            else
                return new DyInteger(left.GetInteger() >> (int)right.GetInteger());
        }

        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.AndOp(left, right, ctx);
            else
                return new DyInteger(left.GetInteger() & (int)right.GetInteger());
        }

        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.OrOp(left, right, ctx);
            else
                return new DyInteger((int)left.GetInteger() | (int)right.GetInteger());
        }

        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.XorOp(left, right, ctx);
            else
                return new DyInteger(left.GetInteger() ^ (int)right.GetInteger());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() == right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() != right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() > right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() < right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() >= right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.Integer)
                return left.GetInteger() <= right.GetInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == DyType.Float)
                return left.GetFloat() <= right.GetFloat() ? DyBool.True : DyBool.False;
            else
                return base.LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyInteger(-arg.GetInteger());

        protected override DyObject PlusOp(DyObject arg, ExecutionContext ctx) => arg;

        protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => new DyInteger(~arg.GetInteger());

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => 
            new DyString(arg.GetInteger().ToString(CI.NumberFormat));
        #endregion

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to)
        {
            if (to.TypeId != DyType.Integer)
                return ctx.InvalidType(DyTypeNames.Integer, to);

            var ifrom = self.GetInteger();
            var istart = ifrom;
            var ito = to.GetInteger();
            var step = ito > ifrom ? 1 : -1;
            return new DyIterator(new DyInteger.RangeEnumerator(ifrom, istart, ito, step));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "to")
                return DyForeignFunction.Member(name, Range, -1, new Par("value"));

            return null;
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "max")
                return DyForeignFunction.Static(name, c => new DyInteger(long.MaxValue));

            if (name == "min")
                return DyForeignFunction.Static(name, c => new DyInteger(long.MinValue));

            return null;
        }
    }
}
