namespace Dyalect.Runtime.Types
{
    public sealed class DyBool : DyObject
    {
        public static readonly DyBool True = new DyBool(true);
        public static readonly DyBool False = new DyBool(false);

        private readonly bool value;

        private DyBool(bool value) : base(StandardType.Bool)
        {
            this.value = value;
        }

        protected internal override bool GetBool() => value;

        public override object ToObject() => value;

        public override string ToString() => value.ToString();
    }

    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        public static readonly DyBoolTypeInfo Instance = new DyBoolTypeInfo();

        private DyBoolTypeInfo() : base(StandardType.Bool, false)
        {

        }

        public override string TypeName => StandardType.BoolName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            ReferenceEquals(arg, DyBool.True) ? "true" : "false";
    }
}
