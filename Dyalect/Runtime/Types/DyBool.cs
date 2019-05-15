namespace Dyalect.Runtime.Types
{
    public abstract class DyBool : DyObject
    {
        public static readonly DyBool True = new TrueBool();
        public static readonly DyBool False = new FalseBool();

        private sealed class TrueBool : DyBool
        {
            protected internal override bool GetBool() => true;

            public override object ToObject() => true;

            public override string ToString() => bool.TrueString;
        }

        private sealed class FalseBool : DyBool
        {
            protected internal override bool GetBool() => false;

            public override object ToObject() => false;

            public override string ToString() => bool.FalseString;
        }

        private DyBool() : base(StandardType.Bool)
        {

        }

        public override abstract object ToObject();
    }

    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        public DyBoolTypeInfo() : base(StandardType.Bool, false)
        {

        }

        public override string TypeName => StandardType.BoolName;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.GetBool() == right.GetBool() ? DyBool.True : DyBool.False;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            ReferenceEquals(arg, DyBool.True) ? "true" : "false";
    }
}
