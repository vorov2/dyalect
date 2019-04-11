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

        public override DyTypeInfo GetTypeInfo() => new DyVariantTypeInfo(base.TypeId, "");

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
}
