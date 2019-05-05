namespace Dyalect.Runtime.Types
{
    public sealed class DyVariant : DyObject
    {
        public int Tag { get; }

        internal DyVariant(int typeCode, int tag, string[] keys, DyObject[] values) : base(typeCode)
        {
            Tag = tag;
            Keys = keys ?? new string[0];
            Values = values ?? Statics.EmptyDyObjects;
        }

        internal string[] Keys { get; }

        internal DyObject[] Values { get; }

        public override object ToObject() => this;

        //internal override DyObject GetItem(int index)
        //{
        //    if (index < 0 || index >= Values.Length)
        //        return null;
        //    return Values[index];
        //}

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
        public DyVariantTypeInfo(int typeCode, string typeName) : base(typeCode)
        {
            TypeName = typeName;
        }

        public override string TypeName { get; }

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
