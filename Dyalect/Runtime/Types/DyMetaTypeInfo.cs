namespace Dyalect.Runtime.Types
{
    internal sealed class DyMetaTypeInfo : DyTypeInfo
    {
        public DyMetaTypeInfo() : base(DyType.TypeInfo) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Get;

        public override string TypeName => DyTypeNames.TypeInfo;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.String)
                return index.GetString() switch
                {
                    "id" => DyInteger.Get(((DyTypeInfo)self).TypeCode),
                    "name" => new DyString(((DyTypeInfo)self).TypeName),
                    _ => ctx.IndexOutOfRange()
                };

            return ctx.IndexOutOfRange();
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());
    }
}
