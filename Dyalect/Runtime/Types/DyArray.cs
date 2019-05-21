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
        private sealed class DyArrayEnumerator : IEnumerator<DyObject>
        {
            private readonly DyObject[] arr;
            private readonly int count;
            private int index = -1;

            public DyArrayEnumerator(DyObject[] arr, int count)
            {
                this.arr = arr;
                this.count = count;
            }

            public DyObject Current => arr[index];

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (++index < count)
                    return true;
                
                return false;
            }

            public void Reset()
            {
                index = -1;
            }
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
                    return ret.TypeId != StandardType.Integer ? 0 : (int)ret.GetInteger();
                }

                var res = ctx.Types[x.TypeId].Gt(ctx, x, y);
                return res == DyBool.True
                    ? 1
                    : ctx.Types[x.TypeId].Eq(ctx, x, y) == DyBool.True ? 0 : -1;
            }
        }

        private const int DEFAULT_SIZE = 4;
        internal DyObject[] Values;

        public int Count { get; private set; }

        public DyObject this[int index]
        {
            get { return Values[index]; }
            set { Values[index] = value; }
        }

        internal DyObject[] GetValues() => Values;

        internal DyArray(DyObject[] values) : base(StandardType.Array)
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
        }

        public void Insert(int index, DyObject item)
        {
            if (index > Count)
                throw new IndexOutOfRangeException();

            if (index == Count && Values.Length > index)
            {
                Values[index] = item;
                Count++;
                return;
            }

            Array.Copy(Values, index, Values, index + 1, Count - index);
            Values[index] = item;
            Count++;
        }

        public bool RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Count--;

                if (index < Count)
                    Array.Copy(Values, index + 1, Values, index, Count - index);

                Values[Count] = null;
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
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            else
                return ctx.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx));
        }

        internal protected override DyObject GetItem(int index, ExecutionContext ctx)
        {
            if (index < 0 || index >= Count)
                return ctx.IndexOutOfRange(StandardType.ArrayName, index);
            return Values[index];
        }

        internal protected override void SetItem(int index, DyObject obj, ExecutionContext ctx)
        {
            if (index < 0 || index >= Count)
                ctx.IndexOutOfRange(this.TypeName(ctx), index);
            else
                Values[index] = obj;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                ctx.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx));
            else
                SetItem((int)index.GetInteger(), value, ctx);
        }

        public IEnumerator<DyObject> GetEnumerator() => new DyArrayEnumerator(Values, Count);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyArrayTypeInfo : DyTypeInfo
    {
        public DyArrayTypeInfo() : base(StandardType.Array, false)
        {

        }

        public override string TypeName => StandardType.ArrayName;

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

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            ((DyArray)self).Add(args.TakeOne(DyNil.Instance));
            return DyNil.Instance;
        }

        private DyObject InsertItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var index = args.TakeOne(DyNil.Instance);

            if (index.TypeId != StandardType.Integer)
                return ctx.IndexInvalidType(TypeName, index.TypeName(ctx));

            var i = (int)index.GetInteger();
            var value = args.TakeAt(1, DyNil.Instance);

            if (i < 0 || i >= arr.Count)
                return ctx.IndexOutOfRange(TypeName, i);

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

            if (index.TypeId != StandardType.Integer)
                return ctx.IndexInvalidType(TypeName, index.TypeName(ctx));

            var idx = (int)index.GetInteger();
            var arr = (DyArray)self;

            if (idx < 0 || idx >= arr.Count)
                return ctx.IndexOutOfRange(TypeName, idx);

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

            return new DyIterator(iterate().GetEnumerator());
        }

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var dyArr = (DyArray)self;
            var arr = dyArr.GetValues();

            var start = (int)args.TakeOne(DyInteger.Zero).GetInteger();
            var endo = args.TakeAt(1, null);
            var end = ReferenceEquals(endo, DyNil.Instance) ? dyArr.Count : (int)endo.GetInteger();

            if (start == 0 && start == end)
                return self;

            if (start < 0 || start >= dyArr.Count)
                return ctx.IndexOutOfRange(TypeName, start);

            if (end < 0 || end >= dyArr.Count)
                return ctx.IndexOutOfRange(TypeName, end);

            var newArr = new DyObject[end - start];
            Array.Copy(arr, start, newArr, 0, end - start);
            return new DyArray(newArr);
        }

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var argo = args.TakeOne(null);

            if (ReferenceEquals(argo, DyNil.Instance))
                return Sort(ctx, self, null);

            var fun = argo as DyFunction;

            if (fun == null)
            {
                return ctx.InvalidType(StandardType.FunctionName, 
                    args == null || args[0] == null ? StandardType.NilName : args[0].TypeName(ctx))
                    ;
            }

            Array.Sort(arr.GetValues(), 0, arr.Count, new DyArray.Comparer(fun, ctx));
            return DyNil.Instance;
        }

        private DyObject Sort(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            Array.Sort(arr.GetValues(), 0, arr.Count, new DyArray.Comparer(null, ctx));
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

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Member(name, Length);

            if (name == "add")
                return DyForeignFunction.Member(name, AddItem, -1, new Par("item"));

            if (name == "insert")
                return DyForeignFunction.Member(name, InsertItem, -1, new Par("index"), new Par("item"));

            if (name == "addRange")
                return DyForeignFunction.Member(name, AddRange, -1, new Par("seq"));

            if (name == "remove")
                return DyForeignFunction.Member(name, RemoveItem, -1, new Par("item"));

            if (name == "removeAt")
                return DyForeignFunction.Member(name, RemoveItemAt, -1, new Par("index"));

            if (name == "clear")
                return DyForeignFunction.Member(name, ClearItems, -1, Statics.EmptyParameters);

            if (name == "indexOf")
                return DyForeignFunction.Member(name, IndexOf, -1, new Par("item"));

            if (name == "lastIndexOf")
                return DyForeignFunction.Member(name, LastIndexOf, -1, new Par("item"));

            if (name == "indices")
                return DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters);

            if (name == "slice")
                return DyForeignFunction.Member(name, GetSlice, -1, new Par("start"), new Par("len", DyNil.Instance));

            if (name == "sort")
                return DyForeignFunction.Member(name, SortBy, -1,new Par("comparator", DyNil.Instance));

            if (name == "compact")
                return DyForeignFunction.Member(name, Compact, -1, Statics.EmptyParameters);

            return null;
        }

        private DyObject New(ExecutionContext ctx, DyObject tuple)
        {
            return new DyArray(((DyTuple)tuple).Values);
        }

        private DyObject Empty(ExecutionContext ctx, DyObject sizeObj, DyObject val)
        {
            var size = sizeObj.GetInteger();

            var arr = new DyObject[size];

            for (var i = 0; i < size; i++)
                arr[i] = val;

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

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "new")
                return DyForeignFunction.Static(name, New, 0, new Par("values", true));

            if (name == "empty")
                return DyForeignFunction.Static(name, Empty, -1, new Par("size"), new Par("default", DyNil.Instance));

            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));

            return null;
        }
    }
}
