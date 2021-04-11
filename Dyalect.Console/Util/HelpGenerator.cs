using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dyalect.Util
{
    internal static class HelpGenerator
    {
        private const int HELP_LENGTH = 60;

        public static string Generate<T>(string prefix = "-") => Generate(typeof(T), prefix);

        public static string Generate(Type type, string prefix = "-")
        {
            var sb = new StringBuilder();
            var names = new List<string>();
            var helps = new List<string>();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            var props = type.GetProperties(flags)
                .OfType<MemberInfo>()
                .Concat(type.GetMethods(flags))
                .Where(p => Attribute.IsDefined(p, typeof(BindingAttribute)));

            foreach (var ac in props)
            {
                var attr = Attribute.GetCustomAttribute(ac, typeof(BindingAttribute)) as BindingAttribute;

                if (attr is null || attr.Help is null)
                    continue;

                var ln = new List<string>();

                foreach (var n in attr.Names)
                    ln.Add(n);

                var name = string.Join(", ", ln.Select(n => prefix + n));

                if (string.IsNullOrEmpty(name))
                    name = "<default>";

                var help = attr.Help ?? "";
                names.Add(name);
                helps.Add(help);
            }

            var pad = names.Any () ? names.Select(s => s.Length).Max(): 0;

            for (var i = 0; i < names.Count; i++)
            {
                sb.AppendFormat("{0}{1}    ", names[i], new string(' ', pad - names[i].Length));
                var h = helps[i];
                SplitHelp(sb, pad, h);
            }

            return sb.ToString();
        }

        private static void SplitHelp(StringBuilder sb, int pad, string h)
        {
            if (h.Length > HELP_LENGTH)
            {
                var idx = 0;
                var lastIdx = 0;
                while ((idx = h.IndexOf(' ', lastIdx + 1)) < HELP_LENGTH && idx != -1)
                    lastIdx = idx;
                lastIdx = lastIdx == 0 ? idx : lastIdx;

                if (lastIdx > 0)
                {
                    var ch = h[0..lastIdx];
                    h = h[lastIdx..].Trim();

                    sb.AppendLine(ch);
                    sb.Append(new string(' ', pad + 4));
                    SplitHelp(sb, pad, h);
                }
                else
                    sb.AppendLine(h);
            }
            else
                sb.AppendLine(h);
        }
    }
}
