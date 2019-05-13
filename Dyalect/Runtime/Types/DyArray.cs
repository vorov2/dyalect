using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

                var res = ctx.Types[x.TypeId].Gt(x, y, ctx);
                return res == DyBool.True
                    ? 1
                    : ctx.Types[x.TypeId].Eq(x, y, ctx) == DyBool.True ? 0 : -1;
            }
        }

        private const int DEFAULT_SIZE = 4;
        private DyObject[] values;

        public int Count { get; private set; }

        public DyObject this[int index]
        {
            get { return values[index]; }
            set { values[index] = value; }
        }

        internal DyObject[] GetValues() => values;

        internal DyArray(DyObject[] values) : base(StandardType.Array)
        {
            this.values = values;
            Count = values.Length;
        }

        public void Add(DyObject val)
        {
            if (Count == values.Length)
            {
                var dest = new DyObject[values.Length == 0 ? DEFAULT_SIZE : values.Length * 2];
                Array.Copy(values, 0, dest, 0, Count);
                values = dest;
            }

            values[Count++] = val;
        }

        public void Insert(int index, DyObject item)
        {
            if (index > Count)
                throw new IndexOutOfRangeException();

            if (index == Count && values.Length > index)
            {
                values[index] = item;
                Count++;
                return;
            }

            Array.Copy(values, index, values, index + 1, Count - index);
            values[index] = item;
            Count++;
        }

        public bool RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                Count--;

                if (index < Count)
                    Array.Copy(values, index + 1, values, index, Count - index);

                values[Count] = null;
                return true;
            }

            return false;
        }

        public bool Remove(DyObject val)
        {
            var index = Array.IndexOf(values, val);
            return RemoveAt(index);
        }

        public void Clear()
        {
            Count = 0;
            values = new DyObject[DEFAULT_SIZE];
        }

        public int IndexOf(DyObject elem)
        {
            return Array.IndexOf(values, elem);
        }

        public int LastIndexOf(DyObject elem)
        {
            return Array.LastIndexOf(values, elem);
        }

        public override object ToObject()
        {
            var newArr = new object[Count];

            for (var i = 0; i < newArr.Length; i++)
                newArr[i] = values[i].ToObject();

            return newArr;
        }

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.GetInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        private DyObject GetItem(int index)
        {
            if (index < 0 || index >= Count)
                return null;
            return values[index];
        }

        private void SetItem(int index, DyObject obj, ExecutionContext ctx)
        {
            if (index < 0 || index >= Count)
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                values[index] = obj;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
            else
                SetItem((int)index.GetInteger(), value, ctx);
        }

        public IEnumerator<DyObject> GetEnumerator() => new DyArrayEnumerator(values, Count);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyArrayTypeInfo : DyTypeInfo
    {
        public DyArrayTypeInfo() : base(StandardType.Array, false)
        {

        }

        public override string TypeName => StandardType.ArrayName;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyArray)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
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
                return Err.IndexInvalidType(TypeName, index.TypeName(ctx)).Set(ctx);

            var i = (int)index.GetInteger();
            var value = args.TakeAt(1, DyNil.Instance);

            if (i < 0 || i >= arr.Count)
                return Err.IndexOutOfRange(TypeName, i).Set(ctx);

            arr.Insert(i, value);
            return DyNil.Instance;
        }

        private DyObject AddRange(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(null);

            if (val == null)
                return DyNil.Instance;

            var iter = DyIterator.GetIterator(val, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            while (true)
            {
                var res = iter.Call(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (!ReferenceEquals(res, DyNil.Terminator))
                    arr.Add(res);
                else
                    break;
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
                return Err.IndexInvalidType(TypeName, index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.GetInteger();
            var arr = (DyArray)self;

            if (idx < 0 || idx >= arr.Count)
                return Err.IndexOutOfRange(TypeName, idx).Set(ctx);

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
                return Err.IndexOutOfRange(TypeName, start).Set(ctx);

            if (end < 0 || end >= dyArr.Count)
                return Err.IndexOutOfRange(TypeName, end).Set(ctx);

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
                return Err.InvalidType(StandardType.FunctionName, 
                    args == null || args[0] == null ? StandardType.NilName : args[0].TypeName(ctx))
                    .Set(ctx);
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
                return DyForeignFunction.Member(name, LenAdapter, -1, Statics.EmptyParameters);

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

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "new")
                return DyForeignFunction.Static(name, New, 0, new Par("values", true));

            if (name == "empty")
                return DyForeignFunction.Static(name, Empty, -1, new Par("size"), new Par("default", DyNil.Instance));

            return null;
        }
    }
}
