using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyStringType : DyType
    {
        public static readonly DyStringType Instance = new DyStringType();

        private DyStringType() : base(StandardType.Bool)
        {

        }

        public override string TypeName => StandardType.StringName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            new DyString(args.TakeOne(DyString.Empty).AsString());

        public override bool CanConvertFrom(Type type) => type == CliType.String || type == CliType.Char;

        public override DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx) => new DyString((obj ?? "").ToString());

        public override bool CanConvertTo(Type type) => type == CliType.String;

        public override object ConvertTo(DyObject obj, Type type, ExecutionContext ctx) => obj.AsString();

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) => new DyString(left.AsString() + right.AsString());

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.AsString() == right.AsString() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.AsString() != right.AsString() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.AsString().CompareTo(right.AsString()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.AsString().CompareTo(right.AsString()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }
        #endregion
    }
}
