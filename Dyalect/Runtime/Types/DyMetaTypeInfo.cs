namespace Dyalect.Runtime.Types
{
    internal sealed class DyMetaTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.TypeInfo;

        public override int ReflectedTypeCode => DyType.TypeInfo;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeCode == DyType.String)
                return index.GetString() switch
                {
                    "code" => DyInteger.Get((int)((DyTypeInfo)self).TypeCode),
                    "name" => new DyString(((DyTypeInfo)self).TypeName),
                    _ => ctx.IndexOutOfRange()
                };

            return ctx.IndexOutOfRange();
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "code" => Func.Auto(name, (ctx, self) => DyInteger.Get((int)((DyTypeInfo)self).TypeCode)),
                "name" => Func.Auto(name, (ctx, self) => new DyString(((DyTypeInfo)self).TypeName)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };
    }
}
