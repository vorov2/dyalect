using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Library.Json
{
    using MAP = Dictionary<string, object>;

    public static class JsonObjectMapper
    {
        public static T Map<T>(MAP dict) where T : new()
        {
            var ret = new T();
            ProcessDictionary(ret, typeof(T), dict);
            return ret;
        }

        public static T Map<T>(List<object> list) where T : IList, new()
        {
            ProcessList(typeof(T), list, out var newList);
            return (T)newList;
        }

        private static bool ProcessDictionary(Type type, MAP dict, out object instance)
        {
            if (!TryCreateInstance(type, out instance))
                return false;

            ProcessDictionary(instance, type, dict);
            return true;
        }

        private static void ProcessDictionary(object instance, Type type, MAP dict)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var pi in props)
            {
                if (!pi.CanWrite || pi.Name == "Item"
                    || Attribute.IsDefined(pi, typeof(JsonIgnoreAttribute)))
                    continue;
                
                var attr = Attribute.GetCustomAttribute(pi, typeof(JsonElementAttribute));
                var name = pi.Name;

                if (attr != null)
                    name = attr.ToString();

                if (!ObtainDictionaryValue(name, pi.PropertyType, dict, out var value))
                    continue;

                pi.SetValue(instance, value);
            }
        }

        private static bool ProcessList(Type newListType, List<object> list, out IList newList)
        {
            if (!TryCreateInstance(newListType, out newList))
                return false;
            
            if (!TryGetElementType(newListType, out var elemType))
                return false;

            foreach (var o in list)
            {
                object vo = o;
                
                if (ConvertValue(elemType, ref vo))
                    newList.Add(vo);
            }

            return true;
        }


        private static bool ObtainDictionaryValue(string key, Type type, MAP dict, out object val)
        {
            if (!dict.TryGetValue(key, out val) || val == null)
                return false;

            if (!ConvertValue(type, ref val))
                return false;

            return true;
        }

        private static bool ConvertValue(Type type, ref object val)
        {
            var tc = Type.GetTypeCode(type);

            switch (tc)
            {
                case TypeCode.Int64:
                    if (val is double) val = (long)val;
                    else return false;
                    break;
                case TypeCode.Int32:
                    if (val is double) val = (int)val;
                    else return false;
                    break;
                case TypeCode.Int16:
                    if (val is double) val = (short)val;
                    else return false;
                    break;
                case TypeCode.UInt64:
                    if (val is double) val = (ulong)val;
                    else return false;
                    break;
                case TypeCode.UInt32:
                    if (val is double) val = (uint)val;
                    else return false;
                    break;
                case TypeCode.UInt16:
                    if (val is double) val = (ushort)val;
                    else return false;
                    break;
                case TypeCode.Single:
                    if (val is double) val = (float)val;
                    else return false;
                    break;
                case TypeCode.Double:
                    if (!(val is double))
                        return false;
                    break;
                case TypeCode.Byte:
                    if (val is double) val = (byte)val;
                    else return false;
                    break;
                case TypeCode.SByte:
                    if (val is double) val = (sbyte)val;
                    else return false;
                    break;
                case TypeCode.Decimal:
                    if (val is double) val = (decimal)val;
                    else return false;
                    break;
                case TypeCode.String:
                    val = val.ToString();
                    break;
                case TypeCode.Char:
                    var str = val.ToString();
                    if (str.Length > 0) val = str[0];
                    else return false;
                    break;
                case TypeCode.Boolean:
                    if (val is bool b) val = b;
                    else return false;
                    break;
                case TypeCode.DateTime:
                    if (val is string s)
                    {
                        if (!DateTime.TryParse(s, out var dt))
                            return false;
                        else val = dt;
                    }
                    break;
                default:
                    {
                        if (val is string ss && type.IsEnum)
                        {
                            val = Enum.Parse(type, ss, true);
                            if (val == null) return false;
                        }
                        else if (val is List<object> xs && typeof(IList).IsAssignableFrom(type))
                        {
                            if (ProcessList(type, xs, out var newList))
                                val = newList;
                            else return false;
                        }
                        else if (val is MAP map)
                        {
                            if (ProcessDictionary(type, map, out var newObj))
                                val = newObj;
                            else return false;
                        }
                    }
                    break;
            }

            return true;
        }

        private static bool TryGetElementType(Type type, out Type elementType)
        {
            var pi = type.GetProperty("Item");
            elementType = null;

            if (pi != null)
            {
                elementType = pi.PropertyType;
                return true;
            }
            else
            {
                var ti = type.GetTypeInfo();

                if (ti.GenericTypeArguments.Length > 0)
                {
                    elementType = ti.GenericTypeArguments[0];
                    return true;
                }
                else
                    return false;
            }
        }

        private static bool TryCreateInstance<T>(Type type, out T value)
        {
            value = default;
            
            try
            {
                var obj = Activator.CreateInstance(type);

                if (!(obj is T tobj))
                    return false;
                else
                {
                    value = tobj;
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}