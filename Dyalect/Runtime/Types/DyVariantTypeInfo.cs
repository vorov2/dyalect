namespace Dyalect.Runtime.Types
{
    internal sealed class DyVariantTypeInfo : DyTypeInfo
    {
        public DyVariantTypeInfo(int typeCode, string typeName)
        {
            TypeCode = typeCode;
            TypeName = typeName;
        }

        public override string TypeName { get; }

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args)
        {
            //Runtime guarantees that there is at least one arguments and
            //this is a name handle (from IndexedStrings) of a constructor
            var tag = (int)args[0].AsInteger();

            if (args.Length > 1)
            {
                var size = (args.Length - 1) / 2;
                var keys = new string[size];
                var values = new DyObject[size];

                for (var i = 1; i < args.Length; i += 2)
                {
                    keys[(i - 1) / 2] = args[i].AsString();
                    values[(i - 1) / 2] = args[i + 1];
                }

                return new DyVariant(TypeCode, tag, keys, values);
            }

            return new DyVariant(TypeCode, tag, null, null);
        }

        protected override DyObject GetOp(DyObject obj, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return obj.GetItem((int)index.AsInteger()) ?? Err.IndexOutOfRange(TypeName, index).Set(ctx);
            else if (index.TypeId == StandardType.String)
                return obj.GetItem(index.AsString()) ?? Err.IndexOutOfRange(TypeName, index).Set(ctx);
            else
                return Err.IndexInvalidType(TypeName, ctx.Assembly.Types[index.TypeId].TypeName).Set(ctx);
        }

        protected override void SetOp(DyObject obj, DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer && !obj.SetItem((int)index.AsInteger(), value))
                ctx.Error = Err.IndexOutOfRange(TypeName, index);
            else if (index.TypeId == StandardType.String && !obj.SetItem(index.AsString(), value))
                ctx.Error = Err.IndexOutOfRange(TypeName, index);
            else
                ctx.Error = Err.IndexInvalidType(TypeName, ctx.Assembly.Types[index.TypeId].TypeName);
        }
    }
}
