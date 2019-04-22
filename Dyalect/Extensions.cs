using Dyalect.Runtime.Types;

namespace Dyalect
{
    public static class Extensions
    {
        public static T TakeOne<T>(this T[] arr, T defaultValue) => TakeAt(arr, 0, defaultValue);

        public static T TakeAt<T>(this T[] arr, int pos, T defaultValue)
        {
            if (arr == null || arr.Length <= pos)
                return defaultValue;

            return arr[pos];
        }
    }
}
