using Dyalect.Parser.Model;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dyalect.Parser
{
    internal static class EscapeCodeParser
    {
        public static bool Parse(string fileName, Location loc, string str, List<BuildMessage> messages, out string value, out List<StringChunk> chunks)
        {
            var buffer = str.ToCharArray(1, str.Length - 2);
            var len = buffer.Length;
            var sb = new StringBuilder(str.Length);
            value = null;
            chunks = null;

            for (var i = 0; i < len; i++)
            {
                var c = buffer[i];

                if (c == '\\')
                {
                    i++;

                    if (i < len)
                    {
                        var cn = buffer[i];

                        switch (cn)
                        {
                            case 's':
                                sb.Append('\u0020');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case '"':
                                sb.Append('"');
                                break;
                            case '\'':
                                sb.Append('\'');
                                break;
                            case '\\':
                                sb.Append('\\');
                                break;
                            case '0':
                                sb.Append('\0');
                                break;
                            case '(':
                                {
                                    if (chunks == null)
                                        chunks = new List<StringChunk>();

                                    if (sb.Length > 0)
                                    {
                                        chunks.Add(new PlainStringChunk(sb.ToString()));
                                        sb.Clear();
                                    }

                                    var start = i;
                                    var code = Balance(ref i, buffer);

                                    if (string.IsNullOrWhiteSpace(code))
                                        return InvalidCodeIsland(messages, loc, fileName, start);

                                    chunks.Add(new CodeChunk(code));
                                }
                                break;
                            case 'u':
                                {
                                    if (i + 3 < len)
                                    {
                                        var ns = new string(buffer, i + 1, 4);
                                        var ci = 0;

                                        if (ns[0] == ' ' || ns[0] == '\t' || ns[3] == ' ' || ns[3] == '\t')
                                            return InvalidLiteral(messages, loc, fileName, i);

                                        if (!int.TryParse(ns, NumberStyles.HexNumber,
                                            CI.Default.NumberFormat, out ci))
                                            return InvalidLiteral(messages, loc, fileName, i);
                                        else
                                        {
                                            sb.Append((char)ci);
                                            i += 4;
                                        }
                                    }
                                    else
                                        return InvalidLiteral(messages, loc, fileName, i);
                                }
                                break;
                            default:
                                return InvalidLiteral(messages, loc, fileName, i);
                        }
                    }
                    else
                        return InvalidLiteral(messages, loc, fileName, i);
                }
                else
                    sb.Append(c);
            }

            if (chunks != null && sb.Length > 0)
                chunks.Add(new PlainStringChunk(sb.ToString()));
            else
                value = sb.ToString();
            return true;
        }

        private static string Balance(ref int i, char[] buffer)
        {
            var b = 0;
            var start = i;

            for (; i < buffer.Length; i++)
            {
                var c = buffer[i];

                if (c == '(')
                    b++;
                else if (c == ')')
                    b--;

                if (b == 0)
                    return new string(buffer, start + 1, i - start - 1);
            }

            return null;
        }

        private static bool InvalidLiteral(List<BuildMessage> messages, Location baseLocation, string fileName, int offset)
        {
            messages.Add(new BuildMessage(ParserErrors.InvalidEscapeCode, BuildMessageType.Error, (int)ParserError.InvalidEscapeCode, baseLocation.Line, baseLocation.Column + offset, fileName));
            return false;
        }

        private static bool InvalidCodeIsland(List<BuildMessage> messages, Location baseLocation, string fileName, int offset)
        {
            messages.Add(new BuildMessage(ParserErrors.CodeIslandInvalid, BuildMessageType.Error, (int)ParserError.CodeIslandInvalid, baseLocation.Line, baseLocation.Column + offset, fileName));
            return false;
        }
    }
}
