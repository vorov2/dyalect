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

        public DyIteratorFunction(IEnumerable<DyObject> enumerable) : base(Builtins.Iterator, Array.Empty<Par>(), DyType.Function, -1) =>
            this.enumerable = enumerable;

        internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) =>
            (enumerator ??= enumerable.GetEnumerator()).MoveNext() ? enumerator.Current : DyNil.Terminator;

        internal override void Reset(ExecutionContext ctx) => enumerator = null;

        public override int GetHashCode() => enumerable.GetHashCode();

        internal override bool Equals(DyFunction func) => func is DyIteratorFunction f && f.enumerable.Equals(enumerator);

        public override DyObject Clone() => new DyIteratorFunction(enumerable);
    }
}
