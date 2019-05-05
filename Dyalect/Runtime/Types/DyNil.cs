namespace Dyalect.Runtime.Types
{
    public class DyNil : DyObject
    {
        private sealed class DyTerminator : DyNil { }

        public static readonly DyNil Instance = new DyNil();
        internal static readonly DyNil Terminator = new DyTerminator();

        private DyNil() : base(StandardType.Nil)
        {
            
        }

        public override object ToObject() => null;

        protected internal override bool GetBool() => false;

        public override string ToString() => "nil";
    }

    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public static readonly DyNilTypeInfo Instance = new DyNilTypeInfo();

        private DyNilTypeInfo() : base(StandardType.Nil, false)
        {

        }

        public override string TypeName => StandardType.NilName;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId ? DyBool.True : DyBool.False;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");
    }
}
