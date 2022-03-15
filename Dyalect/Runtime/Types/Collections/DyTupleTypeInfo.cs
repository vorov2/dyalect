using Dyalect.Debug;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyTupleTypeInfo : DyCollectionTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Add | SupportedOperations.Iter | SupportedOperations.Lit;

        public override string TypeName => DyTypeNames.Tuple;

        public override int ReflectedTypeId => DyType.Tuple;

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            new DyTuple(((DyCollection)left).Concat(ctx, right));

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => ((DyTuple)arg).ToString(false, ctx);

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ((DyTuple)arg).ToString(true, ctx);

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
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

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
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
                return ctx.InvalidType(DyType.Integer, index);

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
                return ctx.InvalidType(DyType.Integer, index);

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

        private DyObject ToArray(ExecutionContext ctx, DyObject self)
        {
            var tuple = (DyTuple)self;
            return new DyArray(tuple.ConvertToPlainValues());
        }

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject item)
        {
            if (item.TypeId != DyType.String)
                return ctx.InvalidType(DyType.String, item);

            var tuple = (DyTuple)self;

            return tuple.HasItem(item.GetString(), ctx) ? DyBool.True : DyBool.False;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Add => Func.Member(name, AddItem, -1, new Par("value")),
                Method.Remove => Func.Member(name, Remove, -1, new Par("value")),
                Method.RemoveAt => Func.Member(name, RemoveAt, -1, new Par("index")),
                Method.Insert => Func.Member(name, Insert, -1, new Par("index"), new Par("value")),
                Method.Keys => Func.Member(name, GetKeys),
                Method.First => Func.Member(name, GetFirst),
                Method.Second => Func.Member(name, GetSecond),
                Method.Sort => Func.Member(name, SortBy, -1, new Par("comparer", DyNil.Instance)),
                Method.ToDictionary => Func.Member(name, ToDictionary),
                Method.ToArray => Func.Member(name, ToArray),
                Method.Contains => Func.Member(name, Contains, -1, new Par("key")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd) =>
            new DyTuple(new[] { fst, snd });

        private DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd) =>
            new DyTuple(new[] { fst, snd, thd });

        private DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Sort => Func.Static(name, SortBy, -1, new Par("value"), new Par("comparer", DyNil.Instance)),
                Method.Pair => Func.Static(name, GetPair, -1, new Par("first"), new Par("second")),
                Method.Triple => Func.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third")),
                Method.Concat => Func.Static(name, Concat, 0, new Par("values", true)),
                Method.Tuple => Func.Static(name, MakeNew, 0, new Par("values")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
