using System;
using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DySetterFunction : DyFunction
    {
        private readonly int fieldIndex;

        public DySetterFunction(int fieldIndex, string name) : base(DyType.Function, Array.Empty<Debug.Par>(), -1) =>
            (this.fieldIndex, FunctionName, Parameters) = (fieldIndex, name, new Par[] { new Par("value") });

        public override string FunctionName { get; }

        public override bool IsExternal => true;

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg) =>
            new DySetterFunction(fieldIndex, FunctionName) { Self = arg };

        internal override DyObject[] CreateLocals(ExecutionContext ctx) => new DyObject[Parameters.Length];

        internal override bool Equals(DyFunction func) => ReferenceEquals(Self, func);

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
         {
            if (fieldIndex is -1 || Self is null)
                return ctx.FieldNotFound();

            var amb = (DyAssemblage)Self;
            
            if (!amb.Keys[fieldIndex].Mutable)
                return ctx.FieldReadOnly();

            amb.Values[fieldIndex] = args[0];
            return DyNil.Instance;
        }

        internal override DyObject InternalCall(ExecutionContext ctx) => throw new NotSupportedException();
    }
}