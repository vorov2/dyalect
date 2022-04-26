using Dyalect.Codegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyArrayTypeInfo : DyCollectionTypeInfo
{
    public override string ReflectedTypeName => nameof(DyType.Array);

    public override int ReflectedTypeId => DyType.Array;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len
        | SupportedOperations.Iter | SupportedOperations.Lit;

    public DyArrayTypeInfo() => AddMixin(DyType.Collection);

    #region Operations
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
            var str = literal ? arr[i].ToLiteral(ctx) : arr[i].ToString(ctx);

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
    #endregion

    [InstanceMethod(Method.Add)]
    internal static void AddItem(DyArray self, DyObject value) => self.Add(value);

    [InstanceMethod(Method.Insert)]
    internal static void InsertItem(ExecutionContext ctx, DyArray self, int index, DyObject value)
    {
        if (index < 0 || index > self.Count)
            ctx.IndexOutOfRange();
        else
            self.Insert(index, value);
    }

    [InstanceMethod]
    internal static void AddRange(ExecutionContext ctx, DyArray self, DyObject values)
    {
        foreach (var o in DyIterator.ToEnumerable(ctx, values))
        {
            if (ctx.HasErrors)
                break;
            self.Add(o);
        }
    }

    [InstanceMethod]
    internal static void InsertRange(ExecutionContext ctx, DyArray self, int index, DyObject values)
    {
        if (index < 0 || index > self.Count)
        {
            ctx.IndexOutOfRange();
            return;
        }

        var coll = DyIterator.ToEnumerable(ctx, values);

        if (!ctx.HasErrors)
        {
            foreach (var e in coll)
                self.Insert(index++, e);
        }
    }

    [InstanceMethod(Method.Remove)]
    internal static bool RemoveItem(ExecutionContext ctx, DyArray self, DyObject value) => self.Remove(ctx, value);

    [InstanceMethod(Method.RemoveAt)]
    internal static void RemoveItemAt(ExecutionContext ctx, DyArray self, int index)
    {
        if (index < 0 || index >= self.Count)
        {
            ctx.IndexOutOfRange(index);
            return;
        }

        self.RemoveAt(index);
    }

    [InstanceMethod(Method.RemoveRange)]
    internal static void RemoveRange(ExecutionContext ctx, DyArray self, DyObject values)
    {
        var coll = DyIterator.ToEnumerable(ctx, values);

        if (ctx.HasErrors)
            return;

        var strict = coll.ToArray();

        foreach (var e in strict)
        {
            self.Remove(ctx, e);

            if (ctx.HasErrors)
                break;
        }
    }

    [InstanceMethod(Method.RemoveRangeAt)]
    internal static void RemoveRangeAt(ExecutionContext ctx, DyArray self, int index, [Default]int? count)
    {
        if (index < 0 || index >= self.Count)
        {
            ctx.IndexOutOfRange(index);
            return;
        }

        if (count is null)
            count = self.Count - index;

        if (index + count > self.Count)
        {
            ctx.IndexOutOfRange();
            return;
        }

        self.RemoveRange(index, count.Value);
    }

    [InstanceMethod]
    internal static void RemoveAll(ExecutionContext ctx, DyArray self, DyObject predicate)
    {
        var toDelete = new List<DyObject>();

        foreach (var o in self)
        {
            var res = predicate.Invoke(ctx, o);

            if (ctx.HasErrors)
                return;

            if (!res.IsFalse())
                toDelete.Add(o);
        }

        foreach (var o in toDelete)
        {
            self.Remove(ctx, o);

            if (ctx.HasErrors)
                return;
        }
    }

    [InstanceMethod(Method.Clear)]
    internal static void ClearItems(DyArray self) => self.Clear();

    [InstanceMethod]
    internal static int IndexOf(ExecutionContext ctx, DyArray self, DyObject value) => self.IndexOf(ctx, value);

    [InstanceMethod]
    internal static int LastIndexOf(ExecutionContext ctx, DyArray self, DyObject value) => self.LastIndexOf(ctx, value);

    [InstanceMethod(Method.Sort)]
    internal static void SortBy(ExecutionContext ctx, DyArray self, [Default]DyObject comparer)
    {
        var sortComparer = new SortComparer(comparer, ctx);
        self.Compact();
        Array.Sort(self.UnsafeAccessValues(), 0, self.Count, sortComparer);
    }

    [InstanceMethod]
    internal static void Swap(ExecutionContext ctx, DyArray self, DyObject index, DyObject other)
    {
        var fst = self.GetItem(index, ctx);

        if (ctx.HasErrors)
            return;

        var snd = self.GetItem(other, ctx);

        if (ctx.HasErrors)
            return;

        self.SetItem(index, snd, ctx);
        self.SetItem(other, fst, ctx);
    }

    [InstanceMethod]
    internal static void Compact(ExecutionContext ctx, DyArray self, [Default]DyObject predicate)
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
                var res = predicate.Invoke(ctx, e);

                if (ctx.HasErrors)
                    return;

                flag = res.IsTrue();
            }
            else
                flag = e.IsNil();

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
        Array.Reverse(self.UnsafeAccessValues());
    }

    [StaticMethod(Method.Array)]
    internal static DyObject New([VarArg]DyObject[] values) => new DyArray(values);

    [StaticMethod(Method.Sort)]
    internal static DyObject StaticSortBy(ExecutionContext ctx, DyObject values, DyObject comparer)
    {
        var arr = values;

        if (values.TypeId != DyType.Array)
        {
            arr = ctx.RuntimeContext.Types[values.TypeId].Cast(ctx, values, ctx.RuntimeContext.Array);

            if (ctx.HasErrors)
                return Nil;
        }

        SortBy(ctx, (DyArray)arr, comparer);
        return arr;
    }

    [StaticMethod]
    internal static DyObject Empty(ExecutionContext ctx, int count, [Default][ParameterName("default")]DyObject def)
    {
        var arr = new DyObject[count];
        def ??= Nil;

        if (def.TypeId == DyType.Iterator)
            def = ((DyIterator)def).GetIteratorFunction();

        if (def is DyFunction func)
        {
            for (var i = 0; i < count; i++)
            {
                var res = func.Call(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                arr[i] = res;
            }
        }
        else
        {
            for (var i = 0; i < count; i++)
                arr[i] = def;
        }

        return new DyArray(arr);
    }

    [StaticMethod]
    internal static DyObject Concat(ExecutionContext ctx, [VarArg]DyObject values) =>
        new DyArray(DyCollection.ConcatValues(ctx, values));

    [StaticMethod]
    internal static DyObject Copy(ExecutionContext ctx, DyArray source, [Default(0)]int index, [Default]DyArray destination, [Default(0)]int destinationIndex, [Default]int? count)
    {
        if (count is null)
            count = source.Count;

        destination ??= new DyArray(new DyObject[destinationIndex + count.Value]);

        if (index < 0 || index >= source.Count)
            return ctx.IndexOutOfRange();

        if (destinationIndex < 0 || destinationIndex >= destination.Count)
            return ctx.IndexOutOfRange();

        if (index + count < 0 || index + count > source.Count)
            return ctx.IndexOutOfRange();

        if (destinationIndex + count < 0 || destinationIndex + count > destination.Count)
            return ctx.IndexOutOfRange();

        Array.Copy(source.UnsafeAccessValues(), index, destination.UnsafeAccessValues(), destinationIndex, count.Value);
        return destination;
    }
}
