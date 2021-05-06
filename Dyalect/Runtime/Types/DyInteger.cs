using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public sealed class DyInteger : DyObject
    {
        public static readonly DyInteger Zero = new(0L);
        public static readonly DyInteger MinusOne = new(-1L);
        public static readonly DyInteger One = new(1L);
        public static readonly DyInteger Two = new(2L);
        public static readonly DyInteger Three = new(3L);
        public static readonly DyInteger Max = new(long.MaxValue);
        public static readonly DyInteger Min = new(long.MinValue);

        private readonly long value;

        public DyInteger(long value) : base(DyType.Integer) =>
            this.value = value;

        public static DyInteger Get(long i) =>
            i switch
            {
                -1 => MinusOne,
                0 => Zero,
                1 => One,
                2 => Two,
                3 => Three,
                _ => new DyInteger(i)
            };

        public override int GetHashCode() => value.GetHashCode();

        public override string ToString() => value.ToString(CI.Default);

        public override bool Equals(DyObject? obj) => obj is DyInteger i && value == i.value;

        public override object ToObject() => value;

        protected internal override bool GetBool() => value != 0;

        protected internal override double GetFloat() => value;

        protected internal override long GetInteger() => value;

        public override DyObject Clone() => this;

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(value);
        }
    }

    internal sealed class DyIntegerTypeInfo : DyTypeInfo
    {
        public DyIntegerTypeInfo() : base(DyType.Integer) { }

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
            if (right.TypeId is DyType.Integer)
                return new DyInteger(left.GetInteger() + right.GetInteger());
            
            if (right.TypeId is DyType.Float)
                return new DyFloat(left.GetFloat() + right.GetFloat());

            return base.AddOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return new DyInteger(left.GetInteger() - right.GetInteger());
            
            if (right.TypeId is DyType.Float)
                return new DyFloat(left.GetFloat() - right.GetFloat());
            
            return ctx.InvalidType(right);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return new DyInteger(left.GetInteger() * right.GetInteger());
            
            if (right.TypeId is DyType.Float)
                return new DyFloat(left.GetFloat() * right.GetFloat());
            
            return ctx.InvalidType(right);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
            {
                var i = right.GetInteger();

                if (i == 0)
                    return ctx.DivideByZero();

                return new DyInteger(left.GetInteger() / i);
            }
            
            if (right.TypeId is DyType.Float)
                return new DyFloat(left.GetFloat() / right.GetFloat());
            
            return ctx.InvalidType(right);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return new DyInteger(left.GetInteger() % right.GetInteger());
            
            if (right.TypeId is DyType.Float)
                return new DyFloat(left.GetFloat() % right.GetFloat());
            
            return ctx.InvalidType(right);
        }

        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() << (int)right.GetInteger());
        }

        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() >> (int)right.GetInteger());
        }

        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() & (int)right.GetInteger());
        }

        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(right);
            return new DyInteger((int)left.GetInteger() | (int)right.GetInteger());
        }

        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return ctx.InvalidType(right);
            return new DyInteger(left.GetInteger() ^ (int)right.GetInteger());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() == right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
                return left.GetFloat() == right.GetFloat() ? DyBool.True : DyBool.False;
            
            return base.EqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() != right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
                return left.GetFloat() != right.GetFloat() ? DyBool.True : DyBool.False;
            
            return base.NeqOp(left, right, ctx); //Important! Should redirect to base
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() > right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
                return left.GetFloat() > right.GetFloat() ? DyBool.True : DyBool.False;
            
            return ctx.InvalidType(right);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() < right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
                return left.GetFloat() < right.GetFloat() ? DyBool.True : DyBool.False;
            
            return ctx.InvalidType(right);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() >= right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
                return left.GetFloat() >= right.GetFloat() ? DyBool.True : DyBool.False;
            
            return ctx.InvalidType(right);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId is DyType.Integer)
                return left.GetInteger() <= right.GetInteger() ? DyBool.True : DyBool.False;
            
            if (right.TypeId is DyType.Float)
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

        private DyObject Convert(ExecutionContext ctx, DyObject obj)
        {
            if (obj.TypeId is DyType.Integer)
                return obj;
            
            if (obj.TypeId is DyType.Float)
                return DyInteger.Get((long)obj.GetFloat());
            
            if (obj.TypeId is DyType.Char or DyType.String)
            {
                _ = long.TryParse(obj.GetString(), out var i);
                return DyInteger.Get(i);
            }

            return ctx.InvalidType(obj);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "max" => DyForeignFunction.Static(name, _ => DyInteger.Max),
                "min" => DyForeignFunction.Static(name, _ => DyInteger.Min),
                "default" => DyForeignFunction.Static(name, _ => DyInteger.Zero),
                "Integer" => DyForeignFunction.Static(name, Convert, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
