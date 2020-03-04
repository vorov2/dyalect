using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyCollection, IEnumerable<DyObject>
    {
        private const int DEFAULT_SIZE = 4;

        internal DyObject[] Values;

        public DyObject this[int index]
        {
            get { return Values[index]; }
            set { Values[index] = value; }
        }

        internal DyArray(DyObject[] values) : base(DyType.Array)
        {
            this.Values = values;
            Count = values.Length;
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
            if (index >= 0 && index < Count)
            {
                Count--;

                if (index < Count)
                    Array.Copy(Values, index + 1, Values, index, Count - index);

                Values[Count] = null;
                Version++;
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
            Version++;
        }

        public int IndexOf(DyObject elem)
        {
            return Array.IndexOf(Values, elem);
        }

        public int LastIndexOf(DyObject elem)
        {
            return Array.LastIndexOf(Values, elem);
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

        internal override DyObject GetValue(int index) => Values[index];

        internal override DyObject[] GetValues() => Values;
    }

    internal sealed class DyArrayTypeInfo : DyCollectionTypeInfo
    {
        public DyArrayTypeInfo() : base(DyType.Array)
        {

        }

        public override string TypeName => DyTypeNames.Array;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

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

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject fun)
        {
            var arr = (DyArray)self;
            var comparer = new DySortComparer(fun as DyFunction, ctx);
            Array.Sort(arr.Values, 0, arr.Count, comparer);
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

        private DyObject Swap(ExecutionContext ctx, DyObject self, DyObject idx1, DyObject idx2)
        {
            var arr = (DyArray)self;
            var fst = arr.GetItem(idx1, ctx);
            if (ctx.HasErrors) return DyNil.Instance;
            var snd = arr.GetItem(idx2, ctx);
            if (ctx.HasErrors) return DyNil.Instance;

            arr.SetItem(idx1, snd, ctx);
            arr.SetItem(idx2, fst, ctx);
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

        private DyObject RemoveRange(ExecutionContext ctx, DyObject self, DyObject items)
        {
            var arr = (DyArray)self;
            var coll = DyIterator.Run(ctx, items);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var strict = coll.ToArray();

            foreach (var e in strict)
                arr.Remove(e);

            return DyNil.Instance;
        }

        private DyObject RemoveRangeAt(ExecutionContext ctx, DyObject self, DyObject start, DyObject len)
        {
            var arr = (DyArray)self;

            if (start.TypeId != DyType.Integer)
                return ctx.InvalidType(start);

            var sti = (int)start.GetInteger();

            if (sti < 0 || sti >= arr.Count)
                return ctx.IndexOutOfRange(sti);

            var le = 0;

            if (len == DyNil.Instance)
                le = arr.Count - sti;
            else if (len.TypeId != DyType.Integer)
                return ctx.InvalidType(len);
            else
                le = (int)len.GetInteger();

            if (sti + le > arr.Count)
                return ctx.IndexOutOfRange(le);

            arr.RemoveRange(sti, le);
            return DyNil.Instance;
        }

        private DyObject RemoveAll(ExecutionContext ctx, DyObject self, DyObject arg)
        {
            if (!(arg is DyFunction fun))
                return ctx.InvalidType(arg);

            var arr = (DyArray)self;
            var toDelete = new List<DyObject>();

            foreach (var o in arr)
            {
                var res = fun.Call1(o, ctx);
                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (res.GetBool())
                    toDelete.Add(o);
            }

            foreach (var o in toDelete)
                arr.Remove(o);

            return DyNil.Instance;
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
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
                case "removeRange":
                    return DyForeignFunction.Member(name, RemoveRange, -1, new Par("items"));
                case "removeRangeAt":
                    return DyForeignFunction.Member(name, RemoveRangeAt, -1, new Par("start"), new Par("len", null));
                case "removeAll":
                    return DyForeignFunction.Member(name, RemoveAll, -1, new Par("predicate"));
                case "clear":
                    return DyForeignFunction.Member(name, ClearItems, -1, Statics.EmptyParameters);
                case "indexOf":
                    return DyForeignFunction.Member(name, IndexOf, -1, new Par("item"));
                case "lastIndexOf":
                    return DyForeignFunction.Member(name, LastIndexOf, -1, new Par("item"));
                case "sort":
                    return DyForeignFunction.Member(name, SortBy, -1, new Par("comparator", DyNil.Instance));
                case "swap":
                    return DyForeignFunction.Member(name, Swap, -1, new Par("fst"), new Par("snd"));
                case "compact":
                    return DyForeignFunction.Member(name, Compact, -1, Statics.EmptyParameters);
                case "reverse":
                    return DyForeignFunction.Member(name, Reverse, -1, Statics.EmptyParameters);
                default:
                    return base.GetMember(name, ctx);
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

            if (name == "sort")
                return DyForeignFunction.Static(name, SortBy, -1, new Par("array"), new Par("comparator", DyNil.Instance));

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
