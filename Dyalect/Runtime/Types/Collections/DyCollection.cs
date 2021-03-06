﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Runtime.Types
{
    public abstract class DyCollection : DyEnumerable
    {
        public virtual DyObject this[int index] => GetValue(index);

        protected DyCollection(int typeId) : base(typeId) { }

        #region Indexing
        protected int CorrectIndex(int index) => index < 0 ? Count + index : index;

        internal DyObject GetItem(int index, ExecutionContext ctx)
        {
            index = CorrectIndex(index);
            
            if (index >= Count)
                return ctx.IndexOutOfRange();
            
            return CollectionGetItem(index, ctx);
        }

        protected abstract DyObject CollectionGetItem(int index, ExecutionContext ctx);

        protected internal override void SetItem(DyObject obj, DyObject value, ExecutionContext ctx)
        {
            if (obj.TypeId is not DyType.Integer)
            {
                ctx.InvalidType(obj);
                return;
            }

            var index = CorrectIndex((int)obj.GetInteger());

            if (index >= Count)
                ctx.IndexOutOfRange();
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

        public override IEnumerator<DyObject> GetEnumerator() =>
            new DyCollectionEnumerator(GetValues(), 0, Count, this);

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
            var newArr = new List<DyObject>(GetValues());
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
            var vals = ((DyTuple)values).GetValues();

            foreach (var v in vals)
            {
                arr.AddRange(DyIterator.ToEnumerable(ctx, v));

                if (ctx.HasErrors)
                    break;
            }

            return arr.ToArray();
        }
    }
}
