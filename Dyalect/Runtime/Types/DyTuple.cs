using System;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyObject
    {
        internal readonly DyObject[] Values;
        internal readonly string[] Keys;

        internal DyTuple(string[] keys, DyObject[] values) : base(StandardType.Tuple)
        {
            Keys = keys;
            Values = values;
        }

        public override object AsObject() => Values;

        protected override bool TestEquality(DyObject obj)
        {
            var t = (DyTuple)obj;

            if (Keys.Length != t.Keys.Length)
                return false;

            for (var i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] != t.Keys[i]
                    || Values[i].Equals(t.Values[i]))
                    return false;
            }

            return true;
        }

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.AsInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else if (index.TypeId == StandardType.String)
                return GetItem(index.AsString()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        private int GetOrdinal(string name) => Array.IndexOf(Keys, name);

        private DyObject GetItem(int index)
        {
            if (index < 0 || index >= Values.Length)
                return null;
            return Values[index];
        }

        private DyObject GetItem(string index)
        {
            return GetItem(GetOrdinal(index));
        }
    }
}
