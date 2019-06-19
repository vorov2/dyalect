using Dyalect.Debug;

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

            public override string ToString() => "true";
        }

        private sealed class FalseBool : DyBool
        {
            protected internal override bool GetBool() => false;

            public override object ToObject() => false;

            public override string ToString() => "false";
        }

        private DyBool() : base(DyType.Bool)
        {

        }

        public override abstract object ToObject();

        public override DyObject Clone() => this;

        public static implicit operator DyBool(bool v) => v ? True : False;
        public static implicit operator bool(DyBool v) => ReferenceEquals(v, True);
    }

    internal sealed class DyBoolTypeInfo : DyTypeInfo
    {
        public DyBoolTypeInfo() : base(DyType.Bool)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Bool;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.GetBool() == right.GetBool() ? DyBool.True : DyBool.False;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            (DyString)(ReferenceEquals(arg, DyBool.True) ? "true" : "false");

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Bool")
                return DyForeignFunction.Static(name, (c, obj) => (DyBool)obj.GetBool(), -1, new Par("value"));

            if (name == "default")
                return DyForeignFunction.Auto(AutoKind.Generated, (c, _) => DyBool.False);

            return null;
        }
    }
}
