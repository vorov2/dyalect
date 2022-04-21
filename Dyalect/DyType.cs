using Dyalect.Runtime.Types;
namespace Dyalect;

public static class DyType
{
    public const int Nil = 1;
    public const int Object = 2;
    public const int Integer = 3;
    public const int Float = 4;
    public const int Bool = 5;
    public const int Char = 6;
    public const int String = 7;
    public const int Function = 8;
    public const int Label = 9;
    public const int TypeInfo = 10;
    public const int Module = 11;
    public const int Array = 12;
    public const int Iterator = 13;
    public const int Tuple = 14;
    public const int Dictionary = 15;
    public const int Set = 16;
    public const int Variant = 17;
    public const int Interop = 18;
    public const int Number = 19;
    public const int Collection = 20;
    public const int Comparable = 21;

    internal static FastList<DyTypeInfo> GetAll() =>
        new()
        {
            null!,
            new DyNilTypeInfo(),
            new DyObjectTypeInfo(),
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
            new DyVariantTypeInfo(),
            new DyInteropObjectTypeInfo(),
            new DyNumberTypeInfo(),
            new DyCollTypeInfo(),
            new DyComparableTypeInfo()
        };

    public static int GetTypeCodeByName(string name) =>
        name switch
        {
            DyTypeNames.Nil => Nil,
            DyTypeNames.Object => Object,
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
            DyTypeNames.Variant => Variant,
            DyTypeNames.Interop => Interop,
            DyTypeNames.Number => Number,
            DyTypeNames.Collection => Collection,
            DyTypeNames.Comparable => Comparable,
            _ => default
        }; 

    internal static string GetTypeNameByCode(int code) =>
        code switch
        {
            Nil => DyTypeNames.Nil,
            Object => DyTypeNames.Object,
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
            Variant => DyTypeNames.Variant,
            Interop => DyTypeNames.Interop,
            Number => DyTypeNames.Number,
            Collection => DyTypeNames.Collection,
            Comparable => DyTypeNames.Comparable,
            _ => code.ToString(),
        };
}

internal static class DyTypeNames
{
    public static string[] All =
        new[]
        {
            Nil,
            Object,
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
            Variant,
            Interop,
            Number,
            Collection,
            Comparable
        };

    public const string Nil = "Nil";
    public const string Object = "Object";
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
    public const string Variant = "Variant";
    public const string Interop = "Interop";
    public const string Number = "Number";
    public const string Collection = "Collection";
    public const string Comparable = "Comparable";
}
