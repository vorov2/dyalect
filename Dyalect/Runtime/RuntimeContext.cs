using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime
{
    public sealed class RuntimeContext
    {
        internal RuntimeContext(UnitComposition composition)
        {
            (Composition, Units) = (composition, new DyObject[composition.Units.Count][]);
            Types[(int)DyTypeCode.TypeInfo].DecType = Types[(int)DyTypeCode.TypeInfo];
        }

        internal DyObject[][] Units { get; }

        internal List<DyTypeInfo> Types =
            new()
            {
                null!,
                new DyNilTypeInfo(),
                new DyIntegerTypeInfo(),
                new DyFloatTypeInfo(),
                new DyBoolTypeInfo(),
                new DyCharTypeInfo(),
                new DyStringTypeInfo(),
                new DyFunctionTypeInfo(),
                new DyLabelTypeInfo(),
                new DyMetaTypeInfo(),
                new DyModuleTypeInfo(),
                new DyArrayTypeInfo(),
                new DyIteratorTypeInfo(),
                new DyTupleTypeInfo(),
                new DyDictionaryTypeInfo(),
                new DyWrapperTypeInfo(),
                new DySetTypeInfo(),
                new DyErrorTypeInfo()
            };

        public UnitComposition Composition { get; }
    }
}
