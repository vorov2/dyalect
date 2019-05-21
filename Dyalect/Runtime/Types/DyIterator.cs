using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIterator : DyForeignFunction
    {
        internal sealed class RangeEnumerator : IEnumerator<DyObject>
        {
            private readonly Func<DyObject> current;
            private readonly Func<bool> next;

            public RangeEnumerator(Func<DyObject> current, Func<bool> next)
            {
                this.current = current;
                this.next = next;
            }

            public DyObject Current => current();

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() => next();

            public void Reset() => throw new NotSupportedException();
        }

        internal sealed class MultiPartEnumerator : IEnumerator<DyObject>
        {
            private readonly DyObject[] iterators;
            private int nextIterator = 1;
            private IEnumerator<DyObject> current;
            private ExecutionContext ctx;

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
                if (!current.MoveNext())
                {
                    if (iterators.Length > nextIterator)
                    {
                        var it = RunIterator(ctx, iterators[nextIterator]);
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

            public void Reset() => throw new NotSupportedException();
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

        private readonly IEnumerator<DyObject> enumerator;

        public DyIterator(IEnumerator<DyObject> enumerator) : base(Builtins.Iterator, Statics.EmptyParameters, StandardType.Iterator, -1)
        {
            this.enumerator = enumerator;
        }

        internal static DyFunction CreateIterator(int unitId, int handle, FastList<DyObject[]> captures, DyObject[] locals)
        {
            var vars = new FastList<DyObject[]>(captures);
            vars.Add(locals);
            return new DyNativeIterator(unitId, handle, vars);
        }

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (enumerator.MoveNext())
                return enumerator.Current;
            return DyNil.Terminator;
        }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg) => new DyIterator(enumerator) { Self = arg };

        internal static DyFunction GetIterator(ExecutionContext ctx, DyObject val)
        {
            DyFunction iter;

            if (val.TypeId == StandardType.Iterator)
                iter = (DyFunction)val;
            else
            {
                iter = val.GetIterator(ctx) as DyFunction;

                if (ctx.HasErrors)
                    return null;

                if (iter == null)
                {
                    ctx.InvalidType(StandardType.IteratorName, val.TypeName(ctx));
                    return null;
                }

                iter = iter.Call0(ctx) as DyFunction;
            }

            return iter;
        }

        internal static IEnumerable<DyObject> RunIterator(ExecutionContext ctx, DyObject val)
        {
            if (val.TypeId == StandardType.Array)
                return ((DyArray)val).Values;
            else if (val.TypeId == StandardType.Tuple)
                return ((DyTuple)val).Values;
            else if (val.TypeId == StandardType.String)
                return (DyString)val;
            else
                return InternalRunIterator(ctx, val);
        }

        private static IEnumerable<DyObject> InternalRunIterator(ExecutionContext ctx, DyObject val)
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
                    break;
            }
        }
    }

    internal sealed class DyNativeIterator : DyNativeFunction
    {
        public override string FunctionName => "iter";

        public DyNativeIterator(int unitId, int funcId, FastList<DyObject[]> captures) : base(null, unitId, funcId, captures, StandardType.Iterator, -1)
        {

        }

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg) =>
            new DyNativeIterator(UnitId, FunctionId, Captures) { Self = arg };
    }

    internal sealed class DyIteratorTypeInfo : DyTypeInfo
    {
        public DyIteratorTypeInfo() : base(StandardType.Iterator, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => StandardType.BoolName;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => (DyString)$"{Builtins.Iterator}()";

        private DyObject ToArray(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var fn = (DyFunction)self;
            var arr = new List<DyObject>();
            DyObject res = null;

            while (!ReferenceEquals(res, DyNil.Terminator))
            {
                res = fn.Call0(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (!ReferenceEquals(res, DyNil.Terminator))
                    arr.Add(res);
            }

            return new DyArray(arr.ToArray());
        }

        private DyObject Concat(ExecutionContext ctx, DyObject tuple)
        {
            var values = ((DyTuple)tuple).Values;
            return new DyIterator(new DyIterator.MultiPartEnumerator(ctx, values));
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "toArray")
                return DyForeignFunction.Member(name, ToArray, -1, Statics.EmptyParameters);

            if (name == "concat")
                return DyForeignFunction.Static(name, Concat, -1, Statics.EmptyParameters);

            return null;
        }
    }
}
