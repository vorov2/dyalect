using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Dyalect.Util
{
    public sealed class JsonParser
    {
        public sealed class Error
        {
            internal Error(string message, int line, int col)
            {
                Message = message;
                Line = line;
                Col = col;
            }

            public string Message { get; }

            public int Line { get; }

            public int Col { get; }

            public override string ToString() => $"Message ({Line},{Col})";
        }

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
            object obj = null;

            for (int pos = 0; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '{')
                {
                    if (obj != null)
                        Unexpected(pos, c.ToString());

                    Dictionary<string, object> dict;
                    pos = ParseObject(pos + 1, out dict);
                    obj = dict;
                }
                else if (c == '[')
                {
                    if (obj != null)
                        Unexpected(pos, c.ToString());

                    List<object> list;
                    pos = ParseList(pos, out list);
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
            obj = DictionaryComparer != null
                ? new Dictionary<string, object>(DictionaryComparer)
                : new Dictionary<string, object>();
            var key = "";
            var val = default(object);

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == ',')
                {
                    if (key == null)
                        Unexpected(pos, ",");
                    else
                    {
                        if (val != null || !SkipNulls)
                        {
                            obj.Remove(key);
                            obj.Add(key, val);
                        }

                        key = null;
                        val = null;
                    }
                }
                else if (c == '}')
                {
                    if (key != null && (val != null || !SkipNulls))
                    {
                        obj.Remove(key);
                        obj.Add(key, val);
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
            obj = new List<object>();
            var val = default(object);

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
                        if (val != null || !SkipNulls)
                            obj.Add(val);

                        pos = ParseLiteral(pos + 1, out val);
                    }
                }
                else if (c == ']')
                {
                    if (val != null || !SkipNulls)
                        obj.Add(val);
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

        private bool IsSep(char c) => char.IsWhiteSpace(c) || c == '\t' || c == '\r' || c == '\n';

        private bool IsNumeric(char c) => char.IsNumber(c) || c == '-' || c == '+' || c == 'e' || c == 'E' || c == '.';

        private int ParseString(int pos, out string key)
        {
            var sb = new StringBuilder();
            key = null;

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
                var lc = Lookup(buffer, pos + 1);

                if (lc == 'u')
                {
                    var c1 = Lookup(buffer, pos + 2);
                    var c2 = Lookup(buffer, pos + 3);
                    var c3 = Lookup(buffer, pos + 4);
                    var c4 = Lookup(buffer, pos + 5);

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
                else if (lc == '\\' || lc == '"' || lc == '/')
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

                if (c == ']' || c == '}' || c == ',' || ws)
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
                && Lookup(buffer, pos + 1) == 'u'
                && Lookup(buffer, pos + 2) == 'l'
                && Lookup(buffer, pos + 3) == 'l';
            if (!nl)
                InvalidLiteral(pos, "null");
            return pos + 3;
        }

        private int ParseBool(int pos, out bool byt)
        {
            var t = buffer[pos] == 't'
                && Lookup(buffer, pos + 1) == 'r'
                && Lookup(buffer, pos + 2) == 'u'
                && Lookup(buffer, pos + 3) == 'e';

            var f = !t && buffer[pos] == 'f'
                && Lookup(buffer, pos + 1) == 'a'
                && Lookup(buffer, pos + 2) == 'l'
                && Lookup(buffer, pos + 3) == 's'
                && Lookup(buffer, pos + 4) == 'e';

            if (!t && !f)
                InvalidLiteral(pos, "bool");

            byt = t;
            return t ? pos + 3 : pos + 4;
        }

        private int ParseLiteral(int pos, out object val)
        {
            var start = pos;
            val = null;

            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '{')
                {
                    Dictionary<string, object> obj;
                    pos = ParseObject(pos + 1, out obj);
                    val = obj;
                    return pos;
                }
                else if (c == '[')
                {
                    List<object> obj;
                    pos = ParseList(pos, out obj);
                    val = obj;
                    return pos;
                }
                else if (c == '"')
                {
                    string str;
                    pos = ParseString(pos + 1, out str);
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
                    bool byt;
                    pos = ParseBool(pos, out byt);
                    val = byt;
                    return pos;
                }
                else if (IsNumeric(c))
                {
                    double dbl;
                    pos = ParseNumber(pos, out dbl);
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

            var ns = Lookup(buffer, pos + 1);
            return ns == '*' ? ParseComment(pos + 2) :
                ns == '/' ? ParseComment2(pos + 2) : pos;
        }

        private int ParseComment(int pos)
        {
            for (; pos < len; pos++)
            {
                var c = buffer[pos];

                if (c == '*' && Lookup(buffer, pos + 1) == '/')
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

                if (c == '\r' || c == '\n')
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
            ConvertPos(pos, out var line, out var col);

            if (Errors == null)
                _errors = new List<Error>();
            _errors.Add(new Error(message, line, col));
        }

        private void ConvertPos(int pos, out int line, out int col)
        {
            var subs = source.Substring(0, pos + 1);
            var arr = subs.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            line = arr.Length;
            col = arr[arr.Length - 1].Length;
        }

        private static char Lookup(char[] buffer, int pos)
        {
            if (pos < buffer.Length && pos >= 0)
                return buffer[pos];
            else
                return '\0';
        }

        private List<Error> _errors;
        public IEnumerable<Error> Errors
        {
            get { return _errors ?? Enumerable.Empty<Error>(); }
        }

        public bool Success => _errors == null;

        public bool SkipNulls { get; set; } = true;

        public IEqualityComparer<string> DictionaryComparer { get; set; }
    }
}