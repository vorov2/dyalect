using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyTupleTypeInfo : DyCollectionTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Add | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Tuple;

        public override int ReflectedTypeCode => DyType.Tuple;

        internal protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            new DyTuple(((DyCollection)left).Concat(ctx, right));

        internal protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Count;
            return DyInteger.Get(len);
        }

        internal protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var tup = (DyTuple)arg;
            return tup.ToString(ctx);
        }

        internal protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return DyBool.False;

            var (t1, t2) = ((DyTuple)left, (DyTuple)right);

            if (t1.Count != t2.Count)
                return DyBool.False;

            for (var i = 0; i < t1.Count; i++)
            {
                if (ReferenceEquals(ctx.RuntimeContext.Types[t1.Values[i].TypeId].Eq(ctx, t1.Values[i], t2.Values[i]),
                    DyBool.False))
                    return DyBool.False;

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyBool.True;
        }

        internal protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        internal protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        internal static DyObject Concat(ExecutionContext ctx, DyObject values) =>
            new DyTuple(DyCollection.ConcatValues(ctx, values));

        private DyObject GetKeys(ExecutionContext ctx, DyObject self)
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
            var comparer = new SortComparer(fun as DyFunction, ctx);
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
                var e = t.Values[i].GetTaggedValue();

                if (ctx.RuntimeContext.Types[e.TypeId].Eq(ctx, e, item).GetBool(ctx))
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

        private static DyTuple RemoveAt(ExecutionContext ctx, DyTuple self, int index)
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
            if (index.TypeId != DyType.Integer)
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

        private DyObject ToDictionary(ExecutionContext ctx, DyObject self)
        {
            var tuple = (DyTuple)self;
            return new DyDictionary(tuple.ConvertToDictionary(ctx));
        }

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject item)
        {
            if (item.TypeId != DyType.String)
                return ctx.InvalidType(item);

            var tuple = (DyTuple)self;

            return tuple.HasItem(item.GetString(), ctx) ? DyBool.True : DyBool.False;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Add" => Func.Member(name, AddItem, -1, new Par("item")),
                "Remove" => Func.Member(name, Remove, -1, new Par("item")),
                "RemoveAt" => Func.Member(name, RemoveAt, -1, new Par("index")),
                "Insert" => Func.Member(name, Insert, -1, new Par("index"), new Par("item")),
                "Keys" => Func.Member(name, GetKeys),
                "First" => Func.Member(name, GetFirst),
                "Second" => Func.Member(name, GetSecond),
                "Sort" => Func.Member(name, SortBy, -1, new Par("comparator", DyNil.Instance)),
                "ToDictionary" => Func.Member(name, ToDictionary),
                "Contains" => Func.Member(name, Contains, -1, new Par("label")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd) =>
            new DyTuple(new DyObject[] { fst, snd });

        private DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd) =>
            new DyTuple(new DyObject[] { fst, snd, thd });

        private DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Sort" => Func.Static(name, SortBy, -1, new Par("tuple"), new Par("comparator", DyNil.Instance)),
                "Pair" => Func.Static(name, GetPair, -1, new Par("first"), new Par("second")),
                "Triple" => Func.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third")),
                "Concat" => Func.Static(name, Concat, 0, new Par("values", true)),
                "Tuple" => Func.Static(name, MakeNew, 0, new Par("values")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
