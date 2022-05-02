using Dyalect.Runtime.Types;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime;

public static class TypeConverter
{
    public static DyObject ConvertFrom<T>(T obj) => ConvertFrom(obj, typeof(T));

    public static DyObject ConvertFrom(object? obj) => ConvertFrom(obj, obj?.GetType()!);

    internal static DyObject ConvertFrom(object? obj, Type type)
    {
        if (obj is null)
            return DyNil.Instance;

        if (obj is DyObject retval)
            return retval;

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean: return (bool)obj ? True : False;
            case TypeCode.Byte: return new DyInteger((byte)obj);
            case TypeCode.Int16: return new DyInteger((short)obj);
            case TypeCode.Int32: return new DyInteger((int)obj);
            case TypeCode.Int64: return new DyInteger((long)obj);
            case TypeCode.SByte: return new DyInteger((sbyte)obj);
            case TypeCode.UInt16: return new DyInteger((ushort)obj);
            case TypeCode.UInt32: return new DyInteger((uint)obj);
            case TypeCode.UInt64: return new DyInteger((long)(ulong)obj);
            case TypeCode.String:
            case TypeCode.Char: return DyString.Get(obj.ToString());
            case TypeCode.Single: return new DyFloat((float)obj);
            case TypeCode.Double: return new DyFloat((double)obj);
            case TypeCode.Decimal: return new DyFloat((double)(decimal)obj);
            case TypeCode.Empty: return DyNil.Instance;
            default:
                if (obj is IDictionary map)
                {
                    var dict = new Dictionary<DyObject, DyObject>();
                    foreach (DictionaryEntry kv in map)
                        dict[ConvertFrom(kv.Key)] =
                            kv.Value is null ? DyNil.Instance : ConvertFrom(kv.Value);
                    return new DyDictionary(dict);
                }
                else if (obj is IEnumerable seq)
                    return new DyArray(seq.OfType<object>().Select(o => ConvertFrom(o)).ToArray());
                else if (type.IsArray)
                {
                    var arr = (Array)obj;
                    var newArr = new DyObject[arr.Length];
                    for (var i = 0; i < arr.Length; i++)
                        newArr[i] = ConvertFrom(arr.GetValue(i));
                    return new DyArray(newArr);
                }
                else if (BCL.Type.IsAssignableFrom(type))
                    return new DyInteropObject((Type)obj);
                else
                    return new DyInteropObject(type, obj);
        }
    }

    public static T? ConvertTo<T>(ExecutionContext ctx, DyObject obj) => (T?)ConvertTo(ctx, obj, typeof(T));

    public static object? ConvertTo(ExecutionContext ctx, DyObject obj, Type type)
    {
        if (!TryConvert(obj, type, out var result))
        {
            ctx.InvalidCast(obj.TypeName, type.FullName ?? type.Name);
            return null;
        }

        return result; 
    }

    public static bool TryConvert(DyObject obj, Type type, out object? result)
    {
        result = default;
        long i8; double r8; string str;

        if (obj.TypeId == Dy.Interop)
        {
            var interop = (DyInteropObject)obj;

            if (BCL.Type.IsAssignableFrom(interop.Type)) //We have a type info here
            {
                if (BCL.Type.IsAssignableFrom(type)) //Type info is what we need
                {
                    result = interop.Object;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                try
                {
                    result = Convert.ChangeType(interop.Object, type);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        if (type == BCL.DyObject)
        {
            result = obj;
            return true;
        }
        else if (type == BCL.Object)
        {
            result = obj.ToObject();
            return true;
        }
        else if (BCL.DyObject.IsAssignableFrom(type))
        {
            result = Convert.ChangeType(obj, type);
            return true;
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Boolean:
                result = obj.IsTrue();
                return true;
            case TypeCode.Byte:
                if (TryGetInteger(obj, out i8))
                {
                    result = (byte)i8;
                    return true;
                }
                return false;
            case TypeCode.Int16:
                if (TryGetInteger(obj, out i8))
                {
                    result = (short)i8;
                    return true;
                }
                return false;
            case TypeCode.Int32:
                if (TryGetInteger(obj, out i8))
                {
                    result = (int)i8;
                    return true;
                }
                return false;
            case TypeCode.Int64:
                if (TryGetInteger(obj, out i8))
                {
                    result = i8;
                    return true;
                }
                return false;
            case TypeCode.SByte:
                if (TryGetInteger(obj, out i8))
                {
                    result = (sbyte)i8;
                    return true;
                }
                return false;
            case TypeCode.UInt16:
                if (TryGetInteger(obj, out i8))
                {
                    result = (ushort)i8;
                    return true;
                }
                return false;
            case TypeCode.UInt32:
                if (TryGetInteger(obj, out i8))
                {
                    result = (uint)i8;
                    return true;
                }
                return false;
            case TypeCode.UInt64:
                if (TryGetInteger(obj, out i8))
                {
                    result = (ulong)i8;
                    return true;
                }
                return false;
            case TypeCode.String:
                if (TryGetString(obj, out str))
                {
                    result = str;
                    return true;
                }
                return false;
            case TypeCode.Char:
                if (TryGetString(obj, out str))
                {
                    result = string.IsNullOrEmpty(str) ? '\0' : str[0];
                    return true;
                }
                return false;
            case TypeCode.Single:
                if (TryGetFloat(obj, out r8))
                {
                    result = (float)r8;
                    return true;
                }
                return false;
            case TypeCode.Double:
                if (TryGetFloat(obj, out r8))
                {
                    result = r8;
                    return true;
                }
                return false;
            case TypeCode.Decimal:
                if (TryGetFloat(obj, out r8))
                {
                    result = (decimal)r8;
                    return true;
                }
                return false;
            case TypeCode.Empty:
                result = null;
                return true;
            default:
                if (obj is DyDictionary map)
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var genargs = type.GetGenericArguments();
                        var (keyType, valueType) = (genargs[0], genargs[1]);
                        var targetType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                        var ret = (IDictionary)Activator.CreateInstance(targetType)!;
                        foreach (var kv in map.Dictionary) 
                            ret[kv.Key.ToObject()] = kv.Value.ToObject();
                        result = ret;
                        return true;

                    }
                    else if (type == typeof(Hashtable))
                    {
                        var ret = new Hashtable();
                        foreach (var kv in map.Dictionary)
                            ret[kv.Key.ToObject()] = kv.Value.ToObject();
                        result = ret;
                        return true;
                    }
                }
                else if (obj is DyEnumerable enu)
                {
                    if (type.IsArray)
                    {
                        var et = type.GetElementType();
                        if (!TryCreateTypedArray(enu.ToArray(), et!, out var res))
                            return false;
                        result = res;
                        return true;
                    }

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var et = type.GetGenericArguments()[0];
                        if (!TryCreateTypedArray(enu.ToArray(), et!, out var res))
                            return false;
                        result = res;
                        return true;
                    }

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var et = type.GetGenericArguments()[0];
                        if (!TryCreateTypedArray(enu.ToArray(), et!, out var arr))
                            return false;
                        var targetType = typeof(List<>).MakeGenericType(et!);
                        result = Activator.CreateInstance(targetType, arr);
                        return true;
                    }

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
                    {
                        var et = type.GetGenericArguments()[0];
                        if (!TryCreateTypedArray(enu.ToArray(), et!, out var arr))
                            return false;
                        var targetType = typeof(HashSet<>).MakeGenericType(et!);
                        result = Activator.CreateInstance(targetType, arr);
                        return true;
                    }
                }
                break;
        }

        return false;
    }

    private static bool TryGetFloat(DyObject obj, out double result)
    {
        if (obj is DyFloat f)
        {
            result = f.Value;
            return true;
        }
        else if (obj is DyInteger i)
        {
            result = i.Value;
            return true;
        }
        else if (obj is DyChar c)
        {
            result = c.Value;
            return true;
        }

        result = default;
        return false;
    }

    private static bool TryGetInteger(DyObject obj, out long result)
    {
        if (obj is DyInteger i)
        {
            result = i.Value;
            return true;
        }
        else if (obj is DyFloat f)
        {
            result = (long)f.Value;
            return true;
        }
        else if (obj is DyChar c)
        {
            result = c.Value;
            return true;
        }

        result = default;
        return false;
    }

    private static bool TryGetString(DyObject obj, out string result)
    {
        if (obj is DyString s)
        {
            result = s.Value;
            return true;
        }
        else if (obj is DyChar c)
        {
            result = c.Value.ToString();
            return true;
        }

        result = string.Empty;
        return false;
    }

    public static bool TryCreateTypedArray(DyObject[] arr, Type type, out Array? result)
    {
        var xs = Array.CreateInstance(type, arr.Length);
        result = default;

        for (var i = 0; i < arr.Length; i++)
        {
            if (!TryConvert(arr[i], type, out var obj))
                return false;

            xs.SetValue(obj, i);
        }

        result = xs;
        return true;
    }
}
