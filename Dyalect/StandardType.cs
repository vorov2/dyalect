using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect
{
    internal static class StandardType
    {
        public static FastList<DyTypeInfo> GetAll() => 
            new FastList<DyTypeInfo>
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

        public static string[] TypeNames =
            new []
            {
                NilName,
                IntegerName,
                FloatName,
                BoolName,
                CharName,
                StringName,
                FunctionName,
                LabelName,
                TypeInfoName,
                ModuleName,
                ArrayName,
                IteratorName,
                TupleName
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

        public const string NilName = "Nil";
        public const string IntegerName = "Integer";
        public const string FloatName = "Float";
        public const string BoolName = "Bool";
        public const string CharName = "Char";
        public const string StringName = "String";
        public const string FunctionName = "Function";
        public const string LabelName = "Label";
        public const string TypeInfoName = "TypeInfo";
        public const string ModuleName = "Module";
        public const string ArrayName = "Array";
        public const string IteratorName = "Iterator";
        public const string TupleName = "Tuple";

        public static int GetTypeCodeByName(string name)
        {
            switch (name)
            {
                case NilName: return Nil;
                case IntegerName: return Integer;
                case FloatName: return Float;
                case BoolName: return Bool;
                case CharName: return Char;
                case StringName: return String;
                case FunctionName: return Function;
                case LabelName: return Label;
                case TypeInfoName: return TypeInfo;
                case TupleName: return Tuple;
                case ModuleName: return Module;
                case ArrayName: return Array;
                case IteratorName: return Iterator;
                default: return -1;
            }
        }
    }
}
