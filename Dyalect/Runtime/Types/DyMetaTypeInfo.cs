namespace Dyalect.Runtime.Types
{
    internal sealed class DyMetaTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.TypeInfo;

        public override int ReflectedTypeId => DyType.TypeInfo;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.String)
                return index.GetString() switch
                {
                    "code" => DyInteger.Get((int)((DyTypeInfo)self).TypeId),
                    "name" => new DyString(((DyTypeInfo)self).TypeName),
                    _ => ctx.IndexOutOfRange()
                };

            return ctx.IndexOutOfRange();
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());
    }
}
