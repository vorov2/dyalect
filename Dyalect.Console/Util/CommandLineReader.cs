using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Util
{
    public static class CommandLineReader
    {
        class Value
        {
            public bool IsDefault;
        }

        class StringValue : Value
        {
            public string Value;
        }

        class ArrayValue : Value
        {
            public ArrayValue(params string[] args)
            {
                if (args != null)
                    Values.AddRange(args);
                else
                    Values.Add(null);
            }
            public List<string> Values = new List<string>();
        }

        public static T Read<T>(string[] args, IDictionary<string, object> config) where T : new()
        {
            var options = Parse(args, config);

            var bag = ProcessOptionBag<T>(options);

            if (options.Count > 0 && options.First().Key != "$default")
                throw new DyaException($"Unknown switch -{options.First().Key}.");

            return bag;
        }

        private static T ProcessOptionBag<T>(Dictionary<string, Value> options) where T : new()
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
                    key = "<default>";

                    if (options.TryGetValue("$default", out var opt) && opt is StringValue str)
                    {
                        value = ConvertValue(key, str.Value, pi.PropertyType);
                        options.Remove("$default");
                    }
                }
                else
                {
                    ArrayValue arr = null;

                    foreach (var n in attr.Names)
                    {
                        if (options.TryGetValue(n, out var opt))
                        {
                            if (opt is ArrayValue carr)
                            {
                                if (arr == null)
                                    arr = carr;
                                else
                                    arr.Values.AddRange(carr.Values);
                            }
                            else if (opt is StringValue cstr)
                            {
                                if (arr == null)
                                    arr = new ArrayValue(cstr.Value);
                                else
                                    arr.Values.Add(cstr.Value);
                            }
                            else if (opt == null)
                            {
                                if (arr == null)
                                    arr = new ArrayValue(null);
                                else
                                    arr.Values.Add(null);
                            }

                            key = n;
                            options.Remove(n);
                        }
                    }

                    if (key == null)
                        continue;

                    if (!pi.PropertyType.IsArray)
                    {
                        if (arr.Values.Count > 1)
                            throw KeyNotArray(key);
                        value = ConvertValue(key, arr.Values[0], pi.PropertyType);
                    }
                    else
                        value = CreateArray(key, pi.PropertyType, arr.Values);
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

        private static object ConvertValue(string key, string value, Type typ)
        {
            var v = value;

            if (typ.IsArray)
            {
                var lst = new List<string>(1);
                lst.Add(v);
                return CreateArray(key, typ, lst);
            }
            else if (typ == typeof(string))
                return v;
            else if (typ == typeof(int) && int.TryParse(v, out var i4))
                return i4;
            else if (typ == typeof(bool) && v == null || string.Equals(bool.TrueString, v, StringComparison.OrdinalIgnoreCase))
                return true;
            else if (typ.IsEnum && Enum.TryParse(typ, v, true, out var en))
                return en;
            else
                throw InvalidKeyValue(key);
        }

        private static Array CreateArray(string key, Type typ, List<string> elements)
        {
            var elType = typ.GetElementType();
            var arr = Array.CreateInstance(elType, elements.Count);

            for (var i = 0; i < elements.Count; i++)
                arr.SetValue(ConvertValue(key, elements[i], elType), i);

            return arr;
        }

        private static Exception InvalidKeyValue(string key) => new DyaException($"Invalid value for the command line switch -{key}.");

        private static Exception KeyNotArray(string key) => new DyaException($"Command line switch -{key} doesn't support multiple values.");

        private static Dictionary<string, Value> Parse(string[] args, IDictionary<string, object> config)
        {
            var options = new Dictionary<string, Value>();
            string opt = null;
            string def = null;

            if (config != null)
                foreach (var kv in config)
                    options.Add(kv.Key, new StringValue { Value = kv.Value.ToString(), IsDefault = true });

            void AddOption(string key, string val)
            {
                key = key ?? "$default";
                val = val?.Trim('"');

                if (options.TryGetValue(key, out var oldval))
                {
                    if (oldval.IsDefault)
                    {
                        options.Remove(key);
                        options.Add(key, new StringValue { Value = val });
                    }
                    else if (oldval is ArrayValue arr)
                        arr.Values.Add(val);
                    else if (oldval is StringValue str)
                        options[key] = new ArrayValue(str.Value, val);
                }
                else
                    options.Add(key, new StringValue { Value = val });
            }

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
                options.Add(opt, null);

            return options;
        }
    }
}
