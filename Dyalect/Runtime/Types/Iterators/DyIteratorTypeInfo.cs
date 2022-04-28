using Dyalect.Codegen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyIteratorTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

    public override string ReflectedTypeName => nameof(Dy.Iterator);

    public override int ReflectedTypeId => Dy.Iterator;

    #region Operations
    protected override DyObject LengthOp(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return ctx.HasErrors ? Nil : DyInteger.Get(seq.Count());
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject self, DyObject format)
    {
        var fn = self.GetIterator(ctx)!;

        if (ctx.HasErrors)
            return Nil;

        fn.Reset(ctx);

        if (ctx.HasErrors)
            return Nil;

        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyString.Empty;

        var sb = new StringBuilder();
        sb.Append('{');
        var c = 0;

        foreach (var e in seq)
        {
            if (c > 0)
                sb.Append(", ");
            var str = e.ToString(ctx);

            if (ctx.Error is not null)
                return DyString.Empty;

            sb.Append(str.GetString());
            c++;
        }

        if (c == 1)
            sb.Append(", ");

        sb.Append('}');
        return new DyString(sb.ToString());
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (!index.Is(Dy.Integer)) return Nil;

        var i = (int)index.GetInteger();

        try
        {
            var iter = DyIterator.ToEnumerable(ctx, self);
            return i < 0 ? iter.ElementAt(^-i) : iter.ElementAt(i);
        }
        catch (IndexOutOfRangeException)
        {
            return ctx.IndexOutOfRange();
        }
    }

    protected override DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject item)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return seq.Any(o => o.Equals(item, ctx)) ? True : False;
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => new DyTuple(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            Dy.Array => new DyArray(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            Dy.Function => ((DyIterator)self).GetIteratorFunction(),
            Dy.Set => ToSet(ctx, self),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [InstanceMethod]
    internal static DyObject ToArray(ExecutionContext ctx, DyObject self)
    {
        var res = ConvertToArray(ctx, self);
        return res is null ? Nil : new DyArray(res.ToArray());
    }

    [InstanceMethod]
    internal static DyObject ToTuple(ExecutionContext ctx, DyObject self)
    {
        var res = ConvertToArray(ctx, self);
        return res is null ? Nil : new DyTuple(res.ToArray());
    }

    private static List<DyObject>? ConvertToArray(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return ctx.HasErrors ? null : seq.ToList();
    }

    [InstanceMethod]
    internal static DyObject ToDictionary(ExecutionContext ctx, DyObject self, DyObject keySelector, DyObject? valueSelector = null)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        try
        {
            var map = 
                valueSelector is not null
                ? seq.ToDictionary(dy => keySelector.Invoke(ctx, dy), dy => valueSelector.Invoke(ctx, dy))
                : seq.ToDictionary(dy => keySelector.Invoke(ctx, dy));
            return new DyDictionary(map);
        }
        catch (ArgumentException)
        {
            return ctx.KeyAlreadyPresent();
        }
    }

    [InstanceMethod]
    internal static DyObject Take(ExecutionContext ctx, DyObject self, int count)
    {
        if (count < 0) count = 0;
        return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Take(count));
    }

    [InstanceMethod]
    internal static DyObject Skip(ExecutionContext ctx, DyObject self, int count)
    {
        if (count < 0) count = 0;
        return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Skip(count));
    }

    [InstanceMethod]
    internal static DyObject First(ExecutionContext ctx, DyObject self) =>
        DyIterator.ToEnumerable(ctx, self).FirstOrDefault() ?? Nil;

    [InstanceMethod]
    internal static DyObject Last(ExecutionContext ctx, DyObject self) =>
        DyIterator.ToEnumerable(ctx, self).LastOrDefault() ?? Nil;

    [InstanceMethod]
    internal static DyObject Reverse(ExecutionContext ctx, DyObject self) =>
        DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Reverse());

    [InstanceMethod]
    internal static DyObject Slice(ExecutionContext ctx, DyObject self, int index = 0, int? endIndex = null)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        int? count = null;

        if (index < 0)
            index = (count ??= seq.Count()) + index;

        if (endIndex is null)
        {
            if (index == 0)
                return self;

            return DyIterator.Create(seq.Skip(index));
        }

        if (endIndex < 0)
            endIndex = (count ?? seq.Count()) + endIndex - 1;

        return DyIterator.Create(seq.Skip(index).Take(endIndex.Value - index + 1));
    }

    [InstanceMethod]
    internal static DyObject ElementAt(ExecutionContext ctx, DyObject self, int index)
    {
        try
        {
            var iter = DyIterator.ToEnumerable(ctx, self);
            return index < 0 ? iter.ElementAt(^-index) : iter.ElementAt(index);
        }
        catch (IndexOutOfRangeException)
        {
            return ctx.IndexOutOfRange();
        }
    }

    [InstanceMethod]
    internal static DyObject Sort(ExecutionContext ctx, DyObject self, DyObject? comparer = null)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var sortComparer = new SortComparer(comparer, ctx);
        var sorted = seq.OrderBy(dy => dy, sortComparer);
        return DyIterator.Create(sorted);
    }

    [InstanceMethod]
    internal static DyObject Shuffle(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var rnd = new Random();
        var last = 0;

        int sorter(DyObject _)
        {
            var n = rnd.Next();
            if (last != 0 && n > last) n = -n;
            last = n;
            return n;
        }

        var sorted = seq.OrderBy(sorter);
        return DyIterator.Create(sorted);
    }

    [InstanceMethod]
    internal static int Count(ExecutionContext ctx, DyObject self, DyObject? predicate = null)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (predicate is not null)
        {
            return seq.Count(dy =>
            {
                var res = predicate.Invoke(ctx, dy);

                if (ctx.HasErrors)
                    throw new DyErrorException(ctx.Error!);

                return res.IsTrue();
            });
        }
        else
            return seq.Count();
    }

    [InstanceMethod]
    internal static DyObject Map(ExecutionContext ctx, DyObject self, DyObject converter)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var xs = seq.Select(dy => converter.Invoke(ctx, dy));
        return DyIterator.Create(xs);
    }

    [InstanceMethod]
    internal static DyObject Filter(ExecutionContext ctx, DyObject self, DyObject predicate)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var xs = seq.Where(o => predicate.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    [InstanceMethod]
    internal static DyObject TakeWhile(ExecutionContext ctx, DyObject self, DyObject predicate)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var xs = seq.TakeWhile(o => predicate.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    [InstanceMethod]
    internal static DyObject SkipWhile(ExecutionContext ctx, DyObject self, DyObject predicate)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var xs = seq.SkipWhile(o => predicate.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    [InstanceMethod]
    internal static DyObject Reduce(ExecutionContext ctx, DyObject self, DyObject converter, [Default(0)]DyObject initial)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        return seq.Aggregate(initial, (x, y) => converter.Invoke(ctx, x, y));
    }

    [InstanceMethod]
    internal static bool Any(ExecutionContext ctx, DyObject self, DyObject predicate)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return false;

        var res = seq.Any(o => predicate.Invoke(ctx, o).IsTrue());
        return res;
    }

    [InstanceMethod]
    internal static bool All(ExecutionContext ctx, DyObject self, DyObject predicate)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return false;

        var res = seq.All(o => predicate.Invoke(ctx, o).IsTrue());
        return res;
    }

    [InstanceMethod]
    internal static DyObject ForEach(ExecutionContext ctx, DyObject self, DyObject action)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        foreach (var o in seq)
        {
            action.Invoke(ctx, o);

            if (ctx.HasErrors)
                return Nil;
        }

        return Nil;
    }

    [InstanceMethod]
    internal static DyObject ToSet(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        var set = new HashSet<DyObject>();
        set.UnionWith(seq);
        return new DySet(set);
    }

    [InstanceMethod]
    internal static DyObject Distinct(ExecutionContext ctx, DyObject self, DyObject? selector = null)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        IEnumerable<DyObject> res;

        if (selector is not null)
            res = seq.Distinct(new EqualityComparer(ctx, selector));
        else
            res = seq.Distinct();

        return DyIterator.Create(res);
    }

    [StaticMethod]
    internal static DyObject Concat(ExecutionContext ctx, params DyObject[] values) =>
        DyIterator.Create(new MultiPartEnumerable(ctx, values));

    [StaticMethod(Method.Iterator)]
    internal static DyObject Iterator(ExecutionContext ctx, params DyObject[] values) => Concat(ctx, values);

    [StaticMethod]
    internal static DyObject Range(ExecutionContext ctx, [Default(0)]DyObject start, [Default]DyObject end, [Default(1)]DyObject step, bool exclusive = false) =>
        DyIterator.Create(GenerateRange(ctx, start, end ?? Nil, step, exclusive));

    private static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject start, DyObject end, DyObject step, bool exclusive)
    {
        var elem = start;
        var inf = end.Is(Dy.Nil);
        var types = ctx.RuntimeContext.Types;

        if (inf)
        {
            while (true)
            {
                yield return elem;
                elem = types[elem.TypeId].Add(ctx, elem, step);

                if (ctx.HasErrors)
                    yield break;
            }
        }

        var up = ReferenceEquals(types[step.TypeId].Gt(ctx, step, DyInteger.Zero), True);

        if (ctx.HasErrors)
            yield break;

        Func<ExecutionContext, DyObject, DyObject, DyObject> predicate = up && exclusive
                ? types[start.TypeId].Lt : up ? types[start.TypeId].Lte : exclusive
                ? types[start.TypeId].Gt : types[start.TypeId].Gte;

        while (ReferenceEquals(predicate(ctx, elem, end), True))
        {
            yield return elem;
            elem = types[elem.TypeId].Add(ctx, elem, step);

            if (ctx.HasErrors)
                yield break;
        }
    }

    [StaticMethod]
    internal static DyObject Empty() => DyIterator.Create(Enumerable.Empty<DyObject>());

    [StaticMethod]
    internal static DyObject Repeat(ExecutionContext ctx, DyObject value) => DyIterator.Create(Repeater(ctx, value));

    private static IEnumerable<DyObject> Repeater(ExecutionContext ctx, DyObject val)
    {
        if (val.TypeId == Dy.Iterator)
            val = ((DyIterator)val).GetIteratorFunction();

        if (val is DyFunction func)
        {
            if (ctx.HasErrors)
                yield break;

            while (true)
            {
                var res = func.Call(ctx);

                if (ctx.HasErrors)
                    yield break;

                yield return res;
            }
        }
        else
        {
            while (true)
                yield return val;
        }
    }
}
