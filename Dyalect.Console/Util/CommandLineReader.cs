using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dyalect.Util
{
    public static class CommandLineReader
    {
        public static T Read<T>(string[] args) where T : new()
        {
            var options = Parse(args);
            var bag = ProcessOptionBag<T>(options);

            if (options.Count > 0 && options[0].key != null)
                throw new DyaException($"Unknown switch -{options[0].value}.");

            return bag;
        }

        private static T ProcessOptionBag<T>(List<(string key,string value)> options) where T : new()
        {
            var bag = new T();

            foreach (var pi in typeof(T).GetProperties())
            {
                var attr = Attribute.GetCustomAttribute(pi, typeof(BindingAttribute)) as BindingAttribute;

                if (attr == null)
                    continue;

                object value = null;
                string key = null;

                if (attr.Names == null || attr.Names.Length == 0 || (attr.Names.Length == 1 && attr.Names[0] == null))
                {
                    var opt = options.FirstOrDefault(o => o.key == null);
                    key = "<default>";

                    if (opt.value != null)
                    {
                        value = ConvertValue(opt, pi);
                        options.Remove(opt);
                    }
                }
                else
                {
                    var opts = options.Where(o => attr.Names.Contains(o.key)).ToArray();

                    if (opts.Length > 1)
                    {
                        if (!pi.PropertyType.IsArray)
                            throw KeyNotArray(opts[0].key);

                        foreach (var o in opts)
                            options.Remove(o);

                        value = CreateArray(pi, opts.Select(o => o.value).ToArray());
                        key = opts[0].key;
                    }
                    else if (opts.Length > 0)
                    {
                        options.Remove(opts[0]);
                        value = ConvertValue(opts[0], pi);
                        key = opts[0].key;
                    }
                }

                if (value != null)
                {
                    try
                    {
                        pi.SetValue(bag, value);
                    }
                    catch
                    {
                        throw InvalidKeyValue(key);
                    }
                }
            }

            return bag;
        }

        private static object ConvertValue((string key,string value) opt, PropertyInfo pi)
        {
            var v = opt.value;

            if (pi.PropertyType.IsArray)
                return CreateArray(pi, v);
            else if (pi.PropertyType == typeof(string))
                return v;
            else if (pi.PropertyType == typeof(int) && int.TryParse(v, out var i4))
                return i4;
            else if (pi.PropertyType == typeof(bool) && v == null || string.Equals(bool.TrueString, v, StringComparison.OrdinalIgnoreCase))
                return true;
            else if (pi.PropertyType.IsEnum && Enum.TryParse(pi.PropertyType, v, true, out var en))
                return en;
            else
                throw InvalidKeyValue(opt.key);
        }

        private static Array CreateArray(PropertyInfo pi, params object[] elements)
        {
            var arr = Array.CreateInstance(pi.PropertyType.GetElementType(), elements.Length);

            for (var i = 0; i < elements.Length; i++)
                arr.SetValue(elements[i], i);

            return arr;
        }

        private static Exception InvalidKeyValue(string key) => new DyaException($"Invalid value for the command line switch -{key}.");

        private static Exception KeyNotArray(string key) => new DyaException($"Command line switch -{key} doesn't support multiple values.");

        private static List<(string key,string value)> Parse(string[] args)
        {
            var options = new List<(string, string)>();
            string opt = null;
            string def = null;

            void AddOption(string key, string val) => options.Add((key, val?.Trim('"')));

            for (var i = 0; i < args.Length; i++)
            {
                var str = args[i].Trim(' ');
                var iswitch = str[0] == '-';

                if (!iswitch && opt == null)
                {
                    if (def != null)
                        throw new DyaException($"A default command line argument is already specifid: {def}");

                    AddOption(null, def = str);
                    continue;
                }

                if (str.Length > 0 && iswitch)
                {
                    if (opt != null)
                        AddOption(opt, null);
                    opt = str.Substring(1);
                    continue;
                }

                if (opt != null)
                {
                    AddOption(opt, str);
                    opt = null;
                    continue;
                }
            }

            if (opt != null)
                options.Add((opt, null));

            return options;
        }
    }
}
