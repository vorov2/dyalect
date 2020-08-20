using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Library.Json
{
    public static class DictionaryExtensions
    {
        public static object Object(this Dictionary<string, object> dict, string key)
        {
            dict.TryGetValue(key, out var res);
            return res;
        }

        public static string String(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res?.ToString();
        }

        public static int Int(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null && res is double d ? (int)d : 0;
        }

        public static double Double(this Dictionary<string, object> dict, string key)
        {
            var res = Object(dict, key);
            return res != null && res is double d ? d : 0d;
        }

        public static bool Bool(this Dictionary<string, object> dict, string key)
        {
            object res;
            dict.TryGetValue(key, out res);
            return res != null && res is bool b ? b
                : res != null && res.ToString().Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        public static T Enum<T>(this Dictionary<string, object> dict, string key) where T : struct
        {
            var str = dict.String(key);
            System.Enum.TryParse(str, true, out T res);
            return res;
        }

        public static char Char(this Dictionary<string, object> dict, string key)
        {
            dict.TryGetValue(key, out var res);
            return res != null ? res.ToString()[0] : '\0';
        }

        public static List<T> List<T>(this Dictionary<string, object> dict, string key)
        {
            if (!(Object(dict, key) is List<object> list))
                return null;

            if (typeof(T) == typeof(int))
                return list.OfType<double>().Cast<T>().ToList();
            else
                return list.OfType<T>().ToList();
        }
    }
}
