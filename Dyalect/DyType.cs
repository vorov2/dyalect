using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;

namespace Dyalect
{
    internal static class DyType
    {
        public static List<DyTypeInfo> GetAll() =>
            new List<DyTypeInfo>
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
                new DyTupleTypeInfo()
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

        public static int GetTypeCodeByName(string name)
        {
            switch (name)
            {
                case DyTypeNames.Nil: return Nil;
                case DyTypeNames.Integer: return Integer;
                case DyTypeNames.Float: return Float;
                case DyTypeNames.Bool: return Bool;
                case DyTypeNames.Char: return Char;
                case DyTypeNames.String: return String;
                case DyTypeNames.Function: return Function;
                case DyTypeNames.Label: return Label;
                case DyTypeNames.TypeInfo: return TypeInfo;
                case DyTypeNames.Tuple: return Tuple;
                case DyTypeNames.Module: return Module;
                case DyTypeNames.Array: return Array;
                case DyTypeNames.Iterator: return Iterator;
                default: return -1;
            }
        }
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
                Tuple
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
    }
}
