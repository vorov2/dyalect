using System;

namespace Dyalect.Runtime.Types
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
            if (fieldIndex == -1)
                return ctx.FieldNotFound();

            if (ctx.TypeStack.Count == 0 || ctx.TypeStack.Peek() != arg.TypeId)
                return ctx.PrivateAccess();

            var tup = ((DyCustomType)arg).Privates;
            return tup.Values[fieldIndex].GetTaggedValue();
        }

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) => throw new NotSupportedException();

        internal override DyObject[] CreateLocals(ExecutionContext ctx) => throw new NotSupportedException();

        internal override bool Equals(DyFunction func) => throw new NotSupportedException();

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => throw new NotSupportedException();

        internal override DyObject InternalCall(ExecutionContext ctx) => throw new NotSupportedException();
    }
}
