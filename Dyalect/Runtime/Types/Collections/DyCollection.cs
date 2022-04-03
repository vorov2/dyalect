using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyCollection : DyEnumerable
    {
        protected DyCollection(int typeCode) : base(typeCode) { }

        #region Indexing
        protected int CorrectIndex(int index) => index < 0 ? Count + index : index;

        internal DyObject GetItem(int index, ExecutionContext ctx)
        {
            index = CorrectIndex(index);
            
            if (index >= Count)
                return ctx.IndexOutOfRange(index);
            
            return CollectionGetItem(index, ctx);
        }

        protected abstract DyObject CollectionGetItem(int index, ExecutionContext ctx);

        protected internal override void SetItem(DyObject obj, DyObject value, ExecutionContext ctx)
        {
            if (obj.TypeId !=  DyType.Integer)
            {
                ctx.InvalidType(obj);
                return;
            }

            var index = CorrectIndex((int)obj.GetInteger());

            if (index >= Count)
                ctx.IndexOutOfRange(index);
            else
                CollectionSetItem(index, value, ctx);
        }

        protected abstract void CollectionSetItem(int index, DyObject value, ExecutionContext ctx);
        #endregion

        public override object ToObject() => ConvertToArray();

        public IList<object> ConvertToList() => new List<object>(ConvertToArray());

        public DyObject[] ConvertToArray()
        {
            var newArr = new DyObject[Count];

            for (var i = 0; i < newArr.Length; i++)
                newArr[i] = GetValue(i);

            return newArr;
        }

        public override IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(GetValues(), 0, Count, this);

        internal abstract DyObject GetValue(int index);

        internal abstract DyObject[] GetValues();

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                var c = Count;

                for (var i = 0; i < c; i++)
                {
                    var v = GetValue(i);
                    hash = hash * 31 + v.GetHashCode();
                }

                return hash;
            }
        }

        public override bool Equals(DyObject? other)
        {
            if (other is not DyCollection arr)
                return false;

            var c = Count;

            if (arr.Count != c)
                return false;

            for (var i = 0; i < c; i++)
                if (!GetValue(i).Equals(arr.GetValue(i)))
                    return false;

            return true;
        }

        internal DyObject[] Concat(ExecutionContext ctx, DyObject right)
        {
            var newArr = new List<DyObject>(DyIterator.ToEnumerable(ctx, this));

            if (ctx.HasErrors)
                return Array.Empty<DyObject>();

            var coll = DyIterator.ToEnumerable(ctx, right);

            if (ctx.HasErrors)
                return Array.Empty<DyObject>();

            newArr.AddRange(coll);
            return newArr.ToArray();
        }

        internal static DyObject[] ConcatValues(ExecutionContext ctx, DyObject values)
        {
            if (values is null)
                return Array.Empty<DyObject>();

            var arr = new List<DyObject>();

            foreach (var v in (DyTuple)values)
            {
                arr.AddRange(DyIterator.ToEnumerable(ctx, v));

                if (ctx.HasErrors)
                    break;
            }

            return arr.ToArray();
        }
    }
}
