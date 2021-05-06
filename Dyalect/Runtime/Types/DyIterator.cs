using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    #region Nested types
    //Generated when a currently traversed iterator was changed
    internal sealed class IterationException : Exception { }

    //Used to implement "concat" method when several iterators are combined in one
    internal sealed class MultiPartEnumerator : IEnumerator<DyObject>
    {
        private readonly DyObject[] iterators;
        private int nextIterator = 0;
        private IEnumerator<DyObject>? current;
        private readonly ExecutionContext ctx;

        public MultiPartEnumerator(ExecutionContext ctx, params DyObject[] iterators)
        {
            this.iterators = iterators;
            this.ctx = ctx;
        }

        public DyObject Current => current!.Current;

        object IEnumerator.Current => current!.Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if (current is null || !current.MoveNext())
            {
                if (iterators.Length > nextIterator)
                {
                    var it = DyIterator.ToEnumerable(ctx, iterators[nextIterator]);
                    nextIterator++;

                    if (ctx.HasErrors)
                        return false;

                    current = it.GetEnumerator();
                    return current.MoveNext();
                }
                else
                    return false;
            }
            else
                return true;
        }

        public void Reset()
        {
            current = null;
            nextIterator = 0;
        }
    }

    //Used to create MultiPartEnumerator
    internal sealed class MultiPartEnumerable : IEnumerable<DyObject>
    {
        private readonly DyObject[] iterators;
        private readonly ExecutionContext ctx;

        public MultiPartEnumerable(ExecutionContext ctx, params DyObject[] iterators)
        {
            this.iterators = iterators;
            this.ctx = ctx;
        }

        public IEnumerator<DyObject> GetEnumerator() => new MultiPartEnumerator(ctx, iterators);

        IEnumerator IEnumerable.GetEnumerator() => new MultiPartEnumerator(ctx, iterators);
    }
    #endregion

    internal sealed class DyIteratorFunction : DyForeignFunction
    {
        private readonly IEnumerable<DyObject> enumerable;
        private IEnumerator<DyObject>? enumerator;

        public DyIteratorFunction(IEnumerable<DyObject> enumerable) : base(Builtins.Iterator, Array.Empty<Par>(), DyType.Function, -1) =>
            this.enumerable = enumerable;

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
            (enumerator ??= enumerable.GetEnumerator()).MoveNext() ? enumerator.Current : DyNil.Terminator;

        internal override void Reset(ExecutionContext ctx) => enumerator = null;

        public override int GetHashCode() => enumerable.GetHashCode();

        internal override bool Equals(DyFunction func) => func is DyIteratorFunction f && f.enumerable.Equals(enumerator);

        public override DyObject Clone() => new DyIteratorFunction(enumerable);
    }

    internal sealed class DyNativeIteratorFunction : DyNativeFunction
    {
        public override string FunctionName => "iter";

        public DyNativeIteratorFunction(int unitId, int funcId, FastList<DyObject[]> captures)
            : base(null, unitId, funcId, captures, DyType.Function, -1) { }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
            new DyNativeIteratorFunction(UnitId, FunctionId, Captures) { Self = arg };
    }

    public abstract class DyIterator : DyObject
    {
        protected DyIterator() : base(DyType.Iterator) { }

        internal static DyIterator Create(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals) =>
            new DyNativeIterator(unitId, handle, captures, locals);

        public static DyIterator Create(IEnumerable<DyObject> seq) => new DyForeignIterator(seq);

        public abstract DyFunction GetIteratorFunction();

        internal static DyFunction? GetIterator(ExecutionContext ctx, DyObject val)
        {
            DyFunction? iter;

            if (val.TypeId == DyType.Iterator)
                iter = ((DyIterator)val).GetIteratorFunction();
            else if (val.TypeId == DyType.Function)
            {
                var obj = ((DyFunction)val).Call0(ctx);
                iter = obj as DyFunction;

                if (ctx.HasErrors)
                    return null;

                if (iter is null)
                {
                    ctx.InvalidType(obj);
                    return null;
                }
            }
            else
            {
                var obj = val.GetIterator(ctx) as DyIterator;

                if (ctx.HasErrors)
                    return null;

                iter = obj?.GetIteratorFunction();

                if (iter is null)
                {
                    ctx.InvalidType(val);
                    return null;
                }

                iter = iter.Call0(ctx) as DyFunction;
            }

            return iter;
        }

        public static IEnumerable<DyObject> ToEnumerable(ExecutionContext ctx, DyObject val)
        {
            if (val.TypeId == DyType.Array)
                return ((DyArray)val).Values;

            if (val.TypeId == DyType.Tuple)
                return ((DyTuple)val).Values;

            if (val.TypeId == DyType.String)
                return (DyString)val;

            return InternalRun(ctx, val);
        }

        private static IEnumerable<DyObject> InternalRun(ExecutionContext ctx, DyObject val)
        {
            var iter = GetIterator(ctx, val)!;

            if (ctx.HasErrors)
                yield break;

            while (true)
            {
                var res = iter.Call(ctx);

                if (ctx.HasErrors)
                {
                    iter.Reset(ctx);
                    yield break;
                }

                if (!ReferenceEquals(res, DyNil.Terminator))
                    yield return res;
                else
                {
                    iter.Reset(ctx);
                    yield break;
                }
            }
        }
    }

    internal sealed class DyForeignIterator : DyIterator
    {
        private readonly IEnumerable<DyObject> seq;

        public DyForeignIterator(IEnumerable<DyObject> seq) => this.seq = seq;

        public override DyFunction GetIteratorFunction() => new DyIteratorFunction(seq);

        public override object ToObject() => seq;

        public override int GetHashCode() => seq.GetHashCode();
    }

    internal sealed class DyNativeIterator : DyIterator
    {
        private readonly int unitId;
        private readonly int handle;
        private readonly FastList<DyObject[]> captures;

        public DyNativeIterator(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            (this.unitId, this.handle, this.captures) = (unitId, handle, vars);
        }

        public override DyFunction GetIteratorFunction() => new DyNativeIteratorFunction(unitId, handle, captures);

        public override object ToObject() => new MultiPartEnumerable(ExecutionContext.External, GetIteratorFunction());

        public override int GetHashCode() => HashCode.Combine(unitId, handle, captures);
    }

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

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "toArray" => DyForeignFunction.Member(name, ToArray),
                "toTuple" => DyForeignFunction.Member(name, ToTuple),
                "take" => DyForeignFunction.Member(name, Take, -1, new Par("count")),
                "skip" => DyForeignFunction.Member(name, Skip, -1, new Par("count")),
                "first" => DyForeignFunction.Member(name, First),
                "last" => DyForeignFunction.Member(name, Last),
                "slice" => DyForeignFunction.Member(name, GetSlice, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance)),
                "element" => DyForeignFunction.Member(name, ElementAt, -1, new Par("at")),
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
                while (true)
                {
                    var res = func.Call0(ctx);

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
                "Iterator" or "concat" => DyForeignFunction.Static(name, Concat, 0, new Par("values", true)),
                "range" => DyForeignFunction.Static(name, MakeRange, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance),
                    new Par("by", DyInteger.One), new Par("exclusive", DyBool.False)),
                "empty" => DyForeignFunction.Static(name, Empty),
                "repeat" => DyForeignFunction.Static(name, Repeat, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
