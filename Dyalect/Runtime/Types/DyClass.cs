﻿using Dyalect.Compiler;
using System;

namespace Dyalect.Runtime.Types
{
    public sealed class DyClass : DyObject
    {
        internal Unit DeclaringUnit { get; }

        internal string Constructor { get; }

        internal DyObject Privates { get; }

        internal DyClass(int typeCode, string ctor, DyObject privates, Unit unit) : base(typeCode) =>
            (Constructor, Privates, DeclaringUnit) = (ctor, privates, unit);

        public override object ToObject() => this;

        public override string GetConstructor(ExecutionContext ctx) => Constructor;

        public override int GetHashCode() => HashCode.Combine(Constructor, Privates);

        public override bool Equals(DyObject? other) =>
            other is not null && TypeId == other.TypeId && other is DyClass t 
                && t.Constructor == Constructor && t.Privates.Equals(Privates);

        protected internal override bool HasItem(string name, ExecutionContext ctx) => Privates.HasItem(name, ctx);

        public override DyObject Clone() => new DyClass(TypeId, Constructor, Privates, DeclaringUnit);
    }
}
