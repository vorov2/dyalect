using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFloatType : DyType
    {
        public static readonly DyFloatType Instance = new DyFloatType();

        private DyFloatType() : base(StandardType.Float)
        {

        }

        public override string TypeName => StandardType.FloatName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            new DyFloat(args.TakeOne(DyFloat.Zero).AsFloat());

        public override bool CanConvertFrom(Type type) =>
            type == CliType.Single || type == CliType.Double || type == CliType.Decimal;

        public override DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx)
        {
            if (type == CliType.Double)
                return new DyFloat((double)obj);

            return new DyFloat(System.Convert.ToDouble(obj));
        }

        public override bool CanConvertTo(Type type) =>
            type == CliType.Single || type == CliType.Double || type == CliType.Decimal;

        public override object ConvertTo(DyObject obj, Type type, ExecutionContext ctx)
        {
            if (type == CliType.Double)
                return obj.AsFloat();

            return System.Convert.ChangeType(obj.AsFloat(), type);
        }

        #region Binary Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.AsFloat() + right.AsFloat());
            else
                return base.AddOp(left, right, ctx);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.AsFloat() - right.AsFloat());
            else
                return base.SubOp(left, right, ctx);
        }

        protected override DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.AsFloat() * right.AsFloat());
            else
                return base.MulOp(left, right, ctx);
        }

        protected override DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.AsFloat() / right.AsFloat());
            else
                return base.DivOp(left, right, ctx);
        }

        protected override DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return new DyFloat(left.AsFloat() % right.AsFloat());
            else
                return base.RemOp(left, right, ctx);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() == right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() != right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() > right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() < right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() >= right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.GteOp(left, right, ctx);
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.Float || right.TypeId == StandardType.Integer)
                return left.AsFloat() <= right.AsFloat() ? DyBool.True : DyBool.False;
            else
                return base.LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => new DyFloat(-arg.AsFloat());
        #endregion
    }
}
