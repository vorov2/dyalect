using System;
using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static class TypeConverter
    {
        public static DyObject ConvertFrom(object obj, ExecutionContext ctx) => ConvertFrom(obj, obj?.GetType(), ctx);

        public static DyObject ConvertFrom<T>(T obj, ExecutionContext ctx) => ConvertFrom(obj, typeof(T), ctx);

        public static DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx)
        {
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
            }

            throw new InvalidCastException();
        }

        public static T ConvertTo<T>(DyObject obj, ExecutionContext ctx) => (T)ConvertTo(obj, typeof(T), ctx);

        public static object ConvertTo(DyObject obj, Type type, ExecutionContext ctx)
        {
            if (type == typeof(DyObject))
                return obj;
            else if (type == CliType.Object)
                return obj.ToObject();

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
            }

            throw new InvalidCastException();
        }
    }
}
