using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyIterator : DyForeignFunction
    {
        internal sealed class IterationException : Exception { }

        private sealed class MultiPartEnumerator : IEnumerator<DyObject>
        {
            private readonly DyObject[] iterators;
            private int nextIterator = 0;
            private IEnumerator<DyObject> current;
            private readonly ExecutionContext ctx;

            public MultiPartEnumerator(ExecutionContext ctx, params DyObject[] iterators)
            {
                this.iterators = iterators;
                this.ctx = ctx;
            }

            public DyObject Current => current.Current;

            object IEnumerator.Current => current.Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                if (current == null || !current.MoveNext())
                {
                    if (iterators.Length > nextIterator)
                    {
                        var it = Run(ctx, iterators[nextIterator]);
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

        private IEnumerable<DyObject> enumerable;
        private IEnumerator<DyObject> enumerator;

        public DyIterator(IEnumerable<DyObject> enumerable) : this(enumerable.GetEnumerator()) =>
            this.enumerable = enumerable;

        public DyIterator(IEnumerator<DyObject> enumerator) : base(Builtins.Iterator, Statics.EmptyParameters, DyType.Iterator, -1) =>
            this.enumerator = enumerator;

        internal void SetEnumerable(IEnumerable<DyObject> enu) =>
            (enumerable, enumerator) = (enu, enu.GetEnumerator());

        internal static DyFunction CreateIterator(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals)
        {
            var vars = new FastList<DyObject[]>(captures) { locals };
            return new DyNativeIterator(unitId, handle, vars);
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
            enumerator.MoveNext() ? enumerator.Current : DyNil.Terminator;

        internal override void Reset(ExecutionContext ctx)
        {
            if (enumerable is not null)
                enumerator = enumerable.GetEnumerator();
            else
                enumerator.Reset();
        }

        public override int GetHashCode() => enumerator.GetHashCode();

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg) => new DyIterator(enumerable) { Self = arg };

        internal static DyFunction GetIterator(ExecutionContext ctx, DyObject val)
        {
            DyFunction iter;

            if (val.TypeId == DyType.Iterator)
                iter = (DyFunction)val;
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
                iter = val.GetIterator(ctx) as DyFunction;

                if (ctx.HasErrors)
                    return null;

                if (iter is null)
                {
                    ctx.InvalidType(val);
                    return null;
                }

                iter = iter.Call0(ctx) as DyFunction;
            }

            return iter;
        }

        public static IEnumerable<DyObject> Run(ExecutionContext ctx, DyObject val)
        {
            if (val.TypeId == DyType.Array)
                return ((DyArray)val).Values;
            else if (val.TypeId == DyType.Tuple)
                return ((DyTuple)val).Values;
            else if (val.TypeId == DyType.String)
                return (DyString)val;
            else
                return InternalRun(ctx, val);
        }

        private static IEnumerable<DyObject> InternalRun(ExecutionContext ctx, DyObject val)
        {
            var iter = GetIterator(ctx, val);

            if (ctx.HasErrors)
                yield break;

            while (true)
            {
                var res = iter.Call(ctx);

                if (ctx.HasErrors)
                    yield break;

                if (!ReferenceEquals(res, DyNil.Terminator))
                    yield return res;
                else
                {
                    iter.Reset(ctx);
                    break;
                }
            }
        }

        internal override bool Equals(DyFunction func) => func is DyIterator m && m.enumerator.Equals(enumerator);
    }

    internal sealed class DyRange : DyIterator
    {
        public DyObject Step { get; set;  }

        public DyRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, DyObject exclusive)
            : base((IEnumerator<DyObject>)null) 
        {
            Step = step;
            SetEnumerable(GenerateIterator(ctx, from, to, exclusive.GetBool()));
        }

        private IEnumerable<DyObject> GenerateIterator(ExecutionContext ctx, DyObject from, DyObject to, bool exclusive)
        {
            var elem = from;
            var inf = to.TypeId == DyType.Nil;

            if (inf)
            {
                while (true)
                {
                    yield return elem;
                    elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, Step);

                    if (ctx.HasErrors)
                        yield break;
                }
            }

            var up = ctx.RuntimeContext.Types[Step.TypeId].Gt(ctx, Step, DyInteger.Zero) == DyBool.True;

            if (ctx.HasErrors)
                yield break;

            var types = ctx.RuntimeContext.Types[from.TypeId];
            Func<ExecutionContext, DyObject, DyObject, DyObject> predicate =
                up && exclusive ? types.Lt : up ? types.Lte
                : exclusive ? types.Gt : types.Gte;

            while (predicate(ctx, elem, to) == DyBool.True)
            {
                yield return elem;
                elem = ctx.RuntimeContext.Types[elem.TypeId].Add(ctx, elem, Step);

                if (ctx.HasErrors)
                    yield break;
            }
        }
    }

    internal sealed class DyNativeIterator : DyNativeFunction
    {
        public override string FunctionName => "iter";

        public DyNativeIterator(int unitId, int funcId, FastList<DyObject[]> captures) 
            : base(null, unitId, funcId, captures, DyType.Iterator, -1) { }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg) =>
            new DyNativeIterator(UnitId, FunctionId, Captures) { Self = arg };
    }

    internal sealed class DyIteratorTypeInfo : DyTypeInfo
    {
        public DyIteratorTypeInfo() : base(DyType.Iterator) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Len;

        public override string TypeName => DyTypeNames.Iterator;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) => GetCount(ctx, arg);

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var fn = (DyFunction)arg;
            fn.Reset(ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var seq = DyIterator.Run(ctx, arg);

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
            if (index.TypeId != DyType.Integer)
                return ctx.IndexInvalidType(index);

            var i = (int)index.GetInteger();

            try
            {
                if (i < 0)
                    return DyIterator.Run(ctx, self).Reverse().ElementAt(-i);
                else
                    return DyIterator.Run(ctx, self).ElementAt(i);
            }
            catch (IndexOutOfRangeException)
            {
                return ctx.IndexOutOfRange(index);
            }
        }

        private static List<DyObject> ConvertToArray(ExecutionContext ctx, DyObject self)
        {
            var fn = (DyFunction)self;
            fn.Reset(ctx);

            if (ctx.HasErrors)
                return null;

            var seq = DyIterator.Run(ctx, self);

            if (ctx.HasErrors)
                return null;

            return seq.ToList();
        }

        private DyObject ToArray(ExecutionContext ctx, DyObject self)
        {
            var res = ConvertToArray(ctx, self);
            return res == null ? DyNil.Instance : new DyArray(res.ToArray());
        }

        private DyObject ToTuple(ExecutionContext ctx, DyObject self)
        {
            var res = ConvertToArray(ctx, self);
            return res == null ? DyNil.Instance : new DyTuple(res.ToArray());
        }

        private static DyObject GetCount(ExecutionContext ctx, DyObject self)
        {
            var fn = (DyFunction)self;
            fn.Reset(ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            var count = 0;
            DyObject res = null;

            while (!ReferenceEquals(res, DyNil.Terminator))
            {
                res = fn.Call0(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (!ReferenceEquals(res, DyNil.Terminator))
                    count++;
            }

            return DyInteger.Get(count);
        }

        private DyObject Take(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                return ctx.InvalidValue(count);

            return new DyIterator(DyIterator.Run(ctx, self).Take(i));
        }

        private DyObject Skip(ExecutionContext ctx, DyObject self, DyObject count)
        {
            if (count.TypeId != DyType.Integer)
                return ctx.InvalidType(self);

            var i = (int)count.GetInteger();

            if (i < 0)
                return ctx.InvalidValue(count);

            return new DyIterator(DyIterator.Run(ctx, self).Skip(i));
        }

        private DyObject First(ExecutionContext ctx, DyObject self) =>
            DyIterator.Run(ctx, self).FirstOrDefault() ?? DyNil.Instance;

        private DyObject Last(ExecutionContext ctx, DyObject self) =>
            DyIterator.Run(ctx, self).LastOrDefault() ?? DyNil.Instance;

        private DyObject Concat(ExecutionContext ctx, DyObject tuple) =>
            new DyIterator(new DyIterator.MultiPartEnumerable(ctx, ((DyTuple)tuple).Values));

        private DyObject SetStep(ExecutionContext ctx, DyObject self, DyObject step)
        {
            if (self is DyRange r)
                r.Step = step;
            else
                return ctx.OperationNotSupported("by", self);

            return self;
        }

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject fromElem, DyObject toElem)
        {
            var seq =  DyIterator.Run(ctx, self);

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

                return new DyIterator(seq.Skip(beg)); 
            }
            
            var end = (int)toElem.GetInteger();

            if (end < 0)
                end = (count ?? seq.Count()) + end - 1;

            return new DyIterator(seq.Skip(beg).Take(end - beg + 1));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "toArray" => DyForeignFunction.Member(name, ToArray),
                "toTuple" => DyForeignFunction.Member(name, ToTuple),
                "take" => DyForeignFunction.Member(name, Take, -1, new Par("count")),
                "skip" => DyForeignFunction.Member(name, Skip, -1, new Par("count")),
                "first" => DyForeignFunction.Member(name, First),
                "last" => DyForeignFunction.Member(name, Last),
                "slice" => DyForeignFunction.Member(name, GetSlice, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance)),
                "by" => DyForeignFunction.Member(name, SetStep, -1, new Par("value")),
                //"element" => DyForeignFunction.Member(name, ElementAt, -1, new Par("at")),
                _ => null
            };

        private static DyObject MakeRange(ExecutionContext ctx, DyObject from, DyObject to, DyObject step, DyObject exclusive) => new DyRange(ctx, from, to, step, exclusive);

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Iterator")
                return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));
            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, 0, new Par("values", true));
            if (name == "range")
                return DyForeignFunction.Static(name, MakeRange, -1, new Par("from", DyInteger.Zero), new Par("to", DyNil.Instance), 
                    new Par("by", DyInteger.One), new Par("exclusive", DyBool.False));
            return null;
        }
    }
}
