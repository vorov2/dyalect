using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime.Types;

internal sealed class DyTupleTypeInfo : DyCollectionTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
        | SupportedOperations.Add | SupportedOperations.Iter | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(DyType.Tuple);

    public override int ReflectedTypeId => DyType.Tuple;

    public DyTupleTypeInfo() => AddMixin(DyType.Collection, DyType.Comparable);

    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        new DyTuple(((DyCollection)left).Concat(ctx, right));

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var len = ((DyTuple)arg).Count;
        return DyInteger.Get(len);
    }

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => ((DyTuple)arg).ToString(false, ctx);

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ((DyTuple)arg).ToString(true, ctx);

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return DyBool.False;

        var (t1, t2) = ((DyTuple)left, (DyTuple)right);

        if (t1.Count != t2.Count)
            return DyBool.False;

        var t1v = t1.UnsafeAccessValues();
        var t2v = t2.UnsafeAccessValues();

        for (var i = 0; i < t1.Count; i++)
        {
            if (t1v[i].NotEquals(t2v[i], ctx))
                return DyBool.False;

            if (ctx.HasErrors)
                return DyNil.Instance;
        }

        return DyBool.True;
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) => Compare(true, left, right, ctx);

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) => Compare(false, left, right, ctx);

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx)
    {
        if (!field.IsString(ctx)) return Nil;
        return ((DyTuple)self).GetOrdinal(field.GetString()) is not -1 ? DyBool.True : DyBool.False;
    }

    private static DyObject Compare(bool gt, DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId != right.TypeId)
            return ctx.OperationNotSupported(gt ? Builtins.Gt : Builtins.Lt, left, right);

        var xs = (DyTuple)left;
        var ys = (DyTuple)right;
        var xsv = xs.UnsafeAccessValues();
        var ysv = ys.UnsafeAccessValues();
        var len = xs.Count > ys.Count ? ys.Count : xs.Count;

        for (var i = 0; i < len; i++)
        {
            var x = xsv[i];
            var y = ysv[i];
            var res = gt ? x.Greater(y, ctx) : x.Lesser(y, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (res)
                return DyBool.True;

            res = x.Equals(y, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (!res)
                return DyBool.False;
        }

        return DyBool.False;
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

    internal static DyObject GetFirst(ExecutionContext ctx, DyObject self) =>
        self.GetItem(DyInteger.Zero, ctx);

    internal static DyObject GetSecond(ExecutionContext ctx, DyObject self) =>
        self.GetItem(DyInteger.One, ctx);

    internal static DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var tup = (DyTuple)self;
        var comparer = new SortComparer(functor, ctx);
        var newArr = new DyObject[tup.Count];
        Array.Copy(tup.UnsafeAccessValues(), newArr, newArr.Length);
        Array.Sort(newArr, 0, newArr.Length, comparer);
        return new DyTuple(newArr);
    }

    internal static DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject item)
    {
        var t = (DyTuple)self;
        var arr = new DyObject[t.Count + 1];
        Array.Copy(t.UnsafeAccessValues(), arr, t.Count);
        arr[^1] = item;
        return new DyTuple(arr);
    }

    internal static DyObject Remove(ExecutionContext ctx, DyObject self, DyObject item)
    {
        var t = (DyTuple)self;
        var tv = t.UnsafeAccessValues();

        for (var i = 0; i < tv.Length; i++)
        {
            var e = tv[i].GetTaggedValue();

            if (e.Equals(item, ctx))
                return RemoveAt(ctx, t, i);
        }

        return self;
    }

    internal static DyObject RemoveAt(ExecutionContext ctx, DyObject self, DyObject index)
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

    internal static DyTuple RemoveAt(ExecutionContext _, DyTuple self, int index)
    {
        var arr = new DyObject[self.Count - 1];
        var c = 0;
        var sv = self.UnsafeAccessValues();

        for (var i = 0; i < self.Count; i++)
        {
            if (i != index)
                arr[c++] = sv[i];
        }

        return new DyTuple(arr);
    }

    internal static DyObject Insert(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
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
            Array.Copy(tuple.UnsafeAccessValues(), 0, arr, 1, tuple.Count);
        else if (idx == tuple.Count)
            Array.Copy(tuple.UnsafeAccessValues(), 0, arr, 0, tuple.Count);
        else
        {
            Array.Copy(tuple.UnsafeAccessValues(), 0, arr, 0, idx);
            Array.Copy(tuple.UnsafeAccessValues(), idx, arr, idx + 1, tuple.Count - idx);
        }

        return new DyTuple(arr);
    }

    internal static DyObject ToDictionary(ExecutionContext ctx, DyObject self)
    {
        var tuple = (DyTuple)self;
        return new DyDictionary(tuple.ConvertToDictionary(ctx));
    }

    internal static DyObject ToArray(ExecutionContext ctx, DyObject self)
    {
        var tuple = (DyTuple)self;
        return new DyArray(tuple.GetValues());
    }

    internal static DyObject Compact(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = (DyTuple)self;
        var xs = new List<DyObject>();

        foreach (var val in seq.GetValues())
        {
            if (functor.NotNil())
            {
                var res = functor.Invoke(ctx, val);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (ReferenceEquals(res, DyBool.False))
                    xs.Add(val);
            }
            else if (!ReferenceEquals(val, DyNil.Instance))
                xs.Add(val);
        }

        return new DyTuple(xs.ToArray());
    }

    internal static DyObject Alter(ExecutionContext ctx, DyObject self, DyObject newTuple)
    {
        if (newTuple is DyTuple tup)
        {
            var xs = new List<DyObject>(((DyTuple)self).UnsafeAccessValues());

            foreach (var o in tup.UnsafeAccessValues())
            {
                if (o is DyLabel lab)
                {
                    var exist = xs.FirstOrDefault(i => i.GetLabel() == lab.Label) as DyLabel;

                    if (exist is not null)
                    {
                        exist.Value = lab.Value;
                        continue;
                    }
                }

                xs.Add(o);
            }

            return new DyTuple(xs.ToArray());
        }

        return ctx.InvalidType(DyType.Tuple, newTuple);
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
            Method.Compact => Func.Member(name, Compact, -1, new Par("predicate", DyNil.Instance)),
            Method.Alter => Func.Member(name, Alter, 0, new Par("values", ParKind.VarArg)),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    internal static DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd) =>
        new DyTuple(new[] { fst, snd });

    internal static DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd) =>
        new DyTuple(new[] { fst, snd, thd });

    internal static DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Sort => Func.Static(name, SortBy, -1, new Par("value"), new Par("comparer", DyNil.Instance)),
            Method.Pair => Func.Static(name, GetPair, -1, new Par("first"), new Par("second")),
            Method.Triple => Func.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third")),
            Method.Concat => Func.Static(name, Concat, 0, new Par("values", ParKind.VarArg)),
            Method.Tuple => Func.Static(name, MakeNew, 0, new Par("values")),
            _ => base.InitializeStaticMember(name, ctx)
        };
}
