﻿using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignObject : DyObject
    {
        public override int TypeId => TypeInfo.ReflectedTypeCode;

        public DyTypeInfo TypeInfo { get; }

        public string? Constructor { get; }

        protected DyForeignObject(DyForeignTypeInfo typeInfo) : this(typeInfo, null) { }

        protected DyForeignObject(DyForeignTypeInfo typeInfo, string? ctor) : base(-1) =>
            (TypeInfo, Constructor) = (typeInfo, ctor);

        public override string GetConstructor(ExecutionContext ctx) => Constructor ?? "";

        public override int GetHashCode() => HashCode.Combine(TypeId, Constructor);
    }
}
