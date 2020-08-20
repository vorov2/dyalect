using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dyalect.Library.Json
{
    public sealed class JsonFormatter
    {
        private readonly JsonFormatInfo format;
        private readonly string newLine;

        public readonly static JsonFormatter Compact = new JsonFormatter(JsonFormatInfo.Compact);
        public readonly static JsonFormatter Default = new JsonFormatter(JsonFormatInfo.Default);

        public JsonFormatter(JsonFormatInfo format)
        {
            this.format = format;

            switch (format.LineEndings)
            {
                default: newLine = Environment.NewLine; break;
                case JsonFormatInfo.Eol.Cr: newLine = "\r"; break;
                case JsonFormatInfo.Eol.Lf: newLine = "\n"; break;
                case JsonFormatInfo.Eol.CrLf: newLine = "\r\n"; break;
            }
        }

        public string Format(object value)
        {
            var sb = new StringBuilder();
            FormatObject(value, sb, 0);
            return sb.ToString();
        }

        private void FormatObject(object value, StringBuilder sb, int indent)
        {
            if (value == null)
                sb.Append("null");
            else if (value is double d)
                Format(d, sb);
            else if (value is bool b)
                Format(b, sb);
            else if (value is Dictionary<string, object> dict)
                Format(dict, sb, indent);
            else if (value is List<object> xs)
                Format(xs, sb, indent);
            else
                Format(value.ToString(), sb);
        }

        private void Format(bool value, StringBuilder sb)
        {
            sb.Append(value ? "true" : "false");
        }

        private void Format(double value, StringBuilder sb)
        {
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private void Format(string value, StringBuilder sb)
        {
            sb.Append('"');
            var str = value ?? "";
            str = str
                .Replace("\\", "\\\\")
                .Replace("\t", "\\t")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\"", "\\\"");
            sb.Append(str);
            sb.Append('"');
        }

        private void Format(List<object> list, StringBuilder sb, int indent)
        {
            sb.Append('[');
            var compact = format.CompactAll || (format.CompactList
                && !list.Any(e => e is List<object> || e is Dictionary<string, object>));
            
            if (!compact)
                sb.Append(newLine);

            var c = 0;

            foreach (var v in list)
            {
                if (c++ > 0)
                {
                    sb.Append(',');
                    if (!compact)
                        sb.Append(newLine);
                }

                if (!compact && format.IndentWithTabs)
                    sb.Append('\t', indent + 1);
                else if (!compact)
                    sb.Append(' ', indent + format.IndentSize);

                FormatObject(v, sb, indent + (format.IndentWithTabs ? 1 : format.IndentSize));
            }

            if (!compact)
            {
                sb.Append(newLine);
                sb.Append(format.IndentWithTabs ? '\t' : ' ', indent);
            }

            sb.Append(']');
        }

        private void Format(Dictionary<string, object> dict, StringBuilder sb, int indent)
        {
            sb.Append('{');
            if (!format.CompactAll)
                sb.Append(newLine);
            var c = 0;

            foreach (var kv in dict)
            {
                if (c++ > 0)
                {
                    sb.Append(',');
                    if (!format.CompactAll)
                        sb.Append(newLine);
                }

                if (!format.CompactAll && format.IndentWithTabs)
                    sb.Append('\t', indent + 1);
                else if (!format.CompactAll)
                    sb.Append(' ', indent + format.IndentSize);

                sb.Append($"\"{kv.Key}\": ");

                if (kv.Value is Array || kv.Value is Object)
                    FormatObject(kv.Value, sb, indent +
                        (format.IndentWithTabs ? 1 : format.IndentSize));
                else
                    FormatObject(kv.Value, sb, 0);
                
            }

            if (!format.CompactAll)
            {
                sb.Append(newLine);
                sb.Append(format.IndentWithTabs ? '\t' : ' ', indent);
            }

            sb.Append('}');
        }
    }
}