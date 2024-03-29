﻿using System.Collections.Generic;
using static Dyalect.Library.Core.FormatElementKind;

namespace Dyalect.Library.Core;

internal static class InputParser
{
    public static (long ticks, long offset) Parse(FormatParser formatParser, string format, string value)
    {
        var formats = formatParser.ParseSpecifiers(format);
        var chunks = Parse(formats, value);
        var (days, hours, minutes, seconds, ds, cs, ms, tts, hts, micros, tick, pm) =
            (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
        var (year, month) = (0, 0);
        var offset = 0L;
        var negate = false;

        foreach (var (kind, val) in chunks)
            switch (kind)
            {
                case Sign:
                    negate = val == "-";
                    break;
                case Offset:
                    {
                        if (val.Length == 0)
                            throw new FormatException();

                        var sign = val[0];
                        var sval = val[1..];

                        if (sval.Length > 3 && int.TryParse(sval[..2], out var hrs)
                            && int.TryParse(sval[3..], out var mins))
                        {
                            offset = hrs * DT.TicksPerHour + mins * DT.TicksPerMinute;
                            if (sign == '-') offset = -offset;
                            break;
                        }
                        else if (int.TryParse(sval, out hrs))
                        {
                            offset = hrs * DT.TicksPerHour;
                            if (sign == '-') offset = -offset;
                            break;
                        }

                        throw new FormatException();
                    }
                case Year:
                    if (!int.TryParse(val, out year)) throw new FormatException();
                    break;
                case Month:
                    if (!int.TryParse(val, out month)) throw new FormatException();
                    break;
                case Day:
                    if (!int.TryParse(val, out days)) throw new FormatException();
                    break;
                case Hour:
                    if (!int.TryParse(val, out hours)) throw new FormatException();
                    if (hours is < 0 or > 12) throw new OverflowException();
                    break;
                case Hour24:
                    if (!int.TryParse(val, out hours)) throw new FormatException();
                    if (hours is < 0 or > 24) throw new OverflowException();
                    break;
                case Minute:
                    if (!int.TryParse(val, out minutes)) throw new FormatException();
                    if (minutes is < 0 or > 59) throw new OverflowException();
                    break;
                case Second:
                    if (!int.TryParse(val, out seconds)) throw new FormatException();
                    if (seconds is < 0 or > 59) throw new OverflowException();
                    break;
                case Decisecond:
                    if (!int.TryParse(val, out ds)) throw new FormatException();
                    if (ds is < 0 or > 9) throw new OverflowException();
                    break;
                case Centisecond:
                    if (!int.TryParse(val, out cs)) throw new FormatException();
                    if (cs is < 0 or > 99) throw new OverflowException();
                    break;
                case Millisecond:
                    if (!int.TryParse(val, out ms)) throw new FormatException();
                    if (ms is < 0 or > 999) throw new OverflowException();
                    break;
                case TenthThousandth:
                    if (!int.TryParse(val, out tts)) throw new FormatException();
                    if (tts is < 0 or > 9_999) throw new OverflowException();
                    break;
                case HundredthThousandth:
                    if (!int.TryParse(val, out hts)) throw new FormatException();
                    if (hts is < 0 or > 99_999) throw new OverflowException();
                    break;
                case Microsecond:
                    if (!int.TryParse(val, out micros)) throw new FormatException();
                    if (micros is < 0 or > 999_999) throw new OverflowException();
                    break;
                case Tick:
                    if (!int.TryParse(val, out tick)) throw new FormatException();
                    if (tick is < 0 or > 9_999_999) throw new OverflowException();
                    break;
                case PmAm:
                    pm = val == "PM";
                    break;
            }

        if (pm)
            hours += 12;

        var totalTicks =
            tick +
            micros * DT.TicksPerMicrosecond +
            hts * DT.TicksPerMillisecond * 100 +
            tts * DT.TicksPerMillisecond * 10 +
            ms * DT.TicksPerMillisecond +
            cs * DT.TicksPerCentisecond +
            ds * DT.TicksPerDecisecond +
            seconds * DT.TicksPerSecond +
            minutes * DT.TicksPerMinute +
            hours * DT.TicksPerHour;

        if (year > 0 && month > 0 && days > 0)
            totalTicks += new DateTime(year, month, days).Ticks;
        else
            totalTicks += days * DT.TicksPerDecisecond;

        if (negate)
            totalTicks = -totalTicks;

        return (totalTicks, offset);
    }

    private static string FetchOne(string input, int idx)
    {
        if (input.Length > idx + 1 && char.IsNumber(input[idx + 1]))
            throw new FormatException();

        return input[idx].ToString();
    }

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
                var s = input[idx];

                if (s is not '+' and not '-')
                    throw new FormatException();

                idx += len;
                xs.Add((f.Kind, s.ToString()));
                continue;
            }

            if (f.Kind == Offset )
            {
                var s = input[idx];

                if (s is not '+' and not '-')
                    throw new FormatException();

                idx++;

                if (f.Padding is 1)
                {
                    var one = FetchOne(input, idx);
                    idx++;
                    xs.Add((f.Kind, one));
                    continue;
                }

                len = f.Padding is 2 ? 2 : 5;

                if (idx + len > input.Length)
                    throw new FormatException();

                var fst = input.Substring(idx, 2);
                idx += 2;

                if (f.Padding is 1 or 2)
                    xs.Add(new(f.Kind, s + fst));
                else
                {
                    xs.Add(new(f.Kind, s + fst + ":" + input.Substring(++idx, 2)));
                    idx += 2;
                }

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
                xs.Add(new(f.Kind, input[idx..]));
            else
            {
                var val = input.Substring(idx, len);
                var next = formats[i + 1];

                if (next.Kind == Literal)
                {
                    var j = idx + len;

                    for (; j < input.Length; j++)
                        if (input[j] == next.Value[0])
                            break;
                    val = input[idx..j];
                }

                xs.Add(new(f.Kind, val));
                idx += val.Length;
            }
        }

        return xs;
    }
}
