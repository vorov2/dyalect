using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private readonly long from;
            private readonly long start;
            private readonly long to;
            private readonly long step;
            private readonly bool inf;
            private bool fst;
            private long current;

            public RangeEnumerator(long from, long start, long? to, long step)
            {
                this.from = from;
                this.start = start;
                this.to = to ?? 0;
                this.inf = to == null;
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

                current += step;

                if (inf)
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

        public static readonly DyInteger Zero = new(0L);
        public static readonly DyInteger MinusOne = new(-1L);
        public static readonly DyInteger One = new(1L);
        public static readonly DyInteger Two = new(2L);
        public static readonly DyInteger Three = new(3L);
        public static readonly DyInteger Max = new(long.MaxValue);
        public static readonly DyInteger Min = new(long.MinValue);

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

        public override bool Equals(DyObject obj) =>
            obj is DyInteger i && value == i.value;

        public override object ToObject() => value;

        protected internal override bool GetBool() => value != 0;

        internal protected override double GetFloat() => value;

        internal protected override long GetInteger() => value;

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(value);
        }
    }


    internal sealed class DyIntegerTypeInfo : DyTypeInfo
    {
        public DyIntegerTypeInfo() : base(DyType.Integer)
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

        private DyObject Range(ExecutionContext ctx, DyObject self, DyObject to, DyObject step)
        {
            if (to.TypeId != DyType.Integer && to.TypeId != DyType.Nil)
                return ctx.InvalidType(to);

            var ifrom = self.GetInteger();
            var istart = ifrom;
            var istep = step.TypeId == DyType.Nil ? 1L : step.GetInteger();

            if (to == DyNil.Instance)
                return new DyIterator(new DyInteger.RangeEnumerator(ifrom, istart, null, istep));

            var ito = to.GetInteger();

            if (ito <= ifrom)
                istep = -Math.Abs(istep);

            if (istep == 0
                || (istep < 0 && ito > ifrom)
                || (istep > 0 && ito < ifrom))
                return ctx.InvalidRange();

            return new DyIterator(new DyInteger.RangeEnumerator(ifrom, istart, ito, istep));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "range")
                return DyForeignFunction.Member(name, Range, -1, new Par("to", DyNil.Instance), new Par("step", DyNil.Instance));

            return base.GetMember(name, ctx);
        }

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId == DyType.Integer)
                return obj;
            else if (obj.TypeId == DyType.Float)
                return DyInteger.Get((long)obj.GetFloat());
            else if (obj.TypeId == DyType.Char || obj.TypeId == DyType.String)
            {
                _ = long.TryParse(obj.GetString(), out var i);
                return DyInteger.Get(i);
            }

            return ctx.InvalidType(obj);
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => DyForeignFunction.Static(name, _ => DyInteger.Max),
                "min" => DyForeignFunction.Static(name, _ => DyInteger.Min),
                "default" => DyForeignFunction.Static(name, _ => DyInteger.Zero),
                "Integer" => DyForeignFunction.Static(name, Convert, -1, new Par("value")),
                _ => base.GetStaticMember(name, ctx)
            };
    }
}
