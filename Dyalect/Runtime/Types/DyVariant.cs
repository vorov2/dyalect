namespace Dyalect.Runtime.Types
{
    public sealed class DyVariant : DyObject
    {
        public int Tag { get; }

        internal DyVariant(int typeCode, int tag, string[] keys, DyObject[] values) : base(typeCode)
        {
            Tag = tag;
            Keys = keys ?? new string[0];
            Values = values ?? new DyObject[0];
        }

        internal string[] Keys { get; }

        internal DyObject[] Values { get; }

        public override object AsObject() => this;

        //internal override DyObject GetItem(int index)
        //{
        //    if (index < 0 || index >= Values.Length)
        //        return null;
        //    return Values[index];
        //}

        protected override bool TestEquality(DyObject obj)
        {
            if (obj.TypeId != TypeId)
                return false;

            var v = (DyVariant)obj;

            if (v.Tag != Tag)
                return false;

            for (var i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] != v.Keys[i] || !Values[i].Equals(v.Values[i]))
                    return false;
            }

            return true;
        }

        //internal override DyObject GetItem(string key)
        //{
        //    var i = GetOrdinal(key);
        //    return GetItem(i);
        //}

        //internal override bool SetItem(int index, DyObject value)
        //{
        //    if (index < 0 || index >= Values.Length)
        //        return false;
        //    Values[index] = value;
        //    return true;
        //}

        //internal override bool SetItem(string key, DyObject value)
        //{
        //    var i = GetOrdinal(key);
        //    return SetItem(i, value);
        //}

        //private int GetOrdinal(string key)
        //{
        //    for (var i = 0; i < Keys.Length; i++)
        //        if (key == Keys[i])
        //            return i;

        //    return -1;
        //}
    }

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
            var tag = (int)args[0].GetInteger();

            if (args.Length > 1)
            {
                var size = (args.Length - 1) / 2;
                var keys = new string[size];
                var values = new DyObject[size];

                for (var i = 1; i < args.Length; i += 2)
                {
                    keys[(i - 1) / 2] = args[i].ToString(ctx);
                    values[(i - 1) / 2] = args[i + 1];
                }

                return new DyVariant(TypeCode, tag, keys, values);
            }

            return new DyVariant(TypeCode, tag, null, null);
        }

        //protected override DyObject GetOp(DyObject obj, DyObject index, ExecutionContext ctx)
        //{
        //    if (index.TypeId == StandardType.Integer)
        //        return obj.GetItem((int)index.AsInteger()) ?? Err.IndexOutOfRange(TypeName, index).Set(ctx);
        //    else if (index.TypeId == StandardType.String)
        //        return obj.GetItem(index.AsString()) ?? Err.IndexOutOfRange(TypeName, index).Set(ctx);
        //    else
        //        return Err.IndexInvalidType(TypeName, ctx.Assembly.Types[index.TypeId].TypeName).Set(ctx);
        //}

        //protected override void SetOp(DyObject obj, DyObject index, DyObject value, ExecutionContext ctx)
        //{
        //    if (index.TypeId == StandardType.Integer && !obj.SetItem((int)index.AsInteger(), value))
        //        ctx.Error = Err.IndexOutOfRange(TypeName, index);
        //    else if (index.TypeId == StandardType.String && !obj.SetItem(index.AsString(), value))
        //        ctx.Error = Err.IndexOutOfRange(TypeName, index);
        //    else
        //        ctx.Error = Err.IndexInvalidType(TypeName, ctx.Assembly.Types[index.TypeId].TypeName);
        //}
    }
}
