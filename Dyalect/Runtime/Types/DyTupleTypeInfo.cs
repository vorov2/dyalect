namespace Dyalect.Runtime.Types
{
    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public static readonly DyTupleTypeInfo Instance = new DyTupleTypeInfo();

        private DyTupleTypeInfo() : base(StandardType.Tuple)
        {

        }

        public override string TypeName => StandardType.TupleName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) => new DyTuple(new string[args.Length], args);

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Keys.Length;
            return len == 1 ? DyInteger.One
                : len == 2 ? DyInteger.Two
                : len == 3 ? DyInteger.Three
                : new DyInteger(len);
        }
    }
}
