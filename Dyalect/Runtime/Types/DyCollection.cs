﻿using Dyalect.Debug;
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
            switch (name)
            {
                case "indices":
                    return DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters);
                case "slice":
                    return DyForeignFunction.Member(name, GetSlice, -1, new Par("start"), new Par("len", DyNil.Instance));
                default:
                    return null;
            }
        }
    }
}
