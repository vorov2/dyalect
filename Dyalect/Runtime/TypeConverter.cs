using System;
using System.Net.WebSockets;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static class TypeConverter
    {
        public static DyObject ConvertFrom<T>(T obj) => ConvertFrom(obj, typeof(T), null);

        public static DyObject ConvertFrom(object obj) => ConvertFrom(obj, obj?.GetType(), null);

        internal static DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx)
        {
            if (obj is null)
                return DyNil.Instance;
            
            if (obj is DyObject retval)
                return retval;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return (bool)obj ? DyBool.True : DyBool.False;
                case TypeCode.Byte: return new DyInteger((byte)obj);
                case TypeCode.Int16: return new DyInteger((short)obj);
                case TypeCode.Int32: return new DyInteger((int)obj);
                case TypeCode.Int64: return new DyInteger((long)obj);
                case TypeCode.SByte: return new DyInteger((sbyte)obj);
                case TypeCode.UInt16: return new DyInteger((ushort)obj);
                case TypeCode.UInt32: return new DyInteger((uint)obj);
                case TypeCode.UInt64: return new DyInteger((long)(ulong)obj);
                case TypeCode.String:
                case TypeCode.Char: return new DyString(obj.ToString());
                case TypeCode.Single: return new DyFloat((float)obj);
                case TypeCode.Double: return new DyFloat((double)obj);
                case TypeCode.Decimal: return new DyFloat((double)(decimal)obj);
                case TypeCode.Empty: return DyNil.Instance;
                default:
                    if (type.IsArray)
                    {
                        var arr = (Array)obj;
                        var newArr = new DyObject[arr.Length];
                        
                        for (var i = 0; i < arr.Length; i++)
                            newArr[i] = ConvertFrom(arr.GetValue(i), arr.GetValue(i)?.GetType(), ctx);

                        return new DyArray(newArr);
                    }
                    break;
            }

            throw new InvalidCastException();
        }

        public static T ConvertTo<T>(DyObject obj, ExecutionContext ctx) => (T)ConvertTo(obj, typeof(T), ctx);

        public static object ConvertTo(DyObject obj, Type type, ExecutionContext ctx)
        {
            if (type == Dyalect.Types.DyObject)
                return obj;
            else if (type == Dyalect.Types.Object)
                return obj.ToObject();
            else if (Dyalect.Types.DyObject.IsAssignableFrom(type))
                return Convert.ChangeType(obj, type);

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return obj.GetBool();
                case TypeCode.Byte: return (byte)obj.GetInteger();
                case TypeCode.Int16: return (short)obj.GetInteger();
                case TypeCode.Int32: return (int)obj.GetInteger();
                case TypeCode.Int64: return obj.GetInteger();
                case TypeCode.SByte: return (sbyte)obj.GetInteger();
                case TypeCode.UInt16: return (ushort)obj.GetInteger();
                case TypeCode.UInt32: return (uint)obj.GetInteger();
                case TypeCode.UInt64: return (ulong)obj.GetInteger();
                case TypeCode.String: return obj.ToString(ctx).GetString();
                case TypeCode.Char:
                    {
                        var str = obj.ToString(ctx).GetString();
                        return string.IsNullOrEmpty(str) ? '\0' : str[0];
                    }
                case TypeCode.Single: return (float)obj.GetFloat();
                case TypeCode.Double: return obj.GetFloat();
                case TypeCode.Decimal: return (decimal)obj.GetFloat();
                case TypeCode.Empty: return null;
                default:
                    if (type.IsArray && obj is DyCollection coll)
                    {
                        var et = type.GetElementType();
                        var arr = Array.CreateInstance(et, coll.Count);

                        for (var i = 0; i < coll.Count; i++)
                        {
                            var c = coll[i];
                            arr.SetValue(ConvertTo(c, et, ctx), i);
                        }

                        return arr;
                    }
                    break;
            }

            throw new InvalidCastException();
        }
    }
}
