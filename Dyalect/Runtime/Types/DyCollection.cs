using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyCollection : DyObject, IEnumerable<DyObject>
    {
        public int Version { get; protected set; }

        public virtual int Count { get; protected set; }

        public virtual DyObject this[int index] => GetValue(index);

        internal DyCollection(int typeId) : base(typeId) { }

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
            if (other is not DyCollection arr)
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

        internal DyObject[] Concat(ExecutionContext ctx, DyObject right)
        {
            var newArr = new List<DyObject>(GetValues());
            var coll = DyIterator.Run(ctx, right);

            if (ctx.HasErrors)
                return Statics.EmptyDyObjects;

            newArr.AddRange(coll);
            return newArr.ToArray();
        }

        internal static DyObject[] ConcatValues(ExecutionContext ctx, DyObject values)
        {
            if (values == null)
                return Statics.EmptyDyObjects;

            var arr = new List<DyObject>();
            var vals = ((DyTuple)values).Values;

            foreach (var v in vals)
            {
                arr.AddRange(DyIterator.Run(ctx, v));

                if (ctx.HasErrors)
                    break;
            }

            return arr.ToArray();
        }
    }

    internal abstract class DyCollectionTypeInfo : DyTypeInfo
    {
        protected DyCollectionTypeInfo(int typeId) : base(typeId) { }

        protected virtual DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var coll = (DyCollection)self;
            var arr = coll.GetValues();

            if (fromElem.TypeId != DyType.Integer)
                return ctx.InvalidType(fromElem);

            if (toElem.TypeId != DyType.Nil && toElem.TypeId != DyType.Integer)
                return ctx.InvalidType(toElem);

            var beg = (int)fromElem.GetInteger();
            var end = ReferenceEquals(toElem, DyNil.Instance) ? coll.Count - 1 : (int)toElem.GetInteger();

            if (beg == 0 && end == coll.Count - 1)
                return self;

            if (beg < 0)
                beg = coll.Count + beg;

            if (beg >= coll.Count)
                return ctx.IndexOutOfRange(beg);

            if (end < 0)
                end = coll.Count + end - 1;

            if (end >= coll.Count || end < 0)
                return ctx.IndexOutOfRange(end);

            var len = end - beg + 1;

            if (len < 0)
                return ctx.IndexOutOfRange(toElem);

            return new DyIterator(new DyCollectionEnumerable(arr, beg, len, coll));
        }

        protected DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyCollection)self;

            IEnumerable<DyObject> Iterate()
            {
                for (var i = 0; i < arr.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(Iterate());
        }

        protected override DyFunction InitializeInstanceMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "indices" => DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters),
                "slice" => DyForeignFunction.Member(name, GetSlice, -1, new Par("start", DyInteger.Zero), new Par("len", DyNil.Instance)),
                _ => base.InitializeInstanceMember(name, ctx)
            };
    }
}
