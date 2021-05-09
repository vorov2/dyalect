using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIteratorTypeInfo : DyTypeInfo
    {
        public DyIteratorTypeInfo() : base(DyType.Iterator) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Iter;

        public override string TypeName => DyTypeNames.Iterator;

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

                if (ctx.Error != null)
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
            if (index.TypeId is not DyType.Integer)
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
            if (count.TypeId is not DyType.Integer)
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                i = 0;

            return DyIterator.Create(DyIterator.ToEnumerable(ctx, self).Take(i));
        }

        private DyObject Skip(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.TypeId is not DyType.Integer)
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

            if (funObj.TypeId is not DyType.Function && funObj.TypeId is not DyType.Nil)
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
            if (funObj.TypeId is not DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);
            
            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            return DyInteger.Get(seq.Count(dy => fun.Call(ctx, dy).GetBool()));
        }

        private DyObject Map(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId is not DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            return DyIterator.Create(seq.Select(dy => fun.Call(ctx, dy)));
        }

        private DyObject TakeWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId is not DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.TakeWhile(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(xs);
        }

        private DyObject SkipWhile(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId is not DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.SkipWhile(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(xs);
        }

        private DyObject Filter(ExecutionContext ctx, DyObject self, DyObject funObj)
        {
            if (funObj.TypeId is not DyType.Function)
                return ctx.InvalidType(funObj);

            var seq = DyIterator.ToEnumerable(ctx, self);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var fun = (DyFunction)funObj;
            var xs = seq.Where(o => fun.Call(ctx, o).GetBool());
            return DyIterator.Create(xs);
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "toArray" => Func.Member(name, ToArray),
                "toTuple" => Func.Member(name, ToTuple),
                "take" => Func.Member(name, Take, -1, new Par("count")),
                "skip" => Func.Member(name, Skip, -1, new Par("count")),
                "first" => Func.Member(name, First),
                "last" => Func.Member(name, Last),
                "reverse" => Func.Member(name, Reverse),
                "slice" => Func.Member(name, GetSlice, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance)),
                "element" => Func.Member(name, ElementAt, -1, new Par("at")),
                "sort" => Func.Member(name, SortBy, -1, new Par("by", DyNil.Instance)),
                "shuffle" => Func.Member(name, Shuffle),
                "count" => Func.Member(name, CountBy, -1, new Par("by")),
                "map" => Func.Member(name, Map, -1, new Par("transform")),
                "filter" => Func.Member(name, Filter, -1, new Par("include")),
                "takeWhile" => Func.Member(name, TakeWhile, -1, new Par("take")),
                "skipWhile" => Func.Member(name, SkipWhile, -1, new Par("skip")),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private static IEnumerable<DyObject> GenerateRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, bool exclusive)
        {
            var elem = from;
            var inf = to.TypeId == DyType.Nil;

            if (inf)
            {
                while (true)
                {
                    yield return elem;
                    elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, step);

                    if (ctx.HasErrors)
                        yield break;
                }
            }

            var up = ReferenceEquals(ctx.RuntimeContext.Types[step.TypeId].Gt(ctx, step, DyInteger.Zero),
                DyBool.True);

            if (ctx.HasErrors)
                yield break;

            var types = ctx.RuntimeContext.Types[from.TypeId];
            Func<ExecutionContext, DyObject, DyObject, DyObject> predicate =
                up && exclusive ? types.Lt : up ? types.Lte : exclusive ? types.Gt : types.Gte;

            while (ReferenceEquals(predicate(ctx, elem, to), DyBool.True))
            {
                yield return elem;
                elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, step);

                if (ctx.HasErrors)
                    yield break;
            }
        }

        private static DyObject MakeRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, DyObject exclusive) =>
            DyIterator.Create(GenerateRange(ctx, from, to, step, exclusive.GetBool()));

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

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Iterator" or "concat" => Func.Static(name, Concat, 0, new Par("values", true)),
                "range" => Func.Static(name, MakeRange, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance),
                    new Par("by", DyInteger.One), new Par("exclusive", DyBool.False)),
                "empty" => Func.Static(name, Empty),
                "repeat" => Func.Static(name, Repeat, -1, new Par("value")),
                "sort" => Func.Static(name, SortBy, -1, new Par("values"), new Par("by", DyNil.Instance)),
                "shuffle" => Func.Static(name, Shuffle, -1, new Par("values")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
