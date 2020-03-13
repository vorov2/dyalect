using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection, IEnumerable<DyObject>
    {
        internal readonly DyObject[] Values;

        public override int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(DyType.Tuple)
        {
            this.Values = values ?? throw new DyException("Unable to create a tuple with no values.");
        }

        public IDictionary<DyObject, DyObject> ConvertToDictionary()
        {
            var dict = new Dictionary<DyObject, DyObject>();

            foreach (var obj in Values)
            {
                if (!(obj is DyLabel lab) || !dict.TryAdd(new DyString(lab.Label), lab.Value))
                    dict.Add(new DyString(DefaultKey()), obj);
            }

            return dict;
        }

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            else if (index.TypeId == DyType.String || index.TypeId == DyType.Char)
                return GetItem(index.GetString(), ctx) ?? ctx.IndexOutOfRange(index.GetString());
            else
                return ctx.IndexInvalidType(index);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                SetItem((int)index.GetInteger(), value, ctx);
            else if (index.TypeId == DyType.String)
                SetItem(index.GetString(), value, ctx);
            else
                ctx.IndexInvalidType(index);
        }

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            var i = GetOrdinal(name);

            if (i == -1)
                return ctx.IndexOutOfRange(name);

            return GetItem(i, ctx);
        }

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            var i = GetOrdinal(name);

            if (i == -1)
            {
                value = null;
                return false;
            }

            value = GetItem(i, ctx);
            return true;
        }

        protected internal override void SetItem(string name, DyObject value, ExecutionContext ctx)
        {
            var i = GetOrdinal(name);

            if (i == -1)
                ctx.IndexOutOfRange(name);

            SetItem(i, value, ctx);
        }

        private int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;
            return -1;
        }

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx)
        {
            return Values[index].TypeId == DyType.Label ? Values[index].GetTaggedValue() : Values[index];
        }

        internal string GetKey(int index) => Values[index].GetLabel();

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (Values[index].TypeId == DyType.Label)
                ((DyLabel)Values[index]).Value = value;
            else
                Values[index] = value;
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx)
        {
            return GetOrdinal(name) != -1;
        }

        private string DefaultKey() => Guid.NewGuid().ToString();

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
        public DyTupleTypeInfo() : base(DyType.Tuple)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

        public override string TypeName => DyTypeNames.Tuple;

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
                if (ctx.Types[t1.Values[i].TypeId].Eq(ctx, t1.Values[i], t2.Values[i]) == DyBool.False)
                    return DyBool.False;

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyBool.True;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject GetKeys(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var tup = (DyTuple)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < tup.Count; i++)
                {
                    var k = tup.GetKey(i);

                    if (k != null)
                        yield return new DyString(k);
                }
            }

            return new DyIterator(iterate());
        }

        private DyObject GetFirst(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return self.GetItem(0, ctx);
        }

        private DyObject GetSecond(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return self.GetItem(1, ctx);
        }

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject fun)
        {
            var tup = (DyTuple)self;
            var comparer = new DySortComparer(fun as DyFunction, ctx);
            var newArr = new DyObject[tup.Count];
            Array.Copy(tup.Values, newArr, newArr.Length);
            Array.Sort(newArr, 0, newArr.Length, comparer);
            return new DyTuple(newArr);
        }

        private DyObject Add(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var t = (DyTuple)self;
            var arr = new DyObject[t.Count + 1];
            Array.Copy(t.Values, arr, t.Count);
            arr[arr.Length - 1] = item;
            return new DyTuple(arr);
        }

        private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var t = (DyTuple)self;

            for (var i = 0; i < t.Values.Length; i++)
            {
                var e = t.Values[i];

                if (ctx.Types[e.TypeId].Eq(ctx, e, item).GetBool())
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
                return ctx.IndexOutOfRange(index);

            return RemoveAt(ctx, t, idx);
        }

        private DyTuple RemoveAt(ExecutionContext ctx, DyTuple self, int index)
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

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            return name switch
            {
                "add" => DyForeignFunction.Member(name, Add, -1, new Par("item")),
                "keys" => DyForeignFunction.Member(name, GetKeys, -1, Statics.EmptyParameters),
                "fst" => DyForeignFunction.Member(name, GetFirst, -1, Statics.EmptyParameters),
                "snd" => DyForeignFunction.Member(name, GetSecond, -1, Statics.EmptyParameters),
                "sort" => DyForeignFunction.Member(name, SortBy, -1, new Par("comparator", DyNil.Instance)),
                _ => base.GetMember(name, ctx),
            };
        }

        private DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            return new DyTuple(new DyObject[] { fst, snd });
        }

        private DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd)
        {
            return new DyTuple(new DyObject[] { fst, snd, thd });
        }

        private DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "sort")
                return DyForeignFunction.Static(name, SortBy, -1, new Par("tuple"), new Par("comparator", DyNil.Instance));

            if (name == "pair")
                return DyForeignFunction.Static(name, GetPair, -1, new Par("first"), new Par("second"));

            if (name == "triple")
                return DyForeignFunction.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third"));

            if (name == "Tuple")
                return DyForeignFunction.Static(name, MakeNew, 0, new Par("values"));

            return null;
        }
    }
}
