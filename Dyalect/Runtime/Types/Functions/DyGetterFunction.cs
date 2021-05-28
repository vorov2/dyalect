using System;

namespace Dyalect.Runtime.Types.Functions
{
    internal sealed class DyGetterFunction : DyFunction
    {
        private readonly int fieldIndex;

        public DyGetterFunction(int fieldIndex) : base(DyType.Function, Array.Empty<Debug.Par>(), -1) =>
            this.fieldIndex = fieldIndex;

        public override string FunctionName => throw new NotSupportedException();

        public override bool IsExternal => throw new NotSupportedException();

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
        {
            if (ctx.TypeStack.Peek() != arg.TypeId)
                return ctx.PrivateAccess();

            var tup = (DyTuple)arg;
            return tup.Values[fieldIndex];
        }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) => throw new NotSupportedException();

        internal override DyObject[] CreateLocals(ExecutionContext ctx) => throw new NotSupportedException();

        internal override bool Equals(DyFunction func) => throw new NotSupportedException();

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => throw new NotSupportedException();

        internal override DyObject InternalCall(ExecutionContext ctx) => throw new NotSupportedException();
    }
}
