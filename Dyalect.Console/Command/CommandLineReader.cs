using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dyalect.Command
{
    public static class CommandLineReader
    {
        public static T Read<T>(string[] args) where T : new()
        {
            var options = CommandLineParser.Parse(args);
            var bag = ProcessOptionBag<T>(options);

            if (options.Count > 0 && options[0].Key != null)
                throw new CommandException($"Unknown switch -{options[0].Key}.");

            return bag;
        }

        private static T ProcessOptionBag<T>(List<Option> options) where T : new()
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
                    var opt = options.FirstOrDefault(o => o.Key == null);
                    key = "<default>";

                    if (opt != null)
                    {
                        value = ConvertValue(opt, pi);
                        options.Remove(opt);
                    }
                }
                else
                {
                    var opts = options.Where(o => attr.Names.Contains(o.Key)).ToArray();

                    if (opts.Length > 1)
                    {
                        if (!pi.PropertyType.IsArray)
                            throw KeyNotArray(opts[0].Key);

                        foreach (var o in opts)
                            options.Remove(o);

                        value = CreateArray(pi, opts.Select(o => o.Value).ToArray());
                        key = opts[0].Key;
                    }
                    else if (opts.Length > 0)
                    {
                        options.Remove(opts[0]);
                        value = ConvertValue(opts[0], pi);
                        key = opts[0].Key;
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

        private static object ConvertValue(Option opt, PropertyInfo pi)
        {
            var v = opt.Value;

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
                throw InvalidKeyValue(opt.Key);
        }

        private static Array CreateArray(PropertyInfo pi, params object[] elements)
        {
            var arr = Array.CreateInstance(pi.PropertyType.GetElementType(), elements.Length);

            for (var i = 0; i < elements.Length; i++)
                arr.SetValue(elements[i], i);

            return arr;
        }

        private static Exception InvalidKeyValue(string key) => new CommandException($"Invalid value for the command line switch -{key}.");

        private static Exception KeyNotArray(string key) => new CommandException($"Command line switch -{key} doesn't support multiple values.");
    }
}
