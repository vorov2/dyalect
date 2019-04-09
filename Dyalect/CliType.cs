using System;
using System.Collections;
using System.Collections.Generic;

namespace Dyalect
{
    internal static class CliType
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
        public static readonly Type Decimal = typeof(decimal);
        public static readonly Type Char = typeof(char);
        public static readonly Type String = typeof(string);
        public static readonly Type Object = typeof(object);
        public static readonly Type IEnumerable = typeof(IEnumerable);
        public static readonly Type IEnumerableObject = typeof(IEnumerable<object>);
        public static readonly Type IDictionary = typeof(IDictionary);
        public static readonly Type IDictionaryStringObject = typeof(IDictionary<string, object>);
        public static readonly Type ArrayObject = typeof(object[]);
        public static readonly Type ListObject = typeof(List<object>);
    }
}
