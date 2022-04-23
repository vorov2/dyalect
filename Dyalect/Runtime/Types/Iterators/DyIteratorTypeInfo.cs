using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

internal sealed class DyIteratorTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

    public override string TypeName => DyTypeNames.Iterator;

    public override int ReflectedTypeId => DyType.Iterator;

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => GetCount(ctx, arg);

    protected override DyObject ToStringOp(DyObject self, DyObject format, ExecutionContext ctx)
    {
        var fn = self.GetIterator(ctx)!;

        if (ctx.HasErrors)
            return Default();

        fn.Reset(ctx);

        if (ctx.HasErrors)
            return DyNil.Instance;

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

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
    {
        if (index.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, index);

        var i = (int)index.GetInteger();

        try
        {
            var iter = DyIterator.ToEnumerable(ctx, self);

            if (i < 0)
            {
                iter = iter.Reverse();
                i = -i;
            }

            return iter.ElementAt(i);
        }
        catch (IndexOutOfRangeException)
        {
            return ctx.IndexOutOfRange();
        }
    }

    protected override DyObject ContainsOp(DyObject self, DyObject item, ExecutionContext ctx)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return seq.Any(o => o.Equals(item, ctx)) ? DyBool.True : DyBool.False;
    }

    private DyObject ToMap(ExecutionContext ctx, DyObject self, DyObject keySelectorObj, DyObject valueSelectorObj)
    {
        if (keySelectorObj.TypeId != DyType.Function)
            return ctx.InvalidType(DyType.Function, keySelectorObj);

        if (valueSelectorObj.TypeId != DyType.Function && valueSelectorObj.TypeId != DyType.Nil)
            return ctx.InvalidType(DyType.Function, DyType.Nil, valueSelectorObj);

        var keySelector = (DyFunction)keySelectorObj;
        var valueSelector = valueSelectorObj as DyFunction;
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        try
        {
            var map = 
                valueSelector is not null
                ? seq.ToDictionary(dy => keySelector.Call(ctx, dy), dy => valueSelector.Call(ctx, dy))
                : seq.ToDictionary(dy => keySelector.Call(ctx, dy));
            return new DyDictionary(map);
        }
        catch (ArgumentException)
        {
            return ctx.KeyAlreadyPresent();
        }
    }

    private static List<DyObject>? ConvertToArray(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return ctx.HasErrors ? null : seq.ToList();
    }

    private DyObject ToArray(ExecutionContext ctx, DyObject self)
    {
        var res = ConvertToArray(ctx, self);
        return res is null ? DyNil.Instance : new DyArray(res.ToArray());
    }

    private DyObject ToTuple(ExecutionContext ctx, DyObject self)
    {
        var res = ConvertToArray(ctx, self);
        return res is null ? DyNil.Instance : new DyTuple(res.ToArray());
    }

    private static DyObject GetCount(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        return ctx.HasErrors ? DyNil.Instance : DyInteger.Get(seq.Count());
    }

    private DyObject ElementAt(ExecutionContext ctx, DyObject self, DyObject index) => GetOp(self, index, ctx);

    private DyObject Take(ExecutionContext ctx, DyObject self, DyObject count)
    {
        if (count.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, self);

        var i = (int)count.GetInteger();

        if (i < 0)
            i = 0;

        return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Take(i));
    }

    private DyObject Skip(ExecutionContext ctx, DyObject self, DyObject count)
    {
        if (count.TypeId != DyType.Integer)
            return ctx.InvalidType(DyType.Integer, self);

        var i = (int)count.GetInteger();

        if (i < 0)
            i = 0;

        return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Skip(i));
    }

    private DyObject First(ExecutionContext ctx, DyObject self) =>
        DyIterator.ToEnumerable(ctx, self).FirstOrDefault() ?? DyNil.Instance;

    private DyObject Last(ExecutionContext ctx, DyObject self) =>
        DyIterator.ToEnumerable(ctx, self).LastOrDefault() ?? DyNil.Instance;

    private DyObject Concat(ExecutionContext ctx, DyObject tuple) =>
        DyIterator.Create(new MultiPartEnumerable(ctx, ((DyTuple)tuple).GetValues()));

    private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        if (!fromElem.IsInteger(ctx)) return Default();
        if (toElem.NotNil() && !toElem.IsInteger(ctx)) return Default();

        var beg = (int)fromElem.GetInteger();
        int? count = null;

        if (beg < 0)
            beg = (count ??= seq.Count()) + beg;

        if (toElem.IsNil())
        {
            if (beg == 0)
                return self;

            return DyIterator.Create(seq.Skip(beg));
        }

        var end = (int)toElem.GetInteger();

        if (end < 0)
            end = (count ?? seq.Count()) + end - 1;

        return DyIterator.Create(seq.Skip(beg).Take(end - beg + 1));
    }

    private DyObject Reverse(ExecutionContext ctx, DyObject self) =>
        DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Reverse());
    
    private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var comparer = new SortComparer(functor, ctx);
        var sorted = seq.OrderBy(dy => dy, comparer);
        return DyIterator.Create(sorted);
    }

    private DyObject Shuffle(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;
        
        var rnd = new Random();
        var sorted = seq.OrderBy(_ => rnd.Next());
        return DyIterator.Create(sorted);
    }

    private DyObject CountBy(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (functor.NotNil())
        {
            return DyInteger.Get(seq.Count(dy =>
            {
                var res = functor.Invoke(ctx, dy);

                if (ctx.HasErrors)
                    throw new DyErrorException(ctx.Error!);

                return res.IsTrue();
            }));
        }
        else
            return DyInteger.Get(seq.Count());
    }

    private DyObject Map(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var xs = seq.Select(dy => functor.Invoke(ctx, dy));
        return DyIterator.Create(xs);
    }

    private DyObject TakeWhile(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var xs = seq.TakeWhile(o => functor.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    private DyObject SkipWhile(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var xs = seq.SkipWhile(o => functor.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    private DyObject Filter(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var xs = seq.Where(o => functor.Invoke(ctx, o).IsTrue());
        return DyIterator.Create(xs);
    }

    private DyObject Reduce(ExecutionContext ctx, DyObject self, DyObject functor, DyObject initial)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        return seq.Aggregate(initial, (x, y) => functor.Invoke(ctx, x, y));
    }

    private DyObject Any(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var res = seq.Any(o => functor.Invoke(ctx, o).IsTrue());
        return res ? DyBool.True : DyBool.False;
    }

    private DyObject All(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        var res = seq.All(o => functor.Invoke(ctx, o).IsTrue());
        return res ? DyBool.True : DyBool.False;
    }

    private DyObject ForEach(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return DyNil.Instance;

        foreach (var o in seq)
        {
            functor.Invoke(ctx, o);

            if (ctx.HasErrors)
                return Default();
        }

        return Default();
    }

    private DyObject ToSet(ExecutionContext ctx, DyObject self)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);
        
        if (ctx.HasErrors)
            return Default();

        var set = new HashSet<DyObject>();
        set.UnionWith(seq);
        return new DySet(set);
    }

    private DyObject Distinct(ExecutionContext ctx, DyObject self, DyObject functor)
    {
        var seq = DyIterator.ToEnumerable(ctx, self);

        if (ctx.HasErrors)
            return Default();

        IEnumerable<DyObject> res;

        if (functor.NotNil())
            res = seq.Distinct(new EqualityComparer(ctx, functor));
        else
            res = seq.Distinct();

        return DyIterator.Create(res);
    }

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Method.ToArray => Func.Member(name, ToArray),
            Method.ToTuple => Func.Member(name, ToTuple),
            Method.ToDictionary => Func.Member(name, ToMap, -1, new Par("keySelector"), new Par("valueSelector", DyNil.Instance)),
            Method.Take => Func.Member(name, Take, -1, new Par("count")),
            Method.Skip => Func.Member(name, Skip, -1, new Par("count")),
            Method.First => Func.Member(name, First),
            Method.Last => Func.Member(name, Last),
            Method.Reverse => Func.Member(name, Reverse),
            Method.Slice => Func.Member(name, GetSlice, -1, new Par("index", DyInteger.Zero), new Par("endIndex", DyNil.Instance)),
            Method.ElementAt => Func.Member(name, ElementAt, -1, new Par("index")),
            Method.Sort => Func.Member(name, SortBy, -1, new Par("comparer", DyNil.Instance)),
            Method.Shuffle => Func.Member(name, Shuffle),
            Method.Count => Func.Member(name, CountBy, -1, new Par("predicate", DyNil.Instance)),
            Method.Map => Func.Member(name, Map, -1, new Par("converter")),
            Method.Filter => Func.Member(name, Filter, -1, new Par("predicate")),
            Method.TakeWhile => Func.Member(name, TakeWhile, -1, new Par("predicate")),
            Method.SkipWhile => Func.Member(name, SkipWhile, -1, new Par("predicate")),
            Method.Reduce => Func.Member(name, Reduce, -1, new Par("converter"), new Par("initial", DyInteger.Zero)),
            Method.Any => Func.Member(name, Any, -1, new Par("predicate")),
            Method.All => Func.Member(name, All, -1, new Par("predicate")),
            Method.ForEach => Func.Member(name, ForEach, -1, new Par("action")),
            Method.ToSet => Func.Member(name, ToSet),
            Method.Distinct => Func.Member(name, Distinct, -1, new Par("selector", DyNil.Instance)),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    private static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, bool exclusive)
    {
        var elem = from;
        var inf = to.TypeId == DyType.Nil;
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

        var up = ReferenceEquals(types[step.TypeId].Gt(ctx, step, DyInteger.Zero), DyBool.True);

        if (ctx.HasErrors)
            yield break;

        Func<ExecutionContext, DyObject, DyObject, DyObject> predicate = up && exclusive 
                ? types[from.TypeId].Lt : up ? types[from.TypeId].Lte : exclusive
                ? types[from.TypeId].Gt : types[from.TypeId].Gte;

        while (ReferenceEquals(predicate(ctx, elem, to), DyBool.True))
        {
            yield return elem;
            elem = types[elem.TypeId].Add(ctx, elem, step);

            if (ctx.HasErrors)
                yield break;
        }
    }

    private static DyObject MakeRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, DyObject exclusive) =>
        DyIterator.Create(GenerateRange(ctx, from, to, step, exclusive.IsTrue()));

    private static DyObject Empty(ExecutionContext ctx) => DyIterator.Create(Enumerable.Empty<DyObject>());

    private static IEnumerable<DyObject> Repeater(ExecutionContext ctx, DyObject val)
    {
        if (val.TypeId == DyType.Iterator)
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

    private static DyObject Repeat(ExecutionContext ctx, DyObject val) => DyIterator.Create(Repeater(ctx, val));

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Iterator or Method.Concat => Func.Static(name, Concat, 0, new Par("values", true)),
            Method.Range => Func.Static(name, MakeRange, -1, new Par("start", DyInteger.Zero), new Par("end", DyNil.Instance),
                new Par("step", DyInteger.One), new Par("exclusive", DyBool.False)),
            Method.Empty => Func.Static(name, Empty),
            Method.Repeat => Func.Static(name, Repeat, -1, new Par("value")),
            _ => base.InitializeStaticMember(name, ctx)
        };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Tuple => new DyTuple(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            DyType.Array => new DyArray(((DyIterator)self).ToEnumerable(ctx).ToArray()),
            DyType.Set => ToSet(ctx, self),
            _ => base.CastOp(self, targetType, ctx)
        };
}
