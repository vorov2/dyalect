using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection, IEnumerable<DyObject>
    {
        internal static readonly DyTuple Empty = new DyTuple(Array.Empty<DyObject>());

        internal readonly DyObject[] Values;

        public override int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(DyType.Tuple) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");


        public static DyTuple Create(params DyObject[] args) => new DyTuple(args);

        public Dictionary<DyObject, DyObject> ConvertToDictionary()
        {
            var dict = new Dictionary<DyObject, DyObject>();

            foreach (var obj in Values)
            {
                if (obj is not DyLabel lab || !dict.TryAdd(new DyString(lab.Label), lab.Value))
                    dict.Add(new DyString(DefaultKey()), obj);
            }

            return dict;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            
            if (index.TypeId == DyType.String || index.TypeId == DyType.Char)
            {
                var i = GetOrdinal(index.GetString());

                if (i == -1)
                    return ctx.IndexOutOfRange();

                return GetItem(i, ctx);
            }
            
            return ctx.InvalidType(index);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.String)
            {
                var i = GetOrdinal(index.GetString());

                if (i == -1)
                    ctx.IndexOutOfRange();

                CollectionSetItem(i, value, ctx);
            }
            else
                base.SetItem(index, value, ctx);
        }

        private int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;
            return -1;
        }

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
            Values[index].TypeId == DyType.Label ? Values[index].GetTaggedValue() : Values[index];

        internal string GetKey(int index) => Values[index].GetLabel()!;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (Values[index].TypeId == DyType.Label)
                ((DyLabel)Values[index]).Value = value;
            else
                Values[index] = value;
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            GetOrdinal(name) != -1;

        private static string DefaultKey() => Guid.NewGuid().ToString();

        public override IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return Values[i].TypeId == DyType.Label ? Values[i].GetTaggedValue() : Values[i];
        }

        internal override DyObject GetValue(int index) => Values[index];

        internal override DyObject[] GetValues() => Values;
    }

    internal sealed class DyTupleTypeInfo : DyCollectionTypeInfo
    {
        public DyTupleTypeInfo() : base(DyType.Tuple) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Add | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Tuple;

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            new DyTuple(((DyCollection) left).Concat(ctx, right));

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var tup = (DyTuple)arg;
            var sb = new StringBuilder();
            sb.Append('(');

            for (var i = 0; i < tup.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                var k = tup.GetKey(i);
                var val = tup.GetItem(i, ctx).ToString(ctx);

                if (ctx.Error != null)
                    return DyString.Empty;

                if (k != null)
                {
                    sb.Append(k);
                    sb.Append(": ");
                }

                sb.Append(val.GetString());
            }

            sb.Append(')');
            return new DyString(sb.ToString());
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return DyBool.False;

            var t1 = (DyTuple)left;
            var t2 = (DyTuple)right;

            if (t1.Count != t2.Count)
                return DyBool.False;

            for (var i = 0; i < t1.Count; i++)
            {
                if (ctx.RuntimeContext.Types[t1.Values[i].TypeId].Eq(ctx, t1.Values[i], t2.Values[i]) == DyBool.False)
                    return DyBool.False;

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyBool.True;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        internal static DyObject Concat(ExecutionContext ctx, DyObject values) =>
            new DyTuple(DyCollection.ConcatValues(ctx, values));

        private DyObject GetKeys(ExecutionContext _, DyObject self)
        {
            IEnumerable<DyObject> Iterate()
            {
                var tup = (DyTuple)self;
                for (var i = 0; i < tup.Count; i++)
                {
                    var k = tup.GetKey(i);
                    if (k is not null)
                        yield return new DyString(k);
                }
            }

            return DyIterator.Create(Iterate());
        }

        private DyObject GetFirst(ExecutionContext ctx, DyObject self) =>
            self.GetItem(DyInteger.Zero, ctx);

        private DyObject GetSecond(ExecutionContext ctx, DyObject self) =>
            self.GetItem(DyInteger.One, ctx);

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject fun)
        {
            var tup = (DyTuple)self;
            var comparer = new DySortComparer(fun as DyFunction, ctx);
            var newArr = new DyObject[tup.Count];
            Array.Copy(tup.Values, newArr, newArr.Length);
            Array.Sort(newArr, 0, newArr.Length, comparer);
            return new DyTuple(newArr);
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var t = (DyTuple)self;
            var arr = new DyObject[t.Count + 1];
            Array.Copy(t.Values, arr, t.Count);
            arr[^1] = item;
            return new DyTuple(arr);
        }

        private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var t = (DyTuple)self;

            for (var i = 0; i < t.Values.Length; i++)
            {
                var e = t.Values[i];

                if (ctx.RuntimeContext.Types[e.TypeId].Eq(ctx, e, item).GetBool())
                    return RemoveAt(ctx, t, i);
            }

            return self;
        }

        private DyObject RemoveAt(ExecutionContext ctx, DyObject self, DyObject index)
        {
            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(index);

            var t = (DyTuple)self;

            var idx = (int)index.GetInteger();
            idx = idx < 0 ? t.Count + idx : idx;

            if (idx < 0 || idx >= t.Count)
                return ctx.IndexOutOfRange();

            return RemoveAt(ctx, t, idx);
        }

        private static DyTuple RemoveAt(ExecutionContext _, DyTuple self, int index)
        {
            var arr = new DyObject[self.Count - 1];
            var c = 0;

            for (var i = 0; i < self.Values.Length; i++)
            {
                if (i != index)
                    arr[c++] = self.Values[i];
            }

            return new DyTuple(arr);
        }

        private DyObject Insert(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            if (index.TypeId is not DyType.Integer)
                return ctx.InvalidType(index);

            var tuple = (DyTuple)self;

            var idx = (int)index.GetInteger();
            idx = idx < 0 ? tuple.Count + idx : idx;

            if (idx < 0 || idx > tuple.Count)
                return ctx.IndexOutOfRange();

            var arr = new DyObject[tuple.Count + 1];
            arr[idx] = value;

            if (idx == 0)
                Array.Copy(tuple.Values, 0, arr, 1, tuple.Count);
            else if (idx == tuple.Count)
                Array.Copy(tuple.Values, 0, arr, 0, tuple.Count);
            else
            {
                Array.Copy(tuple.Values, 0, arr, 0, idx);
                Array.Copy(tuple.Values, idx, arr, idx + 1, tuple.Count - idx);
            }

            return new DyTuple(arr);
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "add" => DyForeignFunction.Member(name, AddItem, -1, new Par("item")),
                "remove" => DyForeignFunction.Member(name, Remove, -1, new Par("item")),
                "removeAt" => DyForeignFunction.Member(name, RemoveAt, -1, new Par("index")),
                "insert" => DyForeignFunction.Member(name, Insert, -1, new Par("index"), new Par("item")),
                "keys" => DyForeignFunction.Member(name, GetKeys),
                "fst" => DyForeignFunction.Member(name, GetFirst),
                "snd" => DyForeignFunction.Member(name, GetSecond),
                "sort" => DyForeignFunction.Member(name, SortBy, -1, new Par("comparator", DyNil.Instance)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd) =>
            new DyTuple(new DyObject[] { fst, snd });

        private DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd) =>
            new DyTuple(new DyObject[] { fst, snd, thd });

        private DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "sort" => DyForeignFunction.Static(name, SortBy, -1, new Par("tuple"), new Par("comparator", DyNil.Instance)),
                "pair" => DyForeignFunction.Static(name, GetPair, -1, new Par("first"), new Par("second")),
                "triple" => DyForeignFunction.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third")),
                "concat" => DyForeignFunction.Static(name, Concat, 0, new Par("values", true)),
                "Tuple" => DyForeignFunction.Static(name, MakeNew, 0, new Par("values")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
