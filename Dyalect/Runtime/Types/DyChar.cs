using Dyalect.Parser;

namespace Dyalect.Runtime.Types
{
    public sealed class DyChar : DyObject
    {
        internal readonly char Value;

        public DyChar(char value) : base(StandardType.Char)
        {
            Value = value;
        }

        public override object ToObject() => Value;

        protected internal override char GetChar() => Value;

        protected internal override string GetString() => Value.ToString();
    }

    internal sealed class DyCharTypeInfo : DyTypeInfo
    {
        public DyCharTypeInfo() : base(StandardType.Char, false)
        {

        }

        public override string TypeName => StandardType.LabelName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => StringUtil.Escape(arg.GetString(), "'");

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var str1 = left.TypeId == StandardType.Char || left.TypeId == StandardType.String ? left.GetString() : left.ToString(ctx).Value;
            var str2 = right.TypeId == StandardType.Char || right.TypeId == StandardType.String ? right.GetString() : right.ToString(ctx).Value;
            return new DyString(str1 + str2);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() == right.GetChar() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar() != right.GetChar() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetChar().CompareTo(right.GetChar()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return null;
        }
        #endregion
    }
}
