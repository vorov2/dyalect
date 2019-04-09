namespace Dyalect.Runtime.Types
{
    public sealed class DyNil : DyObject
    {
        public static readonly DyNil Instance = new DyNil();

        private DyNil() : base(StandardType.Nil)
        {
            
        }

        public override object AsObject() => null;

        public override bool AsBool() => false;

        protected override bool TestEquality(DyObject obj) => ReferenceEquals(this, obj);
    }
}
