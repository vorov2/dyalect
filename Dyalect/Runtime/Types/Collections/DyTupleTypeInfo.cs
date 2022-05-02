using Dyalect.Codegen;
using Dyalect.Compiler;
using Dyalect.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyTupleTypeInfo : DyCollectionTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
        | SupportedOperations.Add | SupportedOperations.Iter | SupportedOperations.In;

    public override string ReflectedTypeName => nameof(Dy.Tuple);

    public override int ReflectedTypeId => Dy.Tuple;

    public DyTupleTypeInfo() => AddMixin(Dy.Collection, Dy.Comparable);

    #region Operations
    //TODO: reconsider logic
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var arr = new List<DyObject>();
        arr.AddRange(DyIterator.ToEnumerable(ctx, left));
        if (ctx.HasErrors) return Nil;
        arr.AddRange(DyIterator.ToEnumerable(ctx, right));
        if (ctx.HasErrors) return Nil;
        return new DyTuple(arr.ToArray());
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var len = ((DyTuple)arg).Count;
        return DyInteger.Get(len);
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => MakeString(ctx, (DyTuple)arg, false);

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => MakeString(ctx, (DyTuple)arg, true);

    internal static DyObject MakeString(ExecutionContext ctx, DyTuple value, bool literal = false)
    {
        var sb = new StringBuilder();
        sb.Append('(');

        for (var i = 0; i < value.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
                sb.Append(' ');
            }

            var v = value.GetValue(i);
            var ki = value.GetKeyInfo(i);

            if (ki is not null)
            {
                if (ki.Mutable)
                    sb.Append("var ");

                if (ki.Label.Length > 0 && char.IsLower(ki.Label[0]) && ki.Label.All(char.IsLetter))
                    sb.Append(ki.Label);
                else
                    sb.Append(StringUtil.Escape(ki.Label));

                sb.Append(':');
                sb.Append(' ');
            }

            var str = literal ? v.ToLiteral(ctx) : v.ToString(ctx);

            if (ctx.HasErrors)
                return Nil;

            sb.Append(str);
        }

        sb.Append(')');
        return new DyString(sb.ToString());
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return False;

        var (t1, t2) = ((DyTuple)left, (DyTuple)right);

        if (t1.Count != t2.Count)
            return False;

        var t1v = t1.UnsafeAccessValues();
        var t2v = t2.UnsafeAccessValues();

        for (var i = 0; i < t1.Count; i++)
        {
            if (t1v[i].NotEquals(t2v[i], ctx))
                return False;

            if (ctx.HasErrors)
                return Nil;
        }

        return True;
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right) => Compare(true, left, right, ctx);

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right) => Compare(false, left, right, ctx);

    protected override DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return ctx.InvalidType(Dy.String, field);

        return ((DyTuple)self).GetOrdinal(field.ToString()) is not -1 ? True : False;
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
                return Nil;

            if (res)
                return True;

            res = x.Equals(y, ctx);

            if (ctx.HasErrors)
                return Nil;

            if (!res)
                return False;
        }

        return False;
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) => ((DyTuple)self).GetItem(index, ctx);

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyTuple)self).SetItem(index, value, ctx);
        return Nil;
    }
    #endregion

    [InstanceMethod(Method.Add)]
    internal static DyObject AddItem(DyTuple self, DyObject value)
    {
        var arr = new DyObject[self.Count + 1];
        Array.Copy(self.UnsafeAccessValues(), arr, self.Count);
        arr[^1] = value;
        return new DyTuple(arr);
    }

    [InstanceMethod]
    internal static DyObject Remove(ExecutionContext ctx, DyTuple self, DyObject value)
    {
        var tv = self.UnsafeAccessValues();

        for (var i = 0; i < tv.Length; i++)
        {
            var e = tv[i] is DyLabel la ? la.Value : tv[i];

            if (e.Equals(value, ctx))
                return RemoveAt(self, i);
        }

        return self;
    }

    [InstanceMethod]
    internal static DyObject RemoveAt(ExecutionContext ctx, DyTuple self, int index)
    {
        index = index < 0 ? self.Count + index : index;

        if (index < 0 || index >= self.Count)
            return ctx.IndexOutOfRange(index);

        return RemoveAt(self, index);
    }

    internal static DyTuple RemoveAt(DyTuple self, int index)
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

    [InstanceMethod]
    //TODO: candidate for removal
    internal static DyObject Concat(ExecutionContext ctx, params DyObject[] values) =>
        new DyTuple(DyCollection.ConcatValues(ctx, values));

    [InstanceMethod]
    internal static DyObject Insert(ExecutionContext ctx, DyTuple self, int index, DyObject value)
    {
        index = index < 0 ? self.Count + index : index;

        if (index < 0 || index > self.Count)
            return ctx.IndexOutOfRange(index);

        var arr = new DyObject[self.Count + 1];
        arr[index] = value;

        if (index == 0)
            Array.Copy(self.UnsafeAccessValues(), 0, arr, 1, self.Count);
        else if (index == self.Count)
            Array.Copy(self.UnsafeAccessValues(), 0, arr, 0, self.Count);
        else
        {
            Array.Copy(self.UnsafeAccessValues(), 0, arr, 0, index);
            Array.Copy(self.UnsafeAccessValues(), index, arr, index + 1, self.Count - index);
        }

        return new DyTuple(arr);
    }

    [InstanceMethod]
    internal static DyObject Keys(DyTuple self)
    {
        IEnumerable<DyObject> Iterate()
        {
            for (var i = 0; i < self.Count; i++)
            {
                var k = self.GetKey(i);
                if (k is not null)
                    yield return new DyString(k);
            }
        }

        return DyIterator.Create(Iterate());
    }

    [InstanceMethod]
    internal static DyObject First(ExecutionContext ctx, DyTuple self) =>
        self.GetItem(DyInteger.Zero, ctx);

    [InstanceMethod]
    internal static DyObject Second(ExecutionContext ctx, DyTuple self) =>
        self.GetItem(DyInteger.One, ctx);

    [InstanceMethod]
    
    //TODO: candidate for removal
    internal static DyObject Sort(ExecutionContext ctx, DyTuple self, DyFunction? comparer = null)
    {
        var sortComparer = new SortComparer(comparer, ctx);
        var newArr = new DyObject[self.Count];
        Array.Copy(self.UnsafeAccessValues(), newArr, newArr.Length);
        Array.Sort(newArr, 0, newArr.Length, sortComparer);
        return new DyTuple(newArr);
    }

    [InstanceMethod]
    internal static DyObject ToDictionary(ExecutionContext ctx, DyTuple self) =>
        new DyDictionary(self.ConvertToDictionary(ctx));

    [InstanceMethod]
    internal static DyObject ToArray(DyTuple self) => new DyArray(self.GetValues());

    [InstanceMethod]
    internal static DyObject Compact(ExecutionContext ctx, DyTuple self, DyObject? predicate = null)
    {
        var xs = new List<DyObject>();

        foreach (var val in self.GetValues())
        {
            if (predicate is not null)
            {
                var res = predicate.Invoke(ctx, val);

                if (ctx.HasErrors)
                    return Nil;

                if (res.IsFalse())
                    xs.Add(val);
            }
            else if (!val.Is(Dy.Nil))
                xs.Add(val);
        }

        return new DyTuple(xs.ToArray());
    }

    [InstanceMethod]
    internal static DyObject Alter(DyTuple self, [VarArg]DyTuple values)
    {
        var xs = new List<DyObject>(self.UnsafeAccessValues());

        foreach (var o in values.UnsafeAccessValues())
        {
            if (o is DyLabel lab)
            {
                var exist = xs.OfType<DyLabel>().FirstOrDefault(i => i.Label == lab.Label);

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

    [StaticMethod(Method.Sort)]
    internal static DyObject StaticSort(ExecutionContext ctx, DyTuple value, DyFunction? comparer = null) =>
        Sort(ctx, value, comparer);

    [StaticMethod]
    internal static DyObject Pair(DyObject first, DyObject second) =>
        new DyTuple(new[] { first, second });

    [StaticMethod]
    internal static DyObject Triple(DyObject first, DyObject second, DyObject third) =>
        new DyTuple(new[] { first, second, third });

    [StaticMethod(Method.Concat)]
    //TODO: candidate for removal
    internal static DyObject StaticConcat(ExecutionContext ctx, params DyObject[] values) =>
        new DyTuple(DyCollection.ConcatValues(ctx, values));

    [StaticMethod(Method.Tuple)]
    internal static DyObject MakeNew([VarArg]DyObject values) => values;
}
