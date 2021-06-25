using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyClass : DyObject
    {
        private readonly bool privateCtor;

        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyTuple Privates { get; }

        internal DyClass(int typeCode, string ctor, bool privateCtor, DyTuple privates, Unit unit) : base(typeCode) =>
            (Constructor, Privates, this.privateCtor, DeclaringUnit) = (ctor, privates, privateCtor, unit);

        public override object ToObject() => this;

        public override void GetConstructor(ExecutionContext ctx, out string ctor, out bool priv) => 
            (ctor, priv) = (Constructor, privateCtor);

        public override int GetHashCode() => HashCode.Combine(Constructor, Privates);

        public override bool Equals(DyObject? other) =>
            other is not null && TypeId == other.TypeId && other is DyClass t 
                && t.Constructor == Constructor && t.Privates.Equals(Privates);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Privates.HasItem(name, ctx);

        public override DyObject Clone() => new DyClass(TypeId, Constructor, privateCtor, Privates, DeclaringUnit);
    }
}
