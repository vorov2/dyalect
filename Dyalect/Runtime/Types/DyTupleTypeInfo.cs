namespace Dyalect.Runtime.Types
{
    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public static readonly DyTupleTypeInfo Instance = new DyTupleTypeInfo();

        private DyTupleTypeInfo() : base(StandardType.Bool)
        {

        }

        public override string TypeName => StandardType.TupleName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) => new DyTuple(new string[args.Length], args);
    }
}
