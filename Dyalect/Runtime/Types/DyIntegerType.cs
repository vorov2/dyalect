using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIntegerType : DyType
    {
        public static readonly DyIntegerType Instance = new DyIntegerType();

        private DyIntegerType() : base(StandardType.Integer)
        {

        }

        public override string TypeName => StandardType.IntegerName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            new DyInteger(args.TakeOne(DyInteger.Zero).AsInteger());

        public override bool CanConvertFrom(Type type) =>
            type == CliType.SByte || type == CliType.Int16 || type == CliType.Int32 || type == CliType.Int64
            || type == CliType.Byte || type == CliType.UInt16 || type == CliType.UInt32 || type == CliType.UInt64;

        public override DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx)
        {
            if (type == CliType.Int64)
                return new DyInteger((long)obj);
            else if (type == CliType.Int32)
                return new DyInteger((int)obj);

            return new DyInteger(System.Convert.ToInt64(obj));
        }

        public override bool CanConvertTo(Type type) => CanConvertFrom(type);

        public override object ConvertTo(DyObject obj, Type type, ExecutionContext ctx)
        {
            if (type == CliType.Int64)
                return obj.AsInteger();
            else if (type == CliType.Int32)
                return (int)obj.AsInteger();

            return System.Convert.ChangeType(obj.AsInteger(), type);
        }

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return new DyInteger(left.AsInteger() + right.AsInteger());
            else if (right.TypeId == StandardType.Float)
                return new DyFloat(left.AsFloat() + right.AsFloat());
            else
                return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return new DyInteger(left.AsInteger() - right.AsInteger());
            else if (right.TypeId == StandardType.Float)
                return new DyFloat(left.AsFloat() - right.AsFloat());
            else
                return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return new DyInteger(left.AsInteger() * right.AsInteger());
            else if (right.TypeId == StandardType.Float)
                return new DyFloat(left.AsFloat() * right.AsFloat());
            else
                return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return new DyInteger(left.AsInteger() / right.AsInteger());
            else if (right.TypeId == StandardType.Float)
                return new DyFloat(left.AsFloat() / right.AsFloat());
            else
                return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return new DyInteger(left.AsInteger() % right.AsInteger());
            else if (right.TypeId == StandardType.Float)
                return new DyFloat(left.AsFloat() % right.AsFloat());
            else
                return base.RemOp(left, right, ctx);
        }

        protected override DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.ShiftLeftOp(left, right, ctx);
            else
                return new DyInteger(left.AsInteger() << (int)right.AsInteger());
        }

        protected override DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.ShiftRightOp(left, right, ctx);
            else
                return new DyInteger(left.AsInteger() >> (int)right.AsInteger());
        }

        protected override DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.AndOp(left, right, ctx);
            else
                return new DyInteger(left.AsInteger() & (int)right.AsInteger());
        }

        protected override DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.OrOp(left, right, ctx);
            else
                return new DyInteger((int)left.AsInteger() | (int)right.AsInteger());
        }

        protected override DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return base.XorOp(left, right, ctx);
            else
                return new DyInteger(left.AsInteger() ^ (int)right.AsInteger());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() == right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() == right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() != right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() != right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() > right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() > right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() < right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() < right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() >= right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() >= right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Integer)
                return left.AsInteger() <= right.AsInteger() ? DyBool.True : DyBool.False;
            else if (right.TypeId == StandardType.Integer)
                return left.AsFloat() <= right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyInteger(-arg.AsInteger());

        protected override DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) => new DyInteger(~arg.AsInteger());
        #endregion
    }
}
