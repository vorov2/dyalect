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

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);

        protected internal override bool GetBool() => value;

        public override object ToObject() => value;
    }

    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        public static readonly DyBoolTypeInfo Instance = new DyBoolTypeInfo();

        private DyBoolTypeInfo() : base(StandardType.Bool)
        {

        }

        public override string TypeName => StandardType.BoolName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            args.TakeOne(DyBool.False).GetBool() ? DyBool.True : DyBool.False;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            ReferenceEquals(arg, DyBool.True) ? "true" : "false";
    }
}
