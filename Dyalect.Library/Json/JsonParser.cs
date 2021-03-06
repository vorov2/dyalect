using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dyalect.Library.Json
{
    public sealed class JsonParser
    {
        private readonly string source;
        private readonly char[] buffer;
        private readonly int len;

        public JsonParser(string source) 
        {
            this.source = source;
            buffer = source.ToCharArray();
            len = buffer.Length;
            DictionaryComparer = StringComparer.OrdinalIgnoreCase;
        }

        public object Parse()
        {
            object obj = null!;

            for (int pos = 0; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '{')
                {
                    if (obj != null)
                        Unexpected(pos, c.ToString());

                    pos = ParseObject(pos + 1, out var dict);
                    obj = dict;
                }
                else if (c == '[')
                {
                    if (obj != null)
                        Unexpected(pos, c.ToString());

                    pos = ParseList(pos, out var list);
                    obj = list;
                }
                else
                {
                    var p = CheckComment(pos);

                    if (p != pos)
                        pos = p;
                    else if (!IsSep(c))
                        Unexpected(pos, c.ToString());
                }
            }

            return obj;
        }

        private int ParseObject(int pos, out Dictionary<string, object> obj)
        {
            obj = DictionaryComparer is not null 
                ? new Dictionary<string, object>(DictionaryComparer)
                : new Dictionary<string, object>();
            var key = "";
            var val = default(object);

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == ',')
                {
                    if (key is null)
                        Unexpected(pos, ",");
                    else
                    {
                        if (val != null || !SkipNulls)
                        {
                            obj.Remove(key);
                            obj.Add(key, val!);
                        }
                            
                        key = null;
                        val = null;
                    }
                }
                else if (c == '}')
                {
                    if (key != null && (val is not null || !SkipNulls))
                    {
                        obj.Remove(key);
                        obj.Add(key, val!);
                    }

                    return pos;
                }
                else if (c == '"')
                {
                    pos = ParseString(pos + 1, out key);
                    pos = ParseValueStart(pos + 1);
                    pos = ParseLiteral(pos, out val);
                }
                else
                {
                    var p = CheckComment(pos);

                    if (p != pos)
                        pos = p;
                    else if (!IsSep(c))
                        Unexpected(pos, c.ToString());
                }
            }

            Expected(pos, "}");
            return pos;
        }

        private int ParseList(int pos, out List<object> obj)
        {
            obj = new();
            object val = null!;

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '[')
                    pos = ParseLiteral(pos + 1, out val);
                else if (c == ',')
                {
                    if (val == null)
                        Unexpected(pos, ",");
                    else
                    {
                        if (val is not null || !SkipNulls)
                            obj.Add(val!);

                        pos = ParseLiteral(pos + 1, out val);
                    }
                }
                else if (c == ']')
                {
                    if (val is not null || !SkipNulls)
                        obj.Add(val!);
                    return pos;
                }
                else
                {
                    var p = CheckComment(pos);

                    if (p != pos)
                        pos = p;
                    else if (!IsSep(c))
                        Unexpected(pos, c.ToString());
                }
            }

            Expected(pos, "]");
            return pos;
        }

        private static bool IsSep(char c) => char.IsWhiteSpace(c) || c is '\t' or '\r' or '\n';

        private static bool IsNumeric(char c) => char.IsNumber(c) || c is '-' or '+' or 'e' or 'E' or '.';

        private int ParseString(int pos, out string key)
        {
            var sb = new StringBuilder();
            key = null!;

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '"')
                {
                    key = sb.ToString();
                    return pos;
                }
                else 
                {
                    var p = CheckEscape(sb, pos);
                    if (p == pos)
                        sb.Append(c);
                    else
                        pos = p;
                }
            }

            Expected(pos, "\"");
            return pos;
        }

        private int CheckEscape(StringBuilder sb, int pos)
        {
            if (buffer[pos] == '\\')
            {
                var lc = buffer.Lookup(pos + 1);

                if (lc == 'u')
                {
                    var c1 = buffer.Lookup(pos + 2);
                    var c2 = buffer.Lookup(pos + 3);
                    var c3 = buffer.Lookup(pos + 4);
                    var c4 = buffer.Lookup(pos + 5);
                    
                    if (char.IsNumber(c1) && char.IsNumber(c2)
                        && char.IsNumber(c3) && char.IsNumber(c4))
                        {
                            sb.Append((char)int.Parse(c1.ToString() + c2 + c3 + c4,
                                NumberStyles.HexNumber));
                            return pos + 5;
                        }
                    else
                        EscapeSequence(pos);
                }
                else if (lc is '\\' or '"' or '/')
                    sb.Append(lc);
                else if (lc == 'b')
                    sb.Append('\b');
                else if (lc == 'f')
                    sb.Append('\f');
                else if (lc == 'n')
                    sb.Append('\n');
                else if (lc == 'r')
                    sb.Append('\r');
                else if (lc == 't')
                    sb.Append('\t');
                else
                    EscapeSequence(pos);

                return pos + 1;
            }
            else
                return pos;
        }

        private int ParseNumber(int pos, out double dbl)
        {
            var start = pos;
            dbl = 0;

            for (; pos < len; pos++)
            {
                var c = buffer[pos];
                var ws = IsSep(c);

                if (c is ']' or '}' or ',' || ws)
                {
                    var str = new string(buffer, start, pos - start);

                    if (!double.TryParse(str, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out dbl))
                        InvalidLiteral(pos, "number");

                    return ws ? pos : pos - 1;
                }
                else if (!IsNumeric(c))
                    InvalidLiteral(pos, "number");
            }

            Unexpected(pos, "EOF");
            return pos;
        }

        private int ParseNull(int pos)
        {
            var nl = buffer[pos] == 'n'
                && buffer.Lookup(pos + 1) == 'u'
                && buffer.Lookup(pos + 2) == 'l'
                && buffer.Lookup(pos + 3) == 'l';
            
            if (!nl)
                InvalidLiteral(pos, "null");
            
            return pos + 3;
        }

        private int ParseBool(int pos, out bool byt)
        {
            var t = buffer[pos] == 't'
                && buffer.Lookup(pos + 1) == 'r'
                && buffer.Lookup(pos + 2) == 'u'
                && buffer.Lookup(pos + 3) == 'e';

            var f = !t && buffer[pos] == 'f'
                && buffer.Lookup(pos + 1) == 'a'
                && buffer.Lookup(pos + 2) == 'l'
                && buffer.Lookup(pos + 3) == 's'
                && buffer.Lookup(pos + 4) == 'e';

            if (!t && !f)
                InvalidLiteral(pos, "bool");
            
            byt = t;
            return t ? pos + 3 : pos + 4;
        }

        private int ParseLiteral(int pos, out object val)
        {
            val = null!;

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '{')
                {
                    pos = ParseObject(pos + 1, out var obj);
                    val = obj;
                    return pos;
                }
                else if (c == '[')
                {
                    pos = ParseList(pos, out var obj);
                    val = obj;
                    return pos;
                }
                else if (c == '"')
                {
                    pos = ParseString(pos + 1, out var str);
                    val = str;
                    return pos;
                }
                else if (c == 'n')
                {
                    pos = ParseNull(pos);
                    return pos;
                }
                else if (c == 't' || c == 'f')
                {
                    pos = ParseBool(pos, out var byt);
                    val = byt;
                    return pos;
                }
                else if (IsNumeric(c))
                {
                    pos = ParseNumber(pos, out var dbl);
                    val = dbl;
                    return pos;
                }
                else
                {
                    var p = CheckComment(pos);

                    if (p != pos)
                        pos = p;
                    else if (!IsSep(c))
                        Unexpected(pos, c.ToString());
                }
            }

            return pos;
        }

        private int ParseValueStart(int pos)
        {
            for (; pos < len; pos++)
            {
                var c = buffer[pos];
            
                if (c == ':')
                    return pos + 1;
                else
                {
                    var p = CheckComment(pos);

                    if (p != pos)
                        pos = p;
                    else if (!IsSep(c))
                        Unexpected(pos, c.ToString());
                }
            }

            Expected(pos, ":");
            return pos;
        }

        private int CheckComment(int pos)
        {
            var sc = buffer[pos] == '/';

            if (!sc)
                return pos;

            var ns = buffer.Lookup(pos + 1);
            return ns switch
            {
                '*' => ParseComment(pos + 2),
                '/' => ParseComment2(pos + 2),
                _ => pos
            };
        }

        private int ParseComment(int pos)
        {
            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '*' && buffer.Lookup(pos + 1) == '/')
                    return pos + 1;
            }

            Expected(pos, "*/");
            return pos;
        }

        private int ParseComment2(int pos)
        {
            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c is '\r' or '\n')
                    return pos;
            }
            
            return pos;
        }

        private void EscapeSequence(int pos) => Failure(pos, "Invalid escape sequence");

        private void Expected(int pos, string sym) => Failure(pos, $"Expected {sym}");

        private void Unexpected(int pos, string sym) => Failure(pos, $"Unexpected {sym}");

        private void InvalidLiteral(int pos, string literal) => Failure(pos, $"Invalid {literal} literal");

        private void Failure(int pos, string message)
        {
            var t = ConvertPos(pos);

            if (ThrowErrors)
                throw new JsonParserException(message, t.Line, t.Col);
            
            errors ??= new();
            errors.Add(new(message, t));
        }

        private Location ConvertPos(int pos)
        {
            var subs = source[..(pos + 1)];
            var arr = subs.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var line = arr.Length;
            var col = arr[^1].Length;
            return new(line, col);
        }

        private List<JsonError>? errors;
        public IEnumerable<JsonError>? Errors => errors;

        public bool ThrowErrors { get; set; } = true;

        public bool SkipNulls { get; set; } = true;

        public bool IsSuccess => errors is null || errors.Count == 0;

        public IEqualityComparer<string> DictionaryComparer { get; set; }
    }
}