namespace Dyalect.Runtime.Types
{
    internal sealed class DyMetaTypeInfo : DyTypeInfo
    {
        public DyMetaTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.TypeInfo) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.TypeInfo;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.DecType.TypeCode == DyTypeCode.String)
                return index.GetString() switch
                {
                    "code" => ctx.RuntimeContext.Integer.Get((int)((DyTypeInfo)self).TypeCode),
                    "name" => new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, ((DyTypeInfo)self).TypeName),
                    _ => ctx.IndexOutOfRange()
                };

            return ctx.IndexOutOfRange();
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, ("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "code" => Func.Auto(ctx, name, (ctx, self) => ctx.RuntimeContext.Integer.Get((int)((DyTypeInfo)self).TypeCode)),
                "name" => Func.Auto(ctx, name, (ctx, self) => new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, ((DyTypeInfo)self).TypeName)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };
    }
}
