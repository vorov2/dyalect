using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dyalect
{
    public static class Extensions
    {
        public static T TakeOne<T>(this T[] arr, T defaultValue = default) => TakeAt(arr, 0, defaultValue);

        public static T TakeAt<T>(this T[] arr, int pos, T defaultValue = default)
        {
            if (arr == null || arr.Length <= pos)
                return defaultValue;

            return arr[pos];
        }

        public static string Format(this string self, params object[] args) => string.Format(self, args);
    }
}
