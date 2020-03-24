using Dyalect.Debug;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyCollection : DyObject, IEnumerable<DyObject>
    {
        public int Version { get; protected set; }

        public virtual int Count { get; protected set; }

        internal DyCollection(int typeId) : base(typeId)
        {

        }

        #region Indexing
        protected int CorrectIndex(int index) => index < 0 ? Count + index : index;

        protected internal sealed override DyObject GetItem(int index, ExecutionContext ctx)
        {
            index = CorrectIndex(index);
            
            if (index < 0 || index >= Count)
                return ctx.IndexOutOfRange(index);
            
            return CollectionGetItem(index, ctx);
        }

        protected abstract DyObject CollectionGetItem(int index, ExecutionContext ctx);

        protected internal sealed override bool TryGetItem(int index, ExecutionContext ctx, out DyObject value)
        {
            index = CorrectIndex(index);

            if (index < 0 || index >= Count)
            {
                value = null;
                return false;
            }

            value = CollectionGetItem(index, ctx);
            return true;
        }

        protected internal sealed override void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            index = CorrectIndex(index);

            if (index < 0 || index >= Count)
                ctx.IndexOutOfRange(index);
            else
                CollectionSetItem(index, value, ctx);
        }

        protected abstract void CollectionSetItem(int index, DyObject value, ExecutionContext ctx);
        #endregion

        public override object ToObject() => ConvertToArray();

        public IList<object> ConvertToList() => new List<object>(ConvertToArray());

        public object[] ConvertToArray()
        {
            var values = GetValues();
            var newArr = new object[Count];

            for (var i = 0; i < newArr.Length; i++)
                newArr[i] = values[i].ToObject();

            return newArr;
        }

        public virtual IEnumerator<DyObject> GetEnumerator() => new DyCollectionEnumerator(GetValues(), 0, Count, this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal abstract DyObject GetValue(int index);

        internal abstract DyObject[] GetValues();

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                var c = GetCount();

                for (var i = 0; i < c; i++)
                {
                    var v = GetValue(i);
                    hash = hash * 31 + v.GetHashCode();
                }

                return hash;
            }
        }

        public override bool Equals(DyObject other)
        {
            if (!(other is DyCollection arr))
                return false;

            var c = GetCount();

            if (arr.GetCount() != c)
                return false;

            for (var i = 0; i < c; i++)
                if (!GetValue(i).Equals(arr.GetValue(i)))
                    return false;

            return true;
        }

        internal override int GetCount() => Count;
    }

    internal abstract class DyCollectionTypeInfo : DyTypeInfo
    {
        protected DyCollectionTypeInfo(int typeId) : base(typeId)
        {

        }

        protected DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject start, DyObject len)
        {
            var coll = (DyCollection)self;
            var arr = coll.GetValues();

            if (start.TypeId != DyType.Integer)
                return ctx.InvalidType(start);

            if (len.TypeId != DyType.Nil && len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);

            var beg = (int)start.GetInteger();
            var end = ReferenceEquals(len, DyNil.Instance) ? coll.Count : beg + (int)len.GetInteger();

            if (beg == 0 && beg == end)
                return self;

            if (beg < 0 || beg >= coll.Count)
                return ctx.IndexOutOfRange(beg);

            if (end < 0 || end > coll.Count)
                return ctx.IndexOutOfRange(end);

            return new DyIterator(new DyCollectionEnumerable(arr, beg, end - beg, coll));
        }

        protected DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyCollection)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < arr.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(iterate());
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return name switch
            {
                "indices" => DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters),
                "slice" => DyForeignFunction.Member(name, GetSlice, -1, new Par("start"), new Par("len", DyNil.Instance)),
                _ => base.GetMember(name, ctx)
            };
        }
    }
}
