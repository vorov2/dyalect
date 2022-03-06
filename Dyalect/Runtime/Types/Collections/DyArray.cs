using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyCollection
    {
        internal static readonly DyArrayTypeInfo Type = new();
        
        private const int DEFAULT_SIZE = 4;

        internal DyObject[] Values;

        public new DyObject this[int index]
        {
            get => Values[CorrectIndex(index)];
            set => Values[CorrectIndex(index)] = value;
        }

        public DyArray(DyObject[] values) : base(Type) => (Values, Count) = (values, values.Length);

        public void Compact()
        {
            if (Count == Values.Length)
                return;
            var arr = new DyObject[Count];
            Array.Copy(Values, arr, Count);
            Values = arr;
        }

        public void RemoveRange(int start, int count)
        {
            var lst = new List<DyObject>(Values);
            lst.RemoveRange(start, count);
            Values = lst.ToArray();
            Count = Values.Length;
            Version++;
        }

        public void Add(DyObject val)
        {
            if (Count == Values.Length)
            {
                var dest = new DyObject[Values.Length == 0 ? DEFAULT_SIZE : Values.Length * 2];
                Array.Copy(Values, 0, dest, 0, Count);
                Values = dest;
            }

            Values[Count++] = val;
            Version++;
        }

        public void Insert(int index, DyObject item)
        {
            index = CorrectIndex(index);

            if (index > Count)
                throw new IndexOutOfRangeException();

            if (index == Count && Values.Length > index)
            {
                Values[index] = item;
                Count++;
                Version++;
                return;
            }

            EnsureSize(Count + 1);
            Array.Copy(Values, index, Values, index + 1, Count - index);
            Values[index] = item;
            Count++;
            Version++;
        }

        private void EnsureSize(int size)
        {
            if (size > Values.Length)
            {
                var exp = Values.Length * 2;

                if (size > exp)
                    exp = size;

                var arr = new DyObject[exp];
                Array.Copy(Values, arr, Values.Length);
                Values = arr;
            }
        }

        public bool RemoveAt(int index)
        {
            index = CorrectIndex(index);

            if (index >= 0 && index < Count)
            {
                Count--;

                if (index < Count)
                    Array.Copy(Values, index + 1, Values, index, Count - index);

                Values[Count] = null!;
                Version++;
                return true;
            }

            return false;
        }

        public bool Remove(ExecutionContext ctx, DyObject val)
        {
            var index = IndexOf(ctx, val);

            if (index < 0)
                return false;

            return RemoveAt(index);
        }

        public void Clear()
        {
            Count = 0;
            Values = new DyObject[DEFAULT_SIZE];
            Version++;
        }

        internal int IndexOf(ExecutionContext ctx, DyObject elem)
        {
            for (var i = 0; i < Count; i++)
            {
                var e = Values[i];

                if (e.DecType.Eq(ctx, e, elem).GetBool())
                    return i;

                if (ctx.HasErrors)
                    return -1;
            }

            return -1;
        }
        
        public int LastIndexOf(ExecutionContext ctx, DyObject elem)
        {
            var index = -1;

            for (var i = 0; i < Values.Length; i++)
            {
                var e = Values[i];

                if (e.DecType.Eq(ctx, e, elem).GetBool())
                    index = i;

                if (ctx.HasErrors)
                    return -1;
            }

            return index;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeCode == DyTypeCode.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            else
                return ctx.InvalidType(index);
        }

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) => Values[index];

        protected override void CollectionSetItem(int index, DyObject obj, ExecutionContext ctx) =>
            Values[index] = obj;

        internal override DyObject GetValue(int index) => Values[CorrectIndex(index)];

        internal override DyObject[] GetValues() => Values;
    }
}
