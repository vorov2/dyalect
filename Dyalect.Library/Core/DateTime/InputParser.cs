using System;
using System.Collections.Generic;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core;

internal static class InputParser
{
    public static List<(FormatElementKind kind, string val)> Parse(List<FormatElement> formats, string input)
    {
        var idx = 0;
        var xs = new List<(FormatElementKind kind, string val)>();

        for (var i = 0; i < formats.Count; i++)
        {
            var f = formats[i];
            var len = f.Kind is Literal or Sign ? f.Value.Length : f.Padding;

            if (idx + len > input.Length)
                throw new FormatException();

            if (f.Kind == Sign)
            {
                var s = input.Substring(idx, len);

                if (s is not "+" and not "-")
                    throw new FormatException();

                idx += len;
                xs.Add((f.Kind, s));
                continue;
            }

            if (f.Kind == Literal)
            {
                if (input.Substring(idx, len) != f.Value)
                    throw new FormatException();

                xs.Add((f.Kind, f.Value));
                idx += len;
                continue;
            }

            if (i == formats.Count - 1)
                xs.Add(new(f.Kind, input.Substring(idx)));
            else
            {
                var val = input.Substring(idx, len);
                var next = formats[i + 1];

                if (f.Kind != Literal && next.Kind == Literal)
                {
                    var j = idx + len;

                    for (; j < input.Length; j++)
                        if (input[j] == next.Value[0])
                            break;
                    val = input.Substring(idx, j - idx);
                }

                xs.Add(new(f.Kind, val));
                idx += val.Length;
            }
        }

        return xs;
    }
}
