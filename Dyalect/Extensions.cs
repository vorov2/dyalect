using System;
namespace Dyalect;

public static class Extensions
{
    public static T? TakeOne<T>(this T[] arr, T? defaultValue = default) => TakeAt(arr, 0, defaultValue);

    public static T? TakeAt<T>(this T[] arr, int pos, T? defaultValue = default)
    {
        if (arr is null || arr.Length <= pos)
            return defaultValue;

        return arr[pos];
    }

    public static T[] Concat<T>(this T[] arr, T[] otherArr)
    {
        var newArr = new T[arr.Length + otherArr.Length];
        Array.Copy(arr, newArr, arr.Length);
        Array.Copy(otherArr, 0, newArr, arr.Length, otherArr.Length);
        return newArr;
    }

    public static string Format(this string self, params object[] args) => string.Format(self, args);

    public static T? GetAttribute<T>(this Type self) where T : Attribute =>
        Attribute.GetCustomAttribute(self, typeof(T)) as T;
}
