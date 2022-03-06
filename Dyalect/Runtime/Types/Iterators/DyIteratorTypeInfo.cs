using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIteratorTypeInfo : DyTypeInfo
    {
        public DyIteratorTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Iterator) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Iterator;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => GetCount(ctx, arg);

        protected override DyObject ToStringOp(DyObject self, ExecutionContext ctx)
        {
            var fn = ((DyIterator)self).GetIteratorFunction();
            fn.Reset(ctx);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.String.Empty;

            var sb = new StringBuilder();
            sb.Append('{');
            var c = 0;

            foreach (var e in seq)
            {
                if (c > 0)
                    sb.Append(", ");
                var str = e.ToString(ctx);

                if (ctx.Error is not null)
                    return ctx.RuntimeContext.String.Empty;

                sb.Append(str.GetString());
                c++;
            }

            if (c == 1)
                sb.Append(", ");

            sb.Append('}');
            return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, sb.ToString());
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(index);

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

        private DyObject ToMap(ExecutionContext ctx, DyObject self, DyObject keySelectorObj, DyObject valueSelectorObj)
        {
            if (keySelectorObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(keySelectorObj);

            if (valueSelectorObj.DecType.TypeCode != DyTypeCode.Function && valueSelectorObj.DecType.TypeCode != DyTypeCode.Nil)
                return ctx.InvalidType(valueSelectorObj);

            var keySelector = (DyFunction)keySelectorObj;
            var valueSelector = valueSelectorObj as DyFunction;
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            try
            {
                var map = 
                    valueSelector is not null
                    ? seq.ToDictionary(dy => keySelector.Call(ctx, dy), dy => valueSelector.Call(ctx, dy))
                    : seq.ToDictionary(dy => keySelector.Call(ctx, dy));
                return new DyDictionary(ctx.RuntimeContext, map);
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
            return res is null ? ctx.RuntimeContext.Nil.Instance : new DyArray(ctx.RuntimeContext.Array, res.ToArray());
        }

        private DyObject ToTuple(ExecutionContext ctx, DyObject self)
        {
            var res = ConvertToArray(ctx, self);
            return res is null ? ctx.RuntimeContext.Nil.Instance : new DyTuple(ctx.RuntimeContext.Tuple, res.ToArray());
        }

        private static DyObject GetCount(ExecutionContext ctx, DyObject self)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);
            return ctx.HasErrors ? ctx.RuntimeContext.Nil.Instance : ctx.RuntimeContext.Integer.Get(seq.Count());
        }

        private DyObject ElementAt(ExecutionContext ctx, DyObject self, DyObject index) => GetOp(self, index, ctx);

        private DyObject Take(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                i = 0;

            return DyIterator.Create(ctx.RuntimeContext.Iterator,  DyIterator.ToEnumerable(ctx, self).Take(i));
        }

        private DyObject Skip(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                i = 0;

            return DyIterator.Create(ctx.RuntimeContext.Iterator,  DyIterator.ToEnumerable(ctx, self).Skip(i));
        }

        private DyObject First(ExecutionContext ctx, DyObject self) =>
            DyIterator.ToEnumerable(ctx, self).FirstOrDefault() ?? ctx.RuntimeContext.Nil.Instance;

        private DyObject Last(ExecutionContext ctx, DyObject self) =>
            DyIterator.ToEnumerable(ctx, self).LastOrDefault() ?? ctx.RuntimeContext.Nil.Instance;

        private DyObject Concat(ExecutionContext ctx, DyObject tuple) =>
            DyIterator.Create(ctx.RuntimeContext.Iterator,  new MultiPartEnumerable(ctx, ((DyTuple)tuple).Values));

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            if (fromElem.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(fromElem);

            if (toElem.DecType.TypeCode != DyTypeCode.Nil && toElem.DecType.TypeCode != DyTypeCode.Integer)
                return ctx.InvalidType(toElem);

            var beg = (int)fromElem.GetInteger();
            int? count = null;

            if (beg < 0)
                beg = (count ??= seq.Count()) + beg;

            if (ReferenceEquals(toElem, ctx.RuntimeContext.Nil.Instance))
            {
                if (beg == 0)
                    return self;

                return DyIterator.Create(ctx.RuntimeContext.Iterator,  seq.Skip(beg));
            }

            var end = (int)toElem.GetInteger();

            if (end < 0)
                end = (count ?? seq.Count()) + end - 1;

            return DyIterator.Create(ctx.RuntimeContext.Iterator,  seq.Skip(beg).Take(end - beg + 1));
        }

        private DyObject Reverse(ExecutionContext ctx, DyObject self) =>
            DyIterator.Create(ctx.RuntimeContext.Iterator,  DyIterator.ToEnumerable(ctx, self).Reverse());
        
        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            if (funObj.DecType.TypeCode != DyTypeCode.Function && funObj.DecType.TypeCode != DyTypeCode.Nil)
                return ctx.InvalidType(funObj);

            var comparer = new SortComparer(funObj as DyFunction, ctx);
            var sorted = seq.OrderBy(dy => dy, comparer);
            return DyIterator.Create(ctx.RuntimeContext.Iterator,  sorted);
        }

        private DyObject Shuffle(ExecutionContext ctx, DyObject self)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;
            
            var rnd = new Random();
            var sorted = seq.OrderBy(_ => rnd.Next());
            return DyIterator.Create(ctx.RuntimeContext.Iterator, sorted);
        }

        private DyObject CountBy(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);
            
            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            return ctx.RuntimeContext.Integer.Get(seq.Count(dy => fun.Call(ctx, dy).GetBool()));
        }

        private DyObject Map(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            return DyIterator.Create(ctx.RuntimeContext.Iterator, seq.Select(dy => fun.Call(ctx, dy)));
        }

        private DyObject TakeWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.TakeWhile(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(ctx.RuntimeContext.Iterator,  xs);
        }

        private DyObject SkipWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.SkipWhile(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(ctx.RuntimeContext.Iterator,  xs);
        }

        private DyObject Filter(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.Where(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(ctx.RuntimeContext.Iterator,  xs);
        }

        private DyObject Reduce(ExecutionContext ctx, DyObject self, DyObject initial, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.Aggregate(initial, (x,y) => fun.Call(ctx, x, y));

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;
            
            return res;
        }

        private DyObject Any(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.Any(o => fun.Call(ctx, o).GetBool());
            return res ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        private DyObject All(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.DecType.TypeCode != DyTypeCode.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return ctx.RuntimeContext.Nil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.All(o => fun.Call(ctx, o).GetBool());
            return res ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;
        }

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);
            return seq.Any(o => o.DecType.Eq(ctx, o, item).GetBool()) 
                ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False; ;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "toArray" => Func.Member(ctx, name, ToArray),
                "toTuple" => Func.Member(ctx, name, ToTuple),
                "toDictionary" => Func.Member(ctx, name, ToMap, -1, new Par("key"), new Par("value", StaticNil.Instance)),
                "take" => Func.Member(ctx, name, Take, -1, new Par("count")),
                "skip" => Func.Member(ctx, name, Skip, -1, new Par("count")),
                "first" => Func.Member(ctx, name, First),
                "last" => Func.Member(ctx, name, Last),
                "reverse" => Func.Member(ctx, name, Reverse),
                "slice" => Func.Member(ctx, name, GetSlice, -1, new Par("from", StaticInteger.Zero), new Par("to", StaticNil.Instance)),
                "element" => Func.Member(ctx, name, ElementAt, -1, new Par("at")),
                "sort" => Func.Member(ctx, name, SortBy, -1, new Par("by", StaticNil.Instance)),
                "shuffle" => Func.Member(ctx, name, Shuffle),
                "count" => Func.Member(ctx, name, CountBy, -1, new Par("by")),
                "map" => Func.Member(ctx, name, Map, -1, new Par("transform")),
                "filter" => Func.Member(ctx, name, Filter, -1, new Par("predicate")),
                "takeWhile" => Func.Member(ctx, name, TakeWhile, -1, new Par("predicate")),
                "skipWhile" => Func.Member(ctx, name, SkipWhile, -1, new Par("predicate")),
                "reduce" => Func.Member(ctx, name, Reduce, -1, new Par("init", StaticInteger.Zero), new Par("by")),
                "any" => Func.Member(ctx, name, Any, -1, new Par("predicate")),
                "all" => Func.Member(ctx, name, All, -1, new Par("predicate")),
                "contains" => Func.Member(ctx, name, Contains, -1, new Par("value")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, bool exclusive)
        {
            var elem = from;
            var inf = to.DecType.TypeCode == DyTypeCode.Nil;

            if (inf)
            {
                while (true)
                {
                    yield return elem;
                    elem = elem.DecType.Add(ctx, elem, step);

                    if (ctx.HasErrors)
                        yield break;
                }
            }

            var up = ReferenceEquals(step.DecType.Gt(ctx, step, ctx.RuntimeContext.Integer.Zero),
                ctx.RuntimeContext.Bool.True);

            if (ctx.HasErrors)
                yield break;

            Func<ExecutionContext, DyObject, DyObject, DyObject> predicate =
                up && exclusive ? from.DecType.Lt : up ? from.DecType.Lte : exclusive
                    ? from.DecType.Gt : from.DecType.Gte;

            while (ReferenceEquals(predicate(ctx, elem, to), ctx.RuntimeContext.Bool.True))
            {
                yield return elem;
                elem = elem.DecType.Add(ctx, elem, step);

                if (ctx.HasErrors)
                    yield break;
            }
        }

        private static DyObject MakeRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, DyObject exclusive) =>
            DyIterator.Create(ctx.RuntimeContext.Iterator,  GenerateRange(ctx, from, to, step, exclusive.GetBool()));

        private static DyObject Empty(ExecutionContext ctx) => DyIterator.Create(ctx.RuntimeContext.Iterator,  Enumerable.Empty<DyObject>());

        private static IEnumerable<DyObject> Repeater(ExecutionContext ctx, DyObject val)
        {
            if (val.DecType.TypeCode != DyTypeCode.Iterator)
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

        private static DyObject Repeat(ExecutionContext ctx, DyObject val) => DyIterator.Create(ctx.RuntimeContext.Iterator,  Repeater(ctx, val));

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Iterator" or "concat" => Func.Static(ctx, name, Concat, 0, new Par("values", true)),
                Builtins.Range => Func.Static(ctx, name, MakeRange, -1, new Par("from", StaticInteger.Zero), new Par("to", StaticNil.Instance),
                    new Par("by", StaticInteger.One), new Par("exclusive", StaticBool.False)),
                "empty" => Func.Static(ctx, name, Empty),
                "repeat" => Func.Static(ctx, name, Repeat, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
