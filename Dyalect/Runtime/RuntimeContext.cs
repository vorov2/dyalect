using Dyalect.Compiler;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public sealed class RuntimeContext
    {
        internal RuntimeContext(UnitComposition composition)
        {
            (Composition, Units) = (composition, new DyObject[composition.Units.Count][]);
            TypeInfo.DecType = TypeInfo;
            Nil.DecType = TypeInfo;
            Integer.DecType = TypeInfo;
            Float.DecType = TypeInfo;
            Bool.DecType = TypeInfo;
            Char.DecType = TypeInfo;
            String.DecType = TypeInfo;
            Function.DecType = TypeInfo;
            Label.DecType = TypeInfo;
            TypeInfo.DecType = TypeInfo;
            Module.DecType = TypeInfo;
            Array.DecType = TypeInfo;
            Iterator.DecType = TypeInfo;
            Tuple.DecType = TypeInfo;
            Dictionary.DecType = TypeInfo;
            Wrapper.DecType = TypeInfo;
            Set.DecType = TypeInfo;
            Error.DecType = TypeInfo;
        }

        internal DyObject[][] Units { get; }

        internal readonly DyNilTypeInfo Nil = new(null!);
        internal readonly DyIntegerTypeInfo Integer = new(null!);
        internal readonly DyFloatTypeInfo Float = new(null!);
        internal readonly DyBoolTypeInfo Bool = new(null!);
        internal readonly DyCharTypeInfo Char = new(null!);
        internal readonly DyStringTypeInfo String = new(null!);
        internal readonly DyFunctionTypeInfo Function = new(null!);
        internal readonly DyLabelTypeInfo Label = new(null!);
        internal readonly DyMetaTypeInfo TypeInfo = new(null!);
        internal readonly DyModuleTypeInfo Module = new(null!);
        internal readonly DyArrayTypeInfo Array = new(null!);
        internal readonly DyIteratorTypeInfo Iterator = new(null!);
        internal readonly DyTupleTypeInfo Tuple = new(null!);
        internal readonly DyDictionaryTypeInfo Dictionary = new(null!);
        internal readonly DyWrapperTypeInfo Wrapper = new(null!);
        internal readonly DySetTypeInfo Set = new(null!);
        internal readonly DyErrorTypeInfo Error = new(null!);

        public UnitComposition Composition { get; }
    }
}
