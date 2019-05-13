using Dyalect.Compiler;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIterator : DyForeignFunction
    {
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

        internal static DyFunction GetIterator(DyObject val, ExecutionContext ctx)
        {
            DyFunction iter;

            if (val.TypeId == StandardType.Iterator)
                iter = val as DyFunction;
            else
            {
                iter = val.GetIterator(ctx) as DyFunction;

                if (ctx.HasErrors || iter == null)
                    return null;

                iter = iter.Call0(ctx) as DyFunction;
            }

            return iter;
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
        public DyIteratorTypeInfo() : base(StandardType.Bool, false)
        {

        }

        public override string TypeName => StandardType.BoolName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => $"{Builtins.Iterator}()";

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

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == "toArray")
                return DyForeignFunction.Member(name, ToArray, -1, Statics.EmptyParameters);

            return null;
        }
    }
}
