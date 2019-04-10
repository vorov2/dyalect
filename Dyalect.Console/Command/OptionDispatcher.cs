using System;
using System.Linq;
using System.Reflection;

namespace Dyalect.Command
{
    public static class OptionDispatcher
    {
        public static T Dispatch<T>() where T : IOptionBag, new()
        {
            var cl = CommandLineParser.Parse();
            var bag = new T();
            bag.DefaultArgument = cl.DefaultArgument;
            bag.StartupPath = cl.StartupPath;

            foreach (var pi in typeof(T).GetProperties())
            {
                var attr = Attribute.GetCustomAttribute(pi, typeof(BindingAttribute)) as BindingAttribute;

                if (attr == null)
                    continue;

                var opts = cl.Options.Where(o => attr.Names.Contains(o.Key)).ToArray();
                object value = null;

                if (opts.Length > 1)
                {
                    if (!pi.PropertyType.IsArray)
                        throw KeyNotArray(opts[0].Key);

                    foreach (var o in opts)
                        cl.Options.Remove(o);

                    value = CreateArray(pi, opts.Select(o => o.Value).ToArray());
                }
                else if (opts.Length > 0)
                {
                    cl.Options.Remove(opts[0]);

                    if (pi.PropertyType.IsArray)
                        value = CreateArray(pi, opts[0].Value);
                    else
                        value = opts[0].Value;
                }

                if (value != null)
                {
                    try
                    {
                        pi.SetValue(bag, value);
                    }
                    catch
                    {
                        throw InvalidKeyValue(opts[0].Key);
                    }
                }
            }

            if (cl.Options.Count > 0)
                throw new CommandException($"Неизвестный ключ \"{cl.Options[0].Key}\".");

            return bag;
        }

        private static Array CreateArray(PropertyInfo pi, params object[] elements)
        {
            var arr = Array.CreateInstance(pi.PropertyType.GetElementType(), elements.Length);

            for (var i = 0; i < elements.Length; i++)
                arr.SetValue(elements[i], i);

            return arr;
        }

        private static Exception InvalidKeyValue(string key) => new CommandException($"Недопустимое значение для ключа \"{key}\".");

        private static Exception KeyNotArray(string key) => new CommandException($"Ключ \"{key}\" не поддерживает множественные значения.");
    }
}
