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

        public static Delegate CreateDelegate(this MethodInfo self, object instance = null) => 
            CreateDelegate(self, self.GetParameters(), instance);

        internal static Delegate CreateDelegate(this MethodInfo self, ParameterInfo[] pars, object instance = null)
        {
            var types = new Type[pars.Length + 1];

            for (var i = 0; i < pars.Length; i++)
                types[i] = pars[i].ParameterType;

            types[^1] = self.ReturnType;

            var func = types.Length switch
            {
                1  => typeof(Func<>),
                2  => typeof(Func<,>),
                3  => typeof(Func<,,>),
                4  => typeof(Func<,,,>),
                5  => typeof(Func<,,,,>),
                6  => typeof(Func<,,,,,>),
                7  => typeof(Func<,,,,,,>),
                8  => typeof(Func<,,,,,,,>),
                9  => typeof(Func<,,,,,,,,>),
                10 => typeof(Func<,,,,,,,,,>),
                11 => typeof(Func<,,,,,,,,,,>),
                12 => typeof(Func<,,,,,,,,,,,>),
                13 => typeof(Func<,,,,,,,,,,,,>),
                14 => typeof(Func<,,,,,,,,,,,,,>),
                15 => typeof(Func<,,,,,,,,,,,,,,>),
                16 => typeof(Func<,,,,,,,,,,,,,,,>),
                17 => typeof(Func<,,,,,,,,,,,,,,,,>),
                _ => throw new Exception("Method not supported. Too many arguments.")
            };

            var dt = func.MakeGenericType(types);
            return instance != null ? self.CreateDelegate(dt, instance) : self.CreateDelegate(dt, instance);
        }
    }
}
