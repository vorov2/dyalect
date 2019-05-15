using Dyalect.Runtime.Types;

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
    }
}
