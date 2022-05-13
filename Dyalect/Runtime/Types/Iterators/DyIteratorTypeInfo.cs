using Dyalect.Codegen;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyIteratorTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Iterator);

    public override int ReflectedTypeId => Dy.Iterator;

    public DyIteratorTypeInfo() => AddMixins(Dy.Lookup);

    #region Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right) => DyIterator.Create(Concat(ctx, left, right));

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return ctx.HasErrors ? Nil : DyInteger.Get(seq.Count());
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (index is not DyInteger ix)
            return Nil;

        var i = (int)ix.Value;

        try
        {
            var iter = DyIterator.ToEnumerable(ctx, self);
            return i < 0 ? iter.ElementAt(^-i) : iter.ElementAt(i);
        }
        catch (ArgumentOutOfRangeException)
        {
            ctx.Error = new(DyError.IndexOutOfRange, index);
            return Nil;
        }
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Tuple => new DyTuple(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            Dy.Array => new DyArray(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            Dy.Function => ((DyIterator)self).GetIteratorFunction(),
            Dy.Set => ConvertToSet(ctx, self),
            _ => base.CastOp(ctx, self, targetType)
        };

    private static DyObject ConvertToSet(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Nil;

        return ToSet(seq);
    }
    #endregion

    [InstanceMethod]
    internal static bool Contains(ExecutionContext ctx, IEnumerable<DyObject> self, DyObject item) =>
        self.Any(o => o.Equals(item, ctx));

    [InstanceMethod]
    internal static DyObject ToArray(IEnumerable<DyObject> self) => new DyArray(self.ToArray());

    [InstanceMethod]
    internal static DyObject ToTuple(IEnumerable<DyObject> self) => new DyTuple(self.ToArray());

    [InstanceMethod]
    internal static DyObject ToDictionary(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction keySelector, DyFunction? valueSelector = null)
    {
        try
        {
            var map =
                valueSelector is not null
                ? self.ToDictionary(dy => keySelector.Call(ctx, dy), dy => valueSelector.Call(ctx, dy))
                : self.ToDictionary(dy => keySelector.Call(ctx, dy));
            return new DyDictionary(map);
        }
        catch (ArgumentException)
        {
            return ctx.KeyAlreadyPresent();
        }
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Take(IEnumerable<DyObject> self, int count) => self.Take(count < 0 ? 0 : count);

    [InstanceMethod]
    internal static IEnumerable<DyObject> Skip(IEnumerable<DyObject> self, int count) => self.Skip(count < 0 ? 0 : count);

    [InstanceMethod]
    internal static DyObject First(IEnumerable<DyObject> self) => self.FirstOrDefault() ?? Nil;

    [InstanceMethod]
    internal static DyObject Single(IEnumerable<DyObject> self)
    {
        var two = self.Take(2).ToList();

        if (two.Count > 1 || two.Count == 0)
            return Nil;

        return two[0];
    }

    [InstanceMethod]
    internal static DyObject Last(ExecutionContext ctx, DyObject self) =>
        DyIterator.ToEnumerable(ctx, self).LastOrDefault() ?? Nil;

    [InstanceMethod]
    internal static IEnumerable<DyObject> Reverse(IEnumerable<DyObject> self) => self.Reverse();

    [InstanceMethod]
    internal static IEnumerable<DyObject> Slice(IEnumerable<DyObject> self, int index = 0, int? endIndex = null)
    {
        int? count = null;

        if (index < 0)
            index = (count ??= self.Count()) + index;

        if (endIndex is null)
        {
            if (index == 0)
                return self;

            return self.Skip(index);
        }

        if (endIndex < 0)
            endIndex = (count ?? self.Count()) + endIndex - 1;

        return self.Skip(index).Take(endIndex.Value - index + 1);
    }

    [InstanceMethod]
    internal static DyObject ElementAt(IEnumerable<DyObject> self, int index)
    {
        //TODO: validate logic
        try
        {
            return index < 0 ? self.ElementAt(^-index) : self.ElementAt(index);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new DyCodeException(DyError.IndexOutOfRange, index);
        }
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Sort(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction? comparer = null)
    {
        var sortComparer = new SortComparer(comparer, ctx);
        return self.OrderBy(dy => dy, sortComparer);
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Shuffle(IEnumerable<DyObject> self)
    {
        var rnd = new Random();
        var last = 0;

        int sorter(DyObject _)
        {
            var n = rnd.Next();
            if (last != 0 && n > last) n = -n;
            last = n;
            return n;
        }

        return self.OrderBy(sorter);
    }

    [InstanceMethod]
    internal static int Count(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction? predicate = null)
    {
        if (predicate is not null)
            return self.Count(dy => predicate.Call(ctx, dy).IsTrue());
        else
            return self.Count();
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Map(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction converter) =>
        self.Select(dy => converter.Call(ctx, dy));

    [InstanceMethod]
    internal static IEnumerable<DyObject> Filter(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction predicate) =>
        self.Where(o => predicate.Call(ctx, o).IsTrue());

    [InstanceMethod]
    internal static IEnumerable<DyObject> TakeWhile(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction predicate) =>
        self.TakeWhile(o => predicate.Call(ctx, o).IsTrue());

    [InstanceMethod]
    internal static IEnumerable<DyObject> SkipWhile(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction predicate) =>
        self.SkipWhile(o => predicate.Call(ctx, o).IsTrue());

    [InstanceMethod]
    internal static DyObject Reduce(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction converter, [Default(0)]DyObject initial) =>
        self.Aggregate(initial, (x, y) => converter.Call(ctx, x, y));

    [InstanceMethod]
    internal static bool Any(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction predicate) =>
        self.Any(o => predicate.Call(ctx, o).IsTrue());

    [InstanceMethod]
    internal static bool All(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction predicate) =>
        self.All(o => predicate.Call(ctx, o).IsTrue());

    [InstanceMethod]
    internal static void ForEach(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction action)
    {
        foreach (var o in self)
            action.Call(ctx, o);
    }

    [InstanceMethod]
    internal static DyObject ToSet(IEnumerable<DyObject> self)
    {
        var set = new HashSet<DyObject>();
        set.UnionWith(self);
        return new DySet(set);
    }

    [InstanceMethod]
    internal static IEnumerable<DyObject> Distinct(ExecutionContext ctx, IEnumerable<DyObject> self, DyFunction? selector = null)
    {
        if (selector is not null)
            return self.Distinct(new EqualityComparer(ctx, selector));
        else
            return self.Distinct();
    }

    [StaticMethod]
    internal static IEnumerable<DyObject> Concat(ExecutionContext ctx, params DyObject[] values) =>
        new MultiPartEnumerable(ctx, values);

    [StaticMethod(Method.Iterator)]
    internal static IEnumerable<DyObject> Iterator(ExecutionContext ctx, params DyObject[] values) => Concat(ctx, values);

    [StaticMethod]
    internal static IEnumerable<DyObject> Range(ExecutionContext ctx, [Default(0)]DyObject start, [Default]DyObject end, [Default(1)]DyObject step, bool exclusive = false) =>
        GenerateRange(ctx, start, end ?? Nil, step, exclusive);

    private static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject start, DyObject end, DyObject step, bool exclusive)
    {
        var elem = start;
        var inf = end.TypeId is Dy.Nil;
        var types = ctx.RuntimeContext.Types;

        if (inf)
        {
            while (true)
            {
                yield return elem;
                elem = elem.Add(step, ctx);
            }
        }

        var up = step.Greater(DyInteger.Zero, ctx);
        Func<DyObject, DyObject, ExecutionContext, bool> predicate =
            up && exclusive ? Extensions.Lesser :
                (
                    up ? Extensions.LesserOrEquals
                    : exclusive ? Extensions.Greater : Extensions.GreaterOrEquals
                );
        
        while (predicate(elem, end, ctx))
        {
            yield return elem;
            elem = elem.Add(step, ctx);
        }
    }

    [StaticMethod]
    internal static IEnumerable<DyObject> Empty() => Enumerable.Empty<DyObject>();

    [StaticMethod]
    internal static IEnumerable<DyObject> Repeat(ExecutionContext ctx, DyObject value) => Repeater(ctx, value);

    private static IEnumerable<DyObject> Repeater(ExecutionContext ctx, DyObject val)
    {
        if (val.TypeId is Dy.Iterator)
            val = ((DyIterator)val).GetIteratorFunction();

        if (val is DyFunction func)
        {
            while (true)
            {
                var res = func.Call(ctx);
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
