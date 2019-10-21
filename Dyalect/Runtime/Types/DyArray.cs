using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyObject, IEnumerable<DyObject>
    {
        internal sealed class Enumerator : IEnumerator<DyObject>
        {
            private readonly DyObject[] arr;
            private readonly int count;
            private readonly DyArray obj;
            private readonly int version;
            private readonly int start;
            private int index = -1;

            public Enumerator(DyObject[] arr, int start, int count, DyArray obj)
            {
                this.arr = arr;
                this.start = start;
                this.count = count;
                this.obj = obj;
                this.version = obj.version;
            }

            public DyObject Current => arr[index + start];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (version != obj.version)
                    throw new DyIterator.IterationException();

                if (++index < count)
                    return true;

                return false;
            }

            public void Reset()
            {
                index = -1;
            }
        }

        internal sealed class Enumerable : IEnumerable<DyObject>
        {
            private readonly DyObject[] arr;
            private readonly int count;
            private readonly DyArray obj;
            private readonly int start;

            public Enumerable(DyObject[] arr, int start, int count, DyArray obj)
            {
                this.arr = arr;
                this.start = start;
                this.count = count;
                this.obj = obj;
            }

            public IEnumerator<DyObject> GetEnumerator() => new Enumerator(arr, start, count, obj);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal sealed class Comparer : IComparer<DyObject>
        {
            private readonly DyFunction fun;
            private readonly ExecutionContext ctx;

            public Comparer(DyFunction fun, ExecutionContext ctx)
            {
                this.fun = fun;
                this.ctx = ctx;
            }

            public int Compare(DyObject x, DyObject y)
            {
                if (fun != null)
                {
                    var ret = fun.Call2(x, y, ctx);
                    return ret.TypeId != DyType.Integer
                        ? (ret.TypeId == DyType.Float ? (int)ret.GetFloat() : 0)
                        : (int)ret.GetInteger();
                }

                var res = ctx.Types[x.TypeId].Gt(ctx, x, y);
                return res == DyBool.True
                    ? 1
                    : ctx.Types[x.TypeId].Eq(ctx, x, y) == DyBool.True ? 0 : -1;
            }
        }

        private const int DEFAULT_SIZE = 4;
        internal DyObject[] Values;
        private int version;

        public int Count { get; private set; }

        public DyObject this[int index]
        {
            get { return Values[index]; }
            set { Values[index] = value; }
        }

        internal DyObject[] GetValues() => Values;

        internal DyArray(DyObject[] values) : base(DyType.Array)
        {
            this.Values = values;
            Count = values.Length;
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
            version++;
        }

        public void Insert(int index, DyObject item)
        {
            if (index > Count)
                throw new IndexOutOfRangeException();

            if (index == Count && Values.Length > index)
            {
                Values[index] = item;
                Count++;
                version++;
                return;
            }

            EnsureSize(Count + 1);
            Array.Copy(Values, index, Values, index + 1, Count - index);
            Values[index] = item;
            Count++;
            version++;
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
            if (index >= 0 && index < Count)
            {
                Count--;

                if (index < Count)
                    Array.Copy(Values, index + 1, Values, index, Count - index);

                Values[Count] = null;
                version++;
                return true;
            }

            return false;
        }

        public bool Remove(DyObject val)
        {
            var index = Array.IndexOf(Values, val);
            return RemoveAt(index);
        }

        public void Clear()
        {
            Count = 0;
            Values = new DyObject[DEFAULT_SIZE];
            version++;
        }

        public int IndexOf(DyObject elem)
        {
            return Array.IndexOf(Values, elem);
        }

        public int LastIndexOf(DyObject elem)
        {
            return Array.LastIndexOf(Values, elem);
        }

        public override object ToObject()
        {
            var newArr = new object[Count];

            for (var i = 0; i < newArr.Length; i++)
                newArr[i] = Values[i].ToObject();

            return newArr;
        }

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            else
                return ctx.IndexInvalidType(index);
        }

        internal protected override DyObject GetItem(int index, ExecutionContext ctx)
        {
            if (index < 0 || index >= Count)
                return ctx.IndexOutOfRange(index);
            return Values[index];
        }

        internal protected override void SetItem(int index, DyObject obj, ExecutionContext ctx)
        {
            if (index < 0 || index >= Count)
                ctx.IndexOutOfRange(index);
            else
                Values[index] = obj;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.Integer)
                ctx.IndexInvalidType(index);
            else
                SetItem((int)index.GetInteger(), value, ctx);
        }

        public IEnumerator<DyObject> GetEnumerator() => new Enumerator(Values, 0, Count, this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal override int GetCount() => Values.Length;
    }

    internal sealed class DyArrayTypeInfo : DyTypeInfo
    {
        public DyArrayTypeInfo() : base(DyType.Array)
        {

        }

        public override string TypeName => DyTypeNames.Array;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyArray)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var arr = (DyArray)arg;
            var sb = new StringBuilder();
            sb.Append('[');

            for (var i = 0; i < arr.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                var str = arr[i].ToString(ctx);

                if (ctx.Error != null)
                    return DyString.Empty;

                sb.Append(str.GetString());
            }

            sb.Append(']');
            return new DyString(sb.ToString());
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var newArr = new List<DyObject>(((DyArray)left).Values);
            var coll = DyIterator.Run(ctx, right);

            if (ctx.HasErrors)
                return DyNil.Instance;

            newArr.AddRange(coll);
            return new DyArray(newArr.ToArray());
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            ((DyArray)self).Add(args.TakeOne(DyNil.Instance));
            return DyNil.Instance;
        }

        private DyObject InsertItem(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            var arr = (DyArray)self;

            if (index.TypeId != DyType.Integer)
                return ctx.IndexInvalidType(index);

            var i = (int)index.GetInteger();

            if (i < 0 || i > arr.Count)
                return ctx.IndexOutOfRange(i);

            arr.Insert(i, value);
            return DyNil.Instance;
        }

        private DyObject AddRange(ExecutionContext ctx, DyObject self, DyObject val)
        {
            var arr = (DyArray)self;

            foreach (var o in DyIterator.Run(ctx, val))
            {
                if (ctx.HasErrors)
                    break;
                arr.Add(o);
            }

            return DyNil.Instance;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var val = args.TakeOne(DyNil.Instance);
            return ((DyArray)self).Remove(val) ? DyBool.True : DyBool.False;
        }

        private DyObject RemoveItemAt(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var index = args.TakeOne(DyNil.Instance);

            if (index.TypeId != DyType.Integer)
                return ctx.IndexInvalidType(index);

            var idx = (int)index.GetInteger();
            var arr = (DyArray)self;

            if (idx < 0 || idx >= arr.Count)
                return ctx.IndexOutOfRange(idx);

            arr.RemoveAt(idx);
            return DyNil.Instance;
        }

        private DyObject ClearItems(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            ((DyArray)self).Clear();
            return DyNil.Instance;
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(DyNil.Instance);
            var i = arr.IndexOf(val);
            return DyInteger.Get(i);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(DyNil.Instance);
            var i = arr.LastIndexOf(val);
            return DyInteger.Get(i);
        }

        private DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < arr.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(iterate());
        }

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject start, DyObject len)
        {
            var dyArr = (DyArray)self;
            var arr = dyArr.Values;

            if (start.TypeId != DyType.Integer)
                return ctx.InvalidType(start);

            if (len.TypeId != DyType.Nil && len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);

            var beg = (int)start.GetInteger();
            var end = ReferenceEquals(len, DyNil.Instance) ? dyArr.Count : beg + (int)len.GetInteger();

            if (beg == 0 && beg == end)
                return self;

            if (beg < 0 || beg >= dyArr.Count)
                return ctx.IndexOutOfRange(beg);

            if (end < 0 || end > dyArr.Count)
                return ctx.IndexOutOfRange(end);

            return new DyIterator(new DyArray.Enumerable(arr, beg, end - beg, dyArr));
        }

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var argo = args.TakeOne(null);

            if (ReferenceEquals(argo, DyNil.Instance))
                return Sort(ctx, self, null);

            var fun = argo as DyFunction;

            if (fun == null)
                return ctx.InvalidType(args == null || args[0] == null ? DyNil.Instance : args[0]);

            Array.Sort(arr.GetValues(), 0, arr.Count, new DyArray.Comparer(fun, ctx));
            return DyNil.Instance;
        }

        private DyObject Sort(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            Array.Sort(arr.GetValues(), 0, arr.Count, new DyArray.Comparer(null, ctx));
            return DyNil.Instance;
        }

        private DyObject Reverse(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            Array.Reverse(arr.Values);
            return DyNil.Instance;
        }

        private DyObject Compact(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            if (arr.Count == 0)
                return DyNil.Instance;

            var idx = 0;

            while (idx < arr.Count)
            {
                var e = arr[idx];

                if (ReferenceEquals(e, DyNil.Instance))
                    arr.RemoveAt(idx);
                else
                    idx++;
            }

            return DyNil.Instance;
        }

        private DyObject InsertRange(ExecutionContext ctx, DyObject self, DyObject index, DyObject range)
        {
            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(index);

            var arr = (DyArray)self;
            var idx = (int)index.GetInteger();

            if (idx < 0 || idx > arr.Count)
                return ctx.IndexOutOfRange(index);

            var coll = DyIterator.Run(ctx, range);

            if (ctx.HasErrors)
                return DyNil.Instance;

            foreach (var e in coll)
                arr.Insert(idx++, e);

            return DyNil.Instance;
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Len:
                    return DyForeignFunction.Member(name, Length);
                case "add":
                    return DyForeignFunction.Member(name, AddItem, -1, new Par("item"));
                case "insert":
                    return DyForeignFunction.Member(name, InsertItem, -1, new Par("index"), new Par("item"));
                case "insertRange":
                    return DyForeignFunction.Member(name, InsertRange, -1, new Par("index"), new Par("items"));
                case "addRange":
                    return DyForeignFunction.Member(name, AddRange, -1, new Par("items"));
                case "remove":
                    return DyForeignFunction.Member(name, RemoveItem, -1, new Par("item"));
                case "removeAt":
                    return DyForeignFunction.Member(name, RemoveItemAt, -1, new Par("index"));
                case "clear":
                    return DyForeignFunction.Member(name, ClearItems, -1, Statics.EmptyParameters);
                case "indexOf":
                    return DyForeignFunction.Member(name, IndexOf, -1, new Par("item"));
                case "lastIndexOf":
                    return DyForeignFunction.Member(name, LastIndexOf, -1, new Par("item"));
                case "indices":
                    return DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters);
                case "slice":
                    return DyForeignFunction.Member(name, GetSlice, -1, new Par("start"), new Par("len", DyNil.Instance));
                case "sort":
                    return DyForeignFunction.Member(name, SortBy, -1, new Par("comparator", DyNil.Instance));
                case "compact":
                    return DyForeignFunction.Member(name, Compact, -1, Statics.EmptyParameters);
                case "reverse":
                    return DyForeignFunction.Member(name, Reverse, -1, Statics.EmptyParameters);
                default:
                    return null;
            }
        }

        private DyObject New(ExecutionContext ctx, DyObject tuple)
        {
            return new DyArray(((DyTuple)tuple).Values);
        }

        private DyObject Empty(ExecutionContext ctx, DyObject sizeObj, DyObject val)
        {
            var size = sizeObj.GetInteger();
            var arr = new DyObject[size];

            if (val is DyFunction func)
            {
                for (var i = 0; i < size; i++)
                {
                    var res = func.Call0(ctx);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    arr[i] = res;
                }
            }
            else
            {
                for (var i = 0; i < size; i++)
                    arr[i] = val;
            }

            return new DyArray(arr);
        }

        internal static DyObject Concat(ExecutionContext ctx, DyObject values)
        {
            if (values == null)
                return DyNil.Instance;

            var vals = ((DyTuple)values).Values;
            var arr = new List<DyObject>();

            foreach (var v in vals)
            {
                arr.AddRange(DyIterator.Run(ctx, v));

                if (ctx.HasErrors)
                    break;
            }

            return new DyArray(arr.ToArray());
        }

        private static DyObject Copy(ExecutionContext ctx, DyObject from, DyObject sourceIndex, DyObject to, DyObject destIndex, DyObject length)
        {
            if (sourceIndex.TypeId != DyType.Integer)
                return ctx.InvalidType(sourceIndex);

            var iSourceIndex = sourceIndex.GetInteger();

            if (destIndex.TypeId != DyType.Integer)
                return ctx.InvalidType(destIndex);

            var iDestIndex = destIndex.GetInteger();

            if (length.TypeId != DyType.Integer)
                return ctx.InvalidType(length);

            var iLen = length.GetInteger();

            if (from.TypeId != DyType.Array)
                return ctx.InvalidType(from);

            var sourceArr = (DyArray)from;

            if (to.TypeId != DyType.Array && to.TypeId != DyType.Nil)
                return ctx.InvalidType(to);

            var destArr = to == DyNil.Instance ? new DyArray(new DyObject[iDestIndex + iLen]) : (DyArray)to;

            if (iSourceIndex < 0 || iSourceIndex >= sourceArr.Count)
                return ctx.IndexOutOfRange(sourceIndex);

            if (iDestIndex < 0 || iDestIndex >= destArr.Count)
                return ctx.IndexOutOfRange(destIndex);

            if (iSourceIndex + iLen < 0 || iSourceIndex + iLen > sourceArr.Count)
                return ctx.IndexOutOfRange(iSourceIndex + iLen);

            if (iDestIndex + iLen < 0 || iDestIndex + iLen > destArr.Count)
                return ctx.IndexOutOfRange(iDestIndex + iLen);

            Array.Copy(sourceArr.Values, iSourceIndex, destArr.Values, iDestIndex, iLen);

            if (iDestIndex != 0 && to == DyNil.Instance)
                for (var i = 0; i < iDestIndex; i++)
                    destArr[i] = DyNil.Instance;

            return destArr;
        }

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Array")
                return DyForeignFunction.Static(name, New, 0, new Par("values", true));

            if (name == "empty")
                return DyForeignFunction.Static(name, Empty, -1, new Par("size"), new Par("default", DyNil.Instance));

            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));

            if (name == "copy")
                return DyForeignFunction.Static(name, Copy, -1, new Par("from"), new Par("fromIndex", DyInteger.Get(0)), new Par("to", DyNil.Instance), new Par("toIndex", DyInteger.Get(0)), new Par("count"));

            return null;
        }
    }
}
