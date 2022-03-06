using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Dynamic;

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
        public const int Object = 17;

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
                new DyErrorTypeInfo()
            };

        public static DyTypeCode GetTypeCodeByName(string name) =>
            name switch
            {
                DyTypeNames.Nil => DyType.Nil,
                DyTypeNames.Integer => DyType.Integer,
                DyTypeNames.Float => DyType.Float,
                DyTypeNames.Bool => DyType.Bool,
                DyTypeNames.Char => DyType.Char,
                DyTypeNames.String => DyType.String,
                DyTypeNames.Function => DyType.Function,
                DyTypeNames.Label => DyType.Label,
                DyTypeNames.TypeInfo => DyType.TypeInfo,
                DyTypeNames.Tuple => DyType.Tuple,
                DyTypeNames.Module => DyType.Module,
                DyTypeNames.Array => DyType.Array,
                DyTypeNames.Iterator => DyType.Iterator,
                DyTypeNames.Dictionary => DyType.Dictionary,
                DyTypeNames.Object => DyType.Object,
                DyTypeNames.Set => DyType.Set,
                DyTypeNames.Error => DyType.Error,
                DyTypeNames.Class => DyType.Class,
                DyTypeNames.Foreign => DyType.Foreign,
                _ => default
            }; 

        public static DyTypeInfo? GetTypeInfoByCode(RuntimeContext rtx, DyTypeCode code) =>
             code switch
             {
                 DyType.Nil => rtx.Nil,
                 DyType.Integer => rtx.Integer,
                 DyType.Float => rtx.Float,
                 DyType.Bool => rtx.Bool,
                 DyType.Char => rtx.Char,
                 DyType.String => rtx.String,
                 DyType.Function => rtx.Function,
                 DyType.Label => rtx.Label,
                 DyType.TypeInfo => rtx.TypeInfo,
                 DyType.Tuple => rtx.Tuple,
                 DyType.Module => rtx.Module,
                 DyType.Array => rtx.Array,
                 DyType.Iterator => rtx.Iterator,
                 DyType.Dictionary => rtx.Dictionary,
                 DyType.Set => rtx.Set,
                 DyType.Error => rtx.Error,
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
                Object => DyTypeNames.Object,
                Set => DyTypeNames.Set,
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
                Dictionary,
                Object,
                Set,
                Error
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
        public const string Object = "Object";
        public const string Set = "Set";
        public const string Error = "Error";
        public const string Class = "Class";
        public const string Foreign = "Foreign";
    }
}
