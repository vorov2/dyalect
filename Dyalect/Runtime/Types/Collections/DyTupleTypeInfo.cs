using Dyalect.Codegen;
using Dyalect.Compiler;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyTupleTypeInfo : DyCollTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Tuple);

    public override int ReflectedTypeId => Dy.Tuple;

    public DyTupleTypeInfo()
    {
        AddMixins(Dy.Container, Dy.Order, Dy.Collection, Dy.Equatable, Dy.Sequence, Dy.Show);
        SetSupportedOperations(Ops.Add);
    }

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

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        IEnumerable<DyObject> Iterate()
        {
            var tuple = (DyTuple)arg;
            var xs = tuple.UnsafeAccess();
            for (var i = 0; i < tuple.Count; i++)
                yield return xs[i];
        }

        try
        {
            return new DyString("(" + Iterate().ToLiteral(ctx) + ")");
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return False;

        var (xs, ys) = ((DyTuple)left, (DyTuple)right);
        
        try
        {
            return DyTuple.Equals(ctx, xs, ys);
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.OperationNotSupported(Builtins.Gt, left, right);

        try
        {
            return DyTuple.Greater(ctx, (DyTuple)left, (DyTuple)right);
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId != right.TypeId)
            return ctx.OperationNotSupported(Builtins.Lt, left, right);

        try
        {
            return DyTuple.Lesser(ctx, (DyTuple)left, (DyTuple)right);
        }
        catch (DyCodeException e)
        {
            ctx.Error = e.Error;
            return Nil;
        }
    }

    protected override DyObject InOp(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return ctx.InvalidType(field);

        return ((DyTuple)self).GetOrdinal(field.ToString()) is not -1 ? True : False;
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) =>
        ((DyTuple)self).GetItem(ctx, index);

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        ((DyTuple)self).SetItem(ctx, index, value);
        return Nil;
    }

    internal override void SetInstanceMember(ExecutionContext ctx, HashString name, DyFunction func)
    {
        if ((string)name is Builtins.Get or Builtins.Set or Builtins.Length)
        {
            ctx.OverloadProhibited(this, (string)name);
            return;
        }

        base.SetInstanceMember(ctx, name, func);
    }
    #endregion

    [InstanceMethod]
    internal static bool ContainsField(DyTuple self, string field) =>
        self.GetOrdinal(field.ToString()) is not -1;

    [InstanceMethod(Method.Add)]
    internal static DyObject AddItem(DyTuple self, DyObject value)
    {
        var arr = new DyObject[self.Count + 1];
        Array.Copy(self.UnsafeAccess(), arr, self.Count);
        arr[^1] = value;
        return new DyTuple(arr);
    }

    [InstanceMethod]
    internal static DyObject Remove(ExecutionContext ctx, DyTuple self, DyObject value)
    {
        var tv = self.UnsafeAccess();

        for (var i = 0; i < tv.Length; i++)
        {
            var e = tv[i] is DyLabel la ? la.Value : tv[i];

            if (e.Equals(value, ctx))
                return InternalRemoveAt(self, i);
        }

        return self;
    }

    [InstanceMethod]
    internal static DyObject RemoveField(DyTuple self, string field)
    {
        var tv = self.UnsafeAccess();

        for (var i = 0; i < tv.Length; i++)
        {
            if (tv[i] is DyLabel la && la.Label == field)
                return InternalRemoveAt(self, i);
        }

        return self;
    }

    [InstanceMethod]
    internal static DyObject RemoveAt(DyTuple self, int index)
    {
        index = index < 0 ? self.Count + index : index;

        if (index < 0 || index >= self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        return InternalRemoveAt(self, index);
    }

    internal static DyTuple InternalRemoveAt(DyTuple self, int index)
    {
        var arr = new DyObject[self.Count - 1];
        var c = 0;
        var sv = self.UnsafeAccess();

        for (var i = 0; i < self.Count; i++)
        {
            if (i != index)
                arr[c++] = sv[i];
        }

        return new DyTuple(arr);
    }

    [InstanceMethod]
    internal static DyObject Insert(DyTuple self, int index, DyObject value)
    {
        index = index < 0 ? self.Count + index : index;

        if (index < 0 || index > self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        var arr = new DyObject[self.Count + 1];
        arr[index] = value;

        if (index == 0)
            Array.Copy(self.UnsafeAccess(), 0, arr, 1, self.Count);
        else if (index == self.Count)
            Array.Copy(self.UnsafeAccess(), 0, arr, 0, self.Count);
        else
        {
            Array.Copy(self.UnsafeAccess(), 0, arr, 0, index);
            Array.Copy(self.UnsafeAccess(), index, arr, index + 1, self.Count - index);
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
    internal static DyObject First(ExecutionContext ctx, DyTuple self)
    {
        var ret = self.GetItem(ctx, 0);
        ctx.ThrowIf();
        return ret;
    }

    [InstanceMethod]
    internal static DyObject Second(ExecutionContext ctx, DyTuple self)
    {
        var ret = self.GetItem(ctx, 1);
        ctx.ThrowIf();
        return ret;
    }

    [InstanceMethod]
    //TODO: candidate for removal
    internal static DyObject Sort(ExecutionContext ctx, DyTuple self, DyFunction? comparer = null)
    {
        var sortComparer = new SortComparer(comparer, ctx);
        var newArr = new DyObject[self.Count];
        Array.Copy(self.UnsafeAccess(), newArr, newArr.Length);
        Array.Sort(newArr, 0, newArr.Length, sortComparer);
        return new DyTuple(newArr);
    }

    [InstanceMethod]
    internal static DyObject ToDictionary(DyTuple self) =>
        new DyDictionary(self.ConvertToDictionary());

    [InstanceMethod(Method.ToArray)]
    internal static DyObject[] ToArray(DyCollection self) => self.ToArray();

    [InstanceMethod]
    internal static DyObject Compact(ExecutionContext ctx, DyTuple self, DyObject? predicate = null)
    {
        var xs = new List<DyObject>();

        foreach (var val in self.ToArray())
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
        var xs = new List<DyObject>(self.UnsafeAccess());

        foreach (var o in values.UnsafeAccess())
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
