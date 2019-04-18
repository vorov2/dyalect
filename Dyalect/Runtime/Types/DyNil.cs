namespace Dyalect.Runtime.Types
{
    public sealed class DyNil : DyObject
    {
        public static readonly DyNil Instance = new DyNil();

        private DyNil() : base(StandardType.Nil)
        {
            
        }

        public override object ToObject() => null;

        protected internal override bool GetBool() => false;
    }

    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public static readonly DyNilTypeInfo Instance = new DyNilTypeInfo();

        private DyNilTypeInfo() : base(StandardType.Nil)
        {

        }

        public override string TypeName => StandardType.NilName;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");
    }
}
