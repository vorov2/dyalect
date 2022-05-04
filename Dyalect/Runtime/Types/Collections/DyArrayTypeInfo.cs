using Dyalect.Codegen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyArrayTypeInfo : DyCollectionTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Array);

    public override int ReflectedTypeId => Dy.Array;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.In;

    public DyArrayTypeInfo() => AddMixin(Dy.Collection);

    #region Operations
    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        DyInteger.Get(((DyArray)arg).Count);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => ToStringOrLiteral(false, arg, ctx);

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOrLiteral(true, arg, ctx);

    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var arr = new List<DyObject>();
        arr.AddRange(DyIterator.ToEnumerable(ctx, left));
        if (ctx.HasErrors) return Nil;
        arr.AddRange(DyIterator.ToEnumerable(ctx, right));
        if (ctx.HasErrors) return Nil;
        return new DyArray(arr.ToArray());
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (index is DyInteger i)
        {
            var arr = (DyArray)self;
            var ix = (int)i.Value;

            if (!CorrectIndex(arr, ref ix, insert: false))
                return ctx.IndexOutOfRange(index);

            return arr[ix];
        }

        return ctx.InvalidType(index);
    }

    protected override DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        if (index is DyInteger i)
        {
            var arr = (DyArray)self;
            var ix = (int)i.Value;

            if (!CorrectIndex(arr, ref ix, insert: false))
                return ctx.IndexOutOfRange(index);

            arr[ix] = value;
            return Nil;
        }

        return ctx.InvalidType(index);
    }

    protected override DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject item)
    {
        var arr = (DyArray)self;
        return arr.IndexOf(ctx, item) != -1 ? True : False;
    }

    private static DyObject ToStringOrLiteral(bool literal, DyObject arg, ExecutionContext ctx)
    {
        var arr = (DyArray)arg;
        var sb = new StringBuilder();
        sb.Append('[');

        for (var i = 0; i < arr.Count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            var str = literal ? arr[i].ToLiteral(ctx) : arr[i].ToString(ctx);

            if (ctx.Error is not null)
                return Nil;

            sb.Append(str.Value);
        }

        sb.Append(']');
        return new DyString(sb.ToString());
    }
    #endregion

    internal static bool CorrectIndex(DyArray arr, ref int index, bool insert = false)
    {
        index = index < 0 ? arr.Count + index : index;
        var max = insert ? arr.Count : arr.Count - 1;

        if (index < 0 || index > max)
            return false;

        return true;
    }

    [InstanceMethod(Method.Add)]
    internal static void AddItem(DyArray self, DyObject value) => self.Add(value);

    [InstanceMethod(Method.Insert)]
    internal static void InsertItem(DyArray self, int index, DyObject value)
    {
        if (!CorrectIndex(self, ref index, insert: true))
            throw new DyCodeException(DyError.IndexOutOfRange, index);
    
        self.Insert(index, value);
    }

    [InstanceMethod]
    internal static void AddRange(DyArray self, IEnumerable<DyObject> values)
    {
        foreach (var o in values)
            self.Add(o);
    }

    [InstanceMethod]
    internal static void InsertRange(DyArray self, int index, IEnumerable<DyObject> values)
    {
        if (!CorrectIndex(self, ref index, insert: true))
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        foreach (var e in values)
            self.Insert(index++, e);
    }

    [InstanceMethod(Method.Remove)]
    internal static bool RemoveItem(ExecutionContext ctx, DyArray self, DyObject value)
    {
        var ix = self.IndexOf(ctx, value);

        if (ctx.HasErrors || ix == -1)
            return false;

        self.RemoveAt(ix);
        return true;
    }

    [InstanceMethod(Method.RemoveAt)]
    internal static void RemoveItemAt(ExecutionContext ctx, DyArray self, int index)
    {
        if (!CorrectIndex(self, ref index, insert: true))
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        self.RemoveAt(index);
    }

    [InstanceMethod(Method.RemoveRange)]
    internal static void RemoveRange(ExecutionContext ctx, DyArray self, IEnumerable<DyObject> values)
    {
        var strict = values.ToArray();

        foreach (var e in strict)
        {
            var ix = self.IndexOf(ctx, e);

            if (ctx.HasErrors)
                break;

            self.RemoveAt(ix);
        }
    }

    [InstanceMethod(Method.RemoveRangeAt)]
    internal static void RemoveRangeAt(DyArray self, int index, int? count = null)
    {
        if (!CorrectIndex(self, ref index))
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        if (count is null)
            count = self.Count - index;

        if (index + count > self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        self.RemoveRange(index, count.Value);
    }

    [InstanceMethod]
    internal static void RemoveAll(ExecutionContext ctx, DyArray self, DyFunction predicate)
    {
        var toDelete = new List<int>();

        for (var i = 0; i < self.Count; i++)
        {
            var o = self[i];
            var res = predicate.Call(ctx, o);

            if (!res.IsFalse())
                toDelete.Add(i);
        }

        var shift = 0;

        foreach (var ix in toDelete)
        {
            self.RemoveAt(ix + shift);
            shift--;
        }
    }

    [InstanceMethod(Method.Clear)]
    internal static void ClearItems(DyArray self) => self.Clear();

    [InstanceMethod]
    internal static int IndexOf(ExecutionContext ctx, DyArray self, DyObject value) => self.IndexOf(ctx, value);

    [InstanceMethod]
    internal static int LastIndexOf(ExecutionContext ctx, DyArray self, DyObject value) => self.LastIndexOf(ctx, value);

    [InstanceMethod(Method.Sort)]
    internal static void SortBy(ExecutionContext ctx, DyArray self, DyFunction? comparer = null)
    {
        var sortComparer = new SortComparer(comparer, ctx);
        self.Compact();
        Array.Sort(self.UnsafeAccess(), 0, self.Count, sortComparer);
    }

    [InstanceMethod]
    internal static void Swap(DyArray self, int index, int other)
    {
        if (!CorrectIndex(self, ref index))
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        if (!CorrectIndex(self, ref other))
            throw new DyCodeException(DyError.IndexOutOfRange, other);

        var fst = self[index];
        var snd = self[other];
        self[index] = snd;
        self[other] = fst;
    }

    [InstanceMethod]
    internal static void Compact(ExecutionContext ctx, DyArray self, DyFunction? predicate = null)
    {
        if (self.Count == 0)
            return;

        var idx = 0;

        while (idx < self.Count)
        {
            var e = self[idx];
            bool flag;

            if (predicate is not null)
            {
                var res = predicate.Call(ctx, e);
                flag = res.IsTrue();
            }
            else
                flag = e.TypeId == Dy.Nil;

            if (flag)
                self.RemoveAt(idx);
            else
                idx++;
        }
    }

    [InstanceMethod]
    internal static void Reverse(DyArray self)
    {
        self.Compact();
        Array.Reverse(self.UnsafeAccess());
    }

    [StaticMethod(Method.Array)]
    internal static DyObject[] New(params DyObject[] values) => values;

    [StaticMethod(Method.Sort)]
    internal static DyObject StaticSortBy(ExecutionContext ctx, DyObject values, DyFunction comparer)
    {
        var arr = values;

        if (values.TypeId != Dy.Array)
        {
            arr = ctx.RuntimeContext.Types[values.TypeId].Cast(ctx, values, ctx.RuntimeContext.Array);

            if (ctx.HasErrors)
                return Nil;
        }

        SortBy(ctx, (DyArray)arr, comparer);
        return arr;
    }

    [StaticMethod]
    internal static DyObject[] Empty(ExecutionContext ctx, int count, [ParameterName("default")] DyObject? def = null)
    {
        var arr = new DyObject[count];
        def ??= Nil;

        if (def.TypeId == Dy.Iterator)
            def = ((DyIterator)def).GetIteratorFunction();

        if (def is DyFunction func)
        {
            for (var i = 0; i < count; i++)
            {
                var res = func.Call(ctx);

                if (ctx.HasErrors)
                    return Array.Empty<DyObject>();

                arr[i] = res;
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
                arr[i] = def;
        }

        return arr;
    }

    [StaticMethod]
    internal static DyObject[] Concat(ExecutionContext ctx, params DyObject[] values) =>
        DyCollection.ConcatValues(ctx, values);

    [StaticMethod]
    internal static DyObject Copy(ExecutionContext ctx, DyArray source, int index = 0, DyArray? destination = null, int destinationIndex = 0, int? count = null)
    {
        if (count is null)
            count = source.Count;

        destination ??= new DyArray(new DyObject[destinationIndex + count.Value]);

        if (index < 0 || index >= source.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (destinationIndex < 0 || destinationIndex >= destination.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (index + count < 0 || index + count > source.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (destinationIndex + count < 0 || destinationIndex + count > destination.Count)
            throw new DyCodeException(DyError.IndexOutOfRange);

        Array.Copy(source.UnsafeAccess(), index, destination.UnsafeAccess(), destinationIndex, count.Value);
        return destination;
    }
}
