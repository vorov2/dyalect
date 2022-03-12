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
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Iterator;

        public override int ReflectedTypeId => DyType.Iterator;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => GetCount(ctx, arg);

        protected override DyObject ToStringOp(DyObject self, ExecutionContext ctx)
        {
            var fn = ((DyIterator)self).GetIteratorFunction();
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
            if (keySelectorObj.TypeId != DyType.Function)
                return ctx.InvalidType(keySelectorObj);

            if (valueSelectorObj.TypeId != DyType.Function && valueSelectorObj.TypeId != DyType.Nil)
                return ctx.InvalidType(valueSelectorObj);

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
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                i = 0;

            return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Take(i));
        }

        private DyObject Skip(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(self);

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
            DyIterator.Create(new MultiPartEnumerable(ctx, ((DyTuple)tuple).Values));

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (fromElem.TypeId != DyType.Integer)
                return ctx.InvalidType(fromElem);

            if (toElem.TypeId != DyType.Nil && toElem.TypeId != DyType.Integer)
                return ctx.InvalidType(toElem);

            var beg = (int)fromElem.GetInteger();
            int? count = null;

            if (beg < 0)
                beg = (count ??= seq.Count()) + beg;

            if (ReferenceEquals(toElem, DyNil.Instance))
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
        
        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            if (funObj.TypeId != DyType.Function && funObj.TypeId != DyType.Nil)
                return ctx.InvalidType(funObj);

            var comparer = new SortComparer(funObj as DyFunction, ctx);
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

        private DyObject CountBy(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);
            
            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            return DyInteger.Get(seq.Count(dy => fun.Call(ctx, dy).GetBool(ctx)));
        }

        private DyObject Map(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            return DyIterator.Create(seq.Select(dy => fun.Call(ctx, dy)));
        }

        private DyObject TakeWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.TakeWhile(o => fun.Call(ctx, o).GetBool(ctx));
            return DyIterator.Create(xs);
        }

        private DyObject SkipWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.SkipWhile(o => fun.Call(ctx, o).GetBool(ctx));
            return DyIterator.Create(xs);
        }

        private DyObject Filter(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.Where(o => fun.Call(ctx, o).GetBool(ctx));
            return DyIterator.Create(xs);
        }

        private DyObject Reduce(ExecutionContext ctx, DyObject self, DyObject initial, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.Aggregate(initial, (x,y) => fun.Call(ctx, x, y));

            if (ctx.HasErrors)
                return DyNil.Instance;
            
            return res;
        }

        private DyObject Any(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.Any(o => fun.Call(ctx, o).GetBool(ctx));
            return res ? DyBool.True : DyBool.False;
        }

        private DyObject All(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId != DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var res = seq.All(o => fun.Call(ctx, o).GetBool(ctx));
            return res ? DyBool.True : DyBool.False;
        }

        private DyObject Contains(ExecutionContext ctx, DyObject self, DyObject item)
        {
            var seq = DyIterator.ToEnumerable(ctx, self);
            return seq.Any(o => ctx.RuntimeContext.Types[o.TypeId].Eq(ctx, o, item).GetBool(ctx)) 
                ? DyBool.True : DyBool.False; ;
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToArray" => Func.Member(name, ToArray),
                "ToTuple" => Func.Member(name, ToTuple),
                "ToDictionary" => Func.Member(name, ToMap, -1, new Par("key"), new Par("value", DyNil.Instance)),
                "Take" => Func.Member(name, Take, -1, new Par("count")),
                "Skip" => Func.Member(name, Skip, -1, new Par("count")),
                "First" => Func.Member(name, First),
                "Last" => Func.Member(name, Last),
                "Reverse" => Func.Member(name, Reverse),
                "Slice" => Func.Member(name, GetSlice, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance)),
                "Element" => Func.Member(name, ElementAt, -1, new Par("at")),
                "Sort" => Func.Member(name, SortBy, -1, new Par("by", DyNil.Instance)),
                "Shuffle" => Func.Member(name, Shuffle),
                "Count" => Func.Member(name, CountBy, -1, new Par("by")),
                "Map" => Func.Member(name, Map, -1, new Par("transform")),
                "Filter" => Func.Member(name, Filter, -1, new Par("predicate")),
                "TakeWhile" => Func.Member(name, TakeWhile, -1, new Par("predicate")),
                "SkipWhile" => Func.Member(name, SkipWhile, -1, new Par("predicate")),
                "Reduce" => Func.Member(name, Reduce, -1, new Par("init", DyInteger.Zero), new Par("by")),
                "Any" => Func.Member(name, Any, -1, new Par("predicate")),
                "All" => Func.Member(name, All, -1, new Par("predicate")),
                "Contains" => Func.Member(name, Contains, -1, new Par("value")),
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
            DyIterator.Create(GenerateRange(ctx, from, to, step, exclusive.GetBool(ctx)));

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
                "Iterator" or "Concat" => Func.Static(name, Concat, 0, new Par("values", true)),
                Builtins.Range => Func.Static(name, MakeRange, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance),
                    new Par("by", DyInteger.One), new Par("exclusive", DyBool.False)),
                "Empty" => Func.Static(name, Empty),
                "Repeat" => Func.Static(name, Repeat, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Tuple => new DyTuple(((DyIterator)self).ToEnumerable().ToArray()),
                DyType.Array => new DyArray(((DyIterator)self).ToEnumerable().ToArray()),
                _ => base.CastOp(self, targetType, ctx)
            };
    }
}
