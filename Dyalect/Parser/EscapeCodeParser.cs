using System;
using System.Globalization;
using System.Text;

namespace Dyalect.Parser
{
    internal static class EscapeCodeParser
    {
        public static int Parse(string str, out string value)
        {
            var buffer = str.ToCharArray(1, str.Length - 2);
            var len = buffer.Length;
            var sb = new StringBuilder(str.Length);
            value = null;

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
                            case 'u':
                                {
                                    if (i + 3 < len)
                                    {
                                        var ns = new string(buffer, i + 1, 4);
                                        var ci = 0;

                                        if (ns[0] == ' ' || ns[0] == '\t' || ns[3] == ' ' || ns[3] == '\t')
                                            return i;

                                        if (!int.TryParse(ns, NumberStyles.HexNumber,
                                            CI.Default.NumberFormat, out ci))
                                            return i;
                                        else
                                        {
                                            sb.Append((char)ci);
                                            i += 4;
                                        }
                                    }
                                    else
                                        return i;
                                }
                                break;
                            default:
                                return i;
                        }
                    }
                    else
                        return i;
                }
                else
                    sb.Append(c);
            }

            value = sb.ToString();
            return -1;
        }
    }
}
