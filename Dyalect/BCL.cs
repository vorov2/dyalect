using Dyalect.Runtime.Types;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect;

internal static class BCL
{
    public static readonly Type Boolean = typeof(bool);
    public static readonly Type SByte = typeof(sbyte);
    public static readonly Type Int16 = typeof(short);
    public static readonly Type Int32 = typeof(int);
    public static readonly Type Int64 = typeof(long);
    public static readonly Type Byte = typeof(byte);
    public static readonly Type UInt16 = typeof(ushort);
    public static readonly Type UInt32 = typeof(uint);
    public static readonly Type UInt64 = typeof(ulong);
    public static readonly Type Single = typeof(float);
    public static readonly Type Double = typeof(double);
    public static readonly Type Float = typeof(float);
    public static readonly Type Decimal = typeof(decimal);
    public static readonly Type Char = typeof(char);
    public static readonly Type String = typeof(string);
    public static readonly Type Object = typeof(object);
    public static readonly Type IEnumerable = typeof(IEnumerable);
    public static readonly Type IEnumerableObject = typeof(IEnumerable<object>);
    public static readonly Type IDictionary = typeof(IDictionary);
    public static readonly Type IDictionaryStringObject = typeof(IDictionary<string, object>);
    public static readonly Type Array = typeof(Array);
    public static readonly Type ArrayObject = typeof(object[]);
    public static readonly Type ListObject = typeof(List<object>);
    public static readonly Type Type = typeof(Type);
    public static readonly Type DyObject = typeof(DyObject);

    public static List<MethodInfo>? GetOverloadedMethod(this Type type, string name, BindingFlags flags)
    {
        List<MethodInfo>? xs = default;

        foreach (var mi in type.GetMethods(flags))
            if (mi.Name == name)
            {
                xs ??= new();
                xs.Add(mi);
            }

        return xs;
    }
}
