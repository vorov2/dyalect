using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyClass : DyObject
    {
        private readonly bool privateCtor;

        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyTuple Fields { get; }

        internal DyClass(DyTypeInfo type, string ctor, bool privateCtor, DyTuple privates, Unit unit) : base(type) =>
            (Constructor, Fields, this.privateCtor, DeclaringUnit) = (ctor, privates, privateCtor, unit);

        public override object ToObject() => this;

        public override void GetConstructor(ExecutionContext ctx, out string ctor, out bool priv) => 
            (ctor, priv) = (Constructor, privateCtor);

        public override int GetHashCode() => HashCode.Combine(Constructor, Fields);

        public override bool Equals(DyObject? other) =>
            other is not null && Is(DecType, other) && other is DyClass t 
                && t.Constructor == Constructor && t.Fields.Equals(Fields);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Fields.HasItem(name, ctx);

        protected internal override DyObject Unbox() => Fields;

        public override DyObject Clone() => new DyClass(DecType, Constructor, privateCtor, Fields, DeclaringUnit);
    }
}
