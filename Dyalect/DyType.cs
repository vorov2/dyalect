using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect
{
    public static class DyType
    {
        internal static List<DyTypeInfo> GetAll() =>
            new()
            {
                new DyNilTypeInfo(),
                new DyIntegerTypeInfo(),
                new DyFloatTypeInfo(),
                new DyBoolTypeInfo(),
                new DyCharTypeInfo(),
                new DyStringTypeInfo(),
                new DyFunctionTypeInfo(),
                new DyLabelTypeInfo(),
                new DyTypeTypeInfo(),
                new DyModuleTypeInfo(),
                new DyArrayTypeInfo(),
                new DyIteratorTypeInfo(),
                new DyTupleTypeInfo(),
                new DyMapTypeInfo(),
                new DyCustomObjectTypeInfo(),
                new DyErrorTypeInfo()
            };

        public const int Nil = 0;
        public const int Integer = 1;
        public const int Float = 2;
        public const int Bool = 3;
        public const int Char = 4;
        public const int String = 5;
        public const int Function = 6;
        public const int Label = 7;
        public const int TypeInfo = 8;
        public const int Module = 9;
        public const int Array = 10;
        public const int Iterator = 11;
        public const int Tuple = 12;
        public const int Map = 13;
        public const int Object = 14;
        public const int Error = 15;

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
                DyTypeNames.Map => Map,
                DyTypeNames.Object => Object,
                DyTypeNames.Error => Error,
                _ => -1,
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
                Map => DyTypeNames.Map,
                Object => DyTypeNames.Object,
                Error => DyTypeNames.Error,
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
                Map,
                Object,
                Error
            };

        public const string Nil = "Nil";
        public const string Integer = "Integer";
        public const string Float = "Float";
        public const string Bool = "Bool";
        public const string Char = "Char";
        public const string String = "String";
        public const string Function = "Function";
        public const string Label = "Label";
        public const string TypeInfo = "TypeInfo";
        public const string Module = "Module";
        public const string Array = "Array";
        public const string Iterator = "Iterator";
        public const string Tuple = "Tuple";
        public const string Map = "Map";
        public const string Object = "Object";
        public const string Error = "Error";
    }
}
