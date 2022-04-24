using Dyalect.Runtime.Types;
namespace Dyalect;

public static partial class DyType
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
    public const int Bounded = 22;

    internal static FastList<DyTypeInfo> GetAll()
    {
        var xs = new FastList<DyTypeInfo>();
        GetAllGenerated(xs);
        return xs;
    }
    static partial void GetAllGenerated(FastList<DyTypeInfo> types);

    public static int GetTypeCodeByName(string name)
    {
        int code = 0;
        GetTypeCodeByNameGenerated(name, ref code);
        return code;
    }
    static partial void GetTypeCodeByNameGenerated(string name, ref int code);

    public static string GetTypeNameByCode(int code)
    {
        string name = null!;
        GetTypeNameByCodeGenerated(code, ref name);
        return name;
    }
    static partial void GetTypeNameByCodeGenerated(int code, ref string name);
}
