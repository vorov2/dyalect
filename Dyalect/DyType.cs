using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect
{
    public static class DyType
    {
        public const int Nil = 1;
        public const int Integer = 2;
        public const int Float = 3;
        public const int Bool = 4;
        public const int Char = 5;
        public const int String = 6;
        public const int Function = 7;
        public const int Label = 8;
        public const int TypeInfo = 9;
        public const int Module = 10;
        public const int Array = 11;
        public const int Iterator = 12;
        public const int Tuple = 13;
        public const int Dictionary = 14;
        public const int Set = 15;
        public const int Error = 16;
        public const int Lazy = 17;
        public const int Object = 18;

        internal static List<DyTypeInfo> GetAll() =>
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
                new DySetTypeInfo(),
                new DyErrorTypeInfo(),
                new DyLazyTypeInfo(),
                new DyObjectTypeInfo()
            };

        public static int GetTypeCodeByName(string name) =>
            name switch
            {
                DyTypeNames.Nil => Nil,
                DyTypeNames.Integer => Integer,
                DyTypeNames.Float => Float,
                DyTypeNames.Bool => Bool,
                DyTypeNames.Char => Char,
                DyTypeNames.String => String,
                DyTypeNames.Function => Function,
                DyTypeNames.Label => Label,
                DyTypeNames.TypeInfo => TypeInfo,
                DyTypeNames.Tuple => Tuple,
                DyTypeNames.Module => Module,
                DyTypeNames.Array => Array,
                DyTypeNames.Iterator => Iterator,
                DyTypeNames.Dictionary => Dictionary,
                DyTypeNames.Set => Set,
                DyTypeNames.Error => Error,
                DyTypeNames.Lazy => Lazy,
                DyTypeNames.Object => Object,
                _ => default
            }; 

        internal static string GetTypeNameByCode(int code) =>
            code switch
            {
                Nil => DyTypeNames.Nil,
                Integer => DyTypeNames.Integer,
                Float => DyTypeNames.Float,
                Bool => DyTypeNames.Bool,
                Char => DyTypeNames.Char,
                String => DyTypeNames.String,
                Function => DyTypeNames.Function,
                Label => DyTypeNames.Label,
                TypeInfo => DyTypeNames.TypeInfo,
                Tuple => DyTypeNames.Tuple,
                Module => DyTypeNames.Module,
                Array => DyTypeNames.Array,
                Iterator => DyTypeNames.Iterator,
                Dictionary => DyTypeNames.Dictionary,
                Set => DyTypeNames.Set,
                Error => DyTypeNames.Error,
                Lazy => DyTypeNames.Lazy,
                Object => DyTypeNames.Object,
                _ => code.ToString(),
            };
    }

    internal static class DyTypeNames
    {
        public static string[] All =
            new[]
            {
                Nil,
                Integer,
                Float,
                Bool,
                Char,
                String,
                Function,
                Label,
                TypeInfo,
                Module,
                Array,
                Iterator,
                Tuple,
                Dictionary,
                Set,
                Error,
                Lazy,
                Object
            };

        public const string Nil = "Nil";
        public const string Integer = "Integer";
        public const string Float = "Float";
        public const string Bool = "Bool";
        public const string Char = "Char";
        public const string String = "String";
        public const string Function = "Function";
        public const string Label = "system:Label";
        public const string TypeInfo = "TypeInfo";
        public const string Module = "Module";
        public const string Array = "Array";
        public const string Iterator = "Iterator";
        public const string Tuple = "Tuple";
        public const string Dictionary = "Dictionary";
        public const string Set = "Set";
        public const string Error = "Error";
        public const string Lazy = "Lazy";
        public const string Object = "Object";
    }
}
