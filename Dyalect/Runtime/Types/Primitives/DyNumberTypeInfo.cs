﻿using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyNumberTypeInfo : DyTypeInfo
    {
        public override string TypeName => DyTypeNames.Number;

        public override int ReflectedTypeId => DyType.Number;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        public DyNumberTypeInfo()
        {
            Closed = true;
            AddDefaultMixin2(Builtins.Add, "other");
            AddDefaultMixin2(Builtins.Sub, "other");
            AddDefaultMixin2(Builtins.Mul, "other");
            AddDefaultMixin2(Builtins.Div, "other");
            AddDefaultMixin2(Builtins.Rem, "other");
            AddDefaultMixin1(Builtins.Neg);
            AddDefaultMixin1(Builtins.Plus);
        }
    }
}