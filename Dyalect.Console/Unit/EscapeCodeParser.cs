using Dyalect.Parser.Model;
using Dyalect.Strings;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dyalect.Units
{
    internal static class EscapeCodeParser
    {
        public static bool Parse(string? fileName, Location loc, string str, out string? value)
        {
            if (str is null || str.Length < 2)
            {
                value = str;
                return true;
            }

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

                                    if (ns[0] == ' ' || ns[0] == '\t' || ns[3] == ' ' || ns[3] == '\t')
                                        return false;

                                    if (!int.TryParse(ns, NumberStyles.HexNumber, CultureInfo.InvariantCulture.NumberFormat, out var ci))
                                        return false;
                                    else
                                    {
                                        sb.Append((char) ci);
                                        i += 4;
                                    }
                                }
                                else
                                    return false;
                            }
                                break;
                            default:
                                return false;
                        }
                    }
                    else
                        return false;
                }
                else
                    sb.Append(c);
            }

            value = sb.ToString();
            return true;
        }
    }
}
