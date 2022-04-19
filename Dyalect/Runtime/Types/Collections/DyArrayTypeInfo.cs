using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyArrayTypeInfo : DyCollectionTypeInfo
    {
        public override string TypeName => DyTypeNames.Array;

        public override int ReflectedTypeId => DyType.Array;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
            | SupportedOperations.Iter | SupportedOperations.Lit;

        public DyArrayTypeInfo() => AddMixin(DyType.Collection);

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyArray)arg).Count);

        private DyString ToStringOrLiteral(bool literal, DyObject arg, ExecutionContext ctx)
        {
            var arr = (DyArray)arg;
            var sb = new StringBuilder();
            sb.Append('[');

            for (var i = 0; i < arr.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                var str = literal ? arr[i].ToLiteral(ctx) :arr[i].ToString(ctx);

                if (ctx.Error != null)
                    return DyString.Empty;

                sb.Append(str.GetString());
            }

            sb.Append(']');
            return new DyString(sb.ToString());
        }

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => ToStringOrLiteral(false, arg, ctx);

        protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOrLiteral(true, arg, ctx);

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            new DyArray(((DyCollection)left).Concat(ctx, right));

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        protected override DyObject ContainsOp(DyObject self, DyObject item, ExecutionContext ctx)
        {
            var arr = (DyArray)self;
            return arr.IndexOf(ctx, item) != -1 ? DyBool.True : DyBool.False;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject arg)
        {
            ((DyArray)self).Add(arg);
            return DyNil.Instance;
        }

        private DyObject InsertItem(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            var arr = (DyArray)self;

            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, index);

            var i = (int)index.GetInteger();

            if (i < 0 || i > arr.Count)
                return ctx.IndexOutOfRange();

            arr.Insert(i, value);
            return DyNil.Instance;
        }

        private DyObject AddRange(ExecutionContext ctx, DyObject self, DyObject val)
        {
            var arr = (DyArray)self;

            foreach (var o in DyIterator.ToEnumerable(ctx, val))
            {
                if (ctx.HasErrors)
                    break;
                arr.Add(o);
            }

            return DyNil.Instance;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject val) =>
            ((DyArray)self).Remove(ctx, val) ? DyBool.True : DyBool.False;

        private DyObject RemoveItemAt(ExecutionContext ctx, DyObject self, DyObject index)
        {
            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, index);

            var idx = (int)index.GetInteger();
            var arr = (DyArray)self;

            if (idx < 0 || idx >= arr.Count)
                return ctx.IndexOutOfRange();

            arr.RemoveAt(idx);
            return DyNil.Instance;
        }

        private DyObject ClearItems(ExecutionContext ctx, DyObject self)
        {
            ((DyArray)self).Clear();
            return DyNil.Instance;
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject val)
        {
            var arr = (DyArray)self;
            var i = arr.IndexOf(ctx, val);
            return DyInteger.Get(i);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject val)
        {
            var arr = (DyArray)self;
            var i = arr.LastIndexOf(ctx, val);
            return DyInteger.Get(i);
        }

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject fun)
        {
            var arr = (DyArray)self;
            var comparer = new SortComparer(fun as DyFunction, ctx);
            arr.Compact();
            Array.Sort(arr.UnsafeAccessValues(), 0, arr.Count, comparer);
            return DyNil.Instance;
        }

        private DyObject Reverse(ExecutionContext ctx, DyObject self)
        {
            var arr = (DyArray)self;
            arr.Compact();
            Array.Reverse(arr.UnsafeAccessValues());
            return DyNil.Instance;
        }

        private DyObject Compact(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function && funObj.TypeId != DyType.Nil)
                return ctx.InvalidType(DyType.Function, DyType.Nil, funObj);

            var fun = funObj as DyFunction;
            var arr = (DyArray)self;

            if (arr.Count == 0)
                return DyNil.Instance;

            var idx = 0;

            while (idx < arr.Count)
            {
                var e = arr[idx];
                bool flag;

                if (fun is not null)
                {
                    var res = fun.Call(ctx, e);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    flag = ReferenceEquals(res, DyBool.True);
                }
                else
                    flag = ReferenceEquals(e, DyNil.Instance);

                if (flag)
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

            if (ctx.HasErrors)
                return DyNil.Instance;

            var snd = arr.GetItem(idx2, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            arr.SetItem(idx1, snd, ctx);
            arr.SetItem(idx2, fst, ctx);
            return DyNil.Instance;
        }

        private DyObject InsertRange(ExecutionContext ctx, DyObject self, DyObject index, DyObject range)
        {
            if (index.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, index);

            var arr = (DyArray)self;
            var idx = (int)index.GetInteger();

            if (idx < 0 || idx > arr.Count)
                return ctx.IndexOutOfRange();

            var coll = DyIterator.ToEnumerable(ctx, range);

            if (ctx.HasErrors)
                return DyNil.Instance;

            foreach (var e in coll)
                arr.Insert(idx++, e);

            return DyNil.Instance;
        }

        private DyObject RemoveRange(ExecutionContext ctx, DyObject self, DyObject items)
        {
            var arr = (DyArray)self;
            var coll = DyIterator.ToEnumerable(ctx, items);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var strict = coll.ToArray();

            foreach (var e in strict)
            {
                arr.Remove(ctx, e);

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyNil.Instance;
        }

        private DyObject RemoveRangeAt(ExecutionContext ctx, DyObject self, DyObject start, DyObject len)
        {
            var arr = (DyArray)self;

            if (start.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, start);

            var sti = (int)start.GetInteger();

            if (sti < 0 || sti >= arr.Count)
                return ctx.IndexOutOfRange();

            int le;

            if (ReferenceEquals(len, DyNil.Instance))
                le = arr.Count - sti;
            else if (len.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, len);
            else
                le = (int)len.GetInteger();

            if (sti + le > arr.Count)
                return ctx.IndexOutOfRange();

            arr.RemoveRange(sti, le);
            return DyNil.Instance;
        }

        private DyObject RemoveAll(ExecutionContext ctx, DyObject self, DyObject arg)
        {
            if (arg is not DyFunction fun)
                return ctx.InvalidType(DyType.Function, arg);

            var arr = (DyArray)self;
            var toDelete = new List<DyObject>();

            foreach (var o in arr)
            {
                var res = fun.Call(ctx, o);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (!res.IsFalse())
                    toDelete.Add(o);
            }

            foreach (var o in toDelete)
            {
                arr.Remove(ctx, o);

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyNil.Instance;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Add => Func.Member(name, AddItem, -1, new Par("value")),
                Method.Insert => Func.Member(name, InsertItem, -1, new Par("index"), new Par("value")),
                Method.InsertRange => Func.Member(name, InsertRange, -1, new Par("index"), new Par("values")),
                Method.AddRange => Func.Member(name, AddRange, -1, new Par("values")),
                Method.Remove => Func.Member(name, RemoveItem, -1, new Par("value")),
                Method.RemoveAt => Func.Member(name, RemoveItemAt, -1, new Par("index")),
                Method.RemoveRange => Func.Member(name, RemoveRange, -1, new Par("values")),
                Method.RemoveRangeAt => Func.Member(name, RemoveRangeAt, -1, new Par("index"), new Par("count", DyNil.Instance)),
                Method.RemoveAll => Func.Member(name, RemoveAll, -1, new Par("predicate")),
                Method.Clear => Func.Member(name, ClearItems),
                Method.IndexOf => Func.Member(name, IndexOf, -1, new Par("value")),
                Method.LastIndexOf => Func.Member(name, LastIndexOf, -1, new Par("value")),
                Method.Sort => Func.Member(name, SortBy, -1, new Par("comparer", DyNil.Instance)),
                Method.Swap => Func.Member(name, Swap, -1, new Par("value"), new Par("other")),
                Method.Compact => Func.Member(name, Compact, -1, new Par("predicate", DyNil.Instance)),
                Method.Reverse => Func.Member(name, Reverse),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };

        private DyObject New(ExecutionContext ctx, DyObject tuple) => new DyArray(((DyTuple)tuple).GetValues());

        private DyObject Empty(ExecutionContext ctx, DyObject sizeObj, DyObject val)
        {
            var size = sizeObj.GetInteger();
            var arr = new DyObject[size];

            if (val.TypeId == DyType.Iterator)
                val = ((DyIterator)val).GetIteratorFunction();

            if (val is DyFunction func)
            {
                for (var i = 0; i < size; i++)
                {
                    var res = func.Call(ctx);

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

        private static DyObject Concat(ExecutionContext ctx, DyObject values) =>
            new DyArray(DyCollection.ConcatValues(ctx, values));

        private static DyObject Copy(ExecutionContext ctx, DyObject from, DyObject sourceIndex, DyObject to, DyObject destIndex, DyObject length)
        {
            if (sourceIndex.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, sourceIndex);

            var iSourceIndex = sourceIndex.GetInteger();

            if (destIndex.TypeId != DyType.Integer)
                return ctx.InvalidType(DyType.Integer, destIndex);

            var iDestIndex = destIndex.GetInteger();

            if (from.TypeId != DyType.Array)
                return ctx.InvalidType(DyType.Array, from);

            var sourceArr = (DyArray)from;

            if (length.TypeId != DyType.Integer && length.TypeId != DyType.Nil)
                return ctx.InvalidType(DyType.Integer, DyType.Nil, length);

            var iLen = length.TypeId == DyType.Nil ? sourceArr.Count : length.GetInteger();

            if (to.TypeId != DyType.Array && to.TypeId != DyType.Nil)
                return ctx.InvalidType(DyType.Array, DyType.Nil, to);

            var destArr = to == DyNil.Instance ? new DyArray(new DyObject[iDestIndex + iLen]) : (DyArray)to;

            if (iSourceIndex < 0 || iSourceIndex >= sourceArr.Count)
                return ctx.IndexOutOfRange();

            if (iDestIndex < 0 || iDestIndex >= destArr.Count)
                return ctx.IndexOutOfRange();

            if (iSourceIndex + iLen < 0 || iSourceIndex + iLen > sourceArr.Count)
                return ctx.IndexOutOfRange();

            if (iDestIndex + iLen < 0 || iDestIndex + iLen > destArr.Count)
                return ctx.IndexOutOfRange();

            Array.Copy(sourceArr.UnsafeAccessValues(), iSourceIndex, destArr.UnsafeAccessValues(), iDestIndex, iLen);

            if (iDestIndex != 0 && to == DyNil.Instance)
                for (var i = 0; i < iDestIndex; i++)
                    destArr[i] = DyNil.Instance;

            return destArr;
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Method.Array => Func.Static(name, New, 0, new Par("values", true)),
                Method.Sort => Func.Static(name, SortBy, -1, new Par("values"), new Par("comparer", DyNil.Instance)),
                Method.Empty => Func.Static(name, Empty, -1, new Par("count"), new Par("default", DyNil.Instance)),
                Method.Concat => Func.Static(name, Concat, 0, new Par("values", true)),
                Method.Copy => Func.Static(name, Copy, -1, new Par("source"),
                    new Par("index", DyInteger.Zero), new Par("destination", DyNil.Instance),
                    new Par("destinationIndex", DyInteger.Zero), new Par("count", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Tuple => new DyTuple(((DyArray)self).ToArray()),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
