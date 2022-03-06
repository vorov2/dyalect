using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyIteratorFunction : DyForeignFunction
    {
        private readonly IEnumerable<DyObject> enumerable;
        private IEnumerator<DyObject>? enumerator;

        public DyIteratorFunction(DyTypeInfo typeInfo, IEnumerable<DyObject> enumerable) : base(typeInfo, Builtins.Iterator, Array.Empty<Par>(), -1) =>
            this.enumerable = enumerable;

        internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) =>
            (enumerator ??= enumerable.GetEnumerator()).MoveNext() ? enumerator.Current : ctx.RuntimeContext.Nil.Terminator;

        internal override void Reset(ExecutionContext ctx) => enumerator = null;

        public override int GetHashCode() => enumerable.GetHashCode();

        internal override bool Equals(DyFunction func) => func is DyIteratorFunction f && f.enumerable.Equals(enumerator);

        public override DyObject Clone() => new DyIteratorFunction(DecType, enumerable);
    }
}
