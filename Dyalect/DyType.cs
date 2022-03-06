﻿using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Dynamic;

namespace Dyalect
{
    public static class DyType
    {
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
        public const int Dictionary = 13;
        public const int Object = 14;
        public const int Set = 15;
        public const int Error = 16;

        public static DyTypeCode GetTypeCodeByName(string name) =>
            name switch
            {
                DyTypeNames.Nil => DyTypeCode.Nil,
                DyTypeNames.Integer => DyTypeCode.Integer,
                DyTypeNames.Float => DyTypeCode.Float,
                DyTypeNames.Bool => DyTypeCode.Bool,
                DyTypeNames.Char => DyTypeCode.Char,
                DyTypeNames.String => DyTypeCode.String,
                DyTypeNames.Function => DyTypeCode.Function,
                DyTypeNames.Label => DyTypeCode.Label,
                DyTypeNames.TypeInfo => DyTypeCode.TypeInfo,
                DyTypeNames.Tuple => DyTypeCode.Tuple,
                DyTypeNames.Module => DyTypeCode.Module,
                DyTypeNames.Array => DyTypeCode.Array,
                DyTypeNames.Iterator => DyTypeCode.Iterator,
                DyTypeNames.Dictionary => DyTypeCode.Dictionary,
                DyTypeNames.Object => DyTypeCode.Object,
                DyTypeNames.Set => DyTypeCode.Set,
                DyTypeNames.Error => DyTypeCode.Error,
                DyTypeNames.Class => DyTypeCode.Class,
                DyTypeNames.Foreign => DyTypeCode.Foreign,
                _ => default
            }; 

        public static DyTypeInfo? GetTypeInfoByCode(DyTypeCode code) =>
             code switch
             {
                 DyTypeCode.Nil => DyNil.Type,
                 DyTypeCode.Integer => DyInteger.Type,
                 DyTypeCode.Float => DyFloat.Type,
                 DyTypeCode.Bool => DyBool.Type,
                 DyTypeCode.Char => DyChar.Type,
                 DyTypeCode.String => DyString.Type,
                 DyTypeCode.Function => DyFunction.Type,
                 DyTypeCode.Label => DyLabel.Type,
                 DyTypeCode.TypeInfo => DyMetaTypeInfo.Instance,
                 DyTypeCode.Tuple => DyTuple.Type,
                 DyTypeCode.Module => DyModule.Type,
                 DyTypeCode.Array => DyArray.Type,
                 DyTypeCode.Iterator => DyIterator.Type,
                 DyTypeCode.Dictionary => DyDictionary.Type,
                 DyTypeCode.Set => DySet.Type,
                 DyTypeCode.Error => DyError.Type,
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
