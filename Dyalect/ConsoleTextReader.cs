﻿using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;

namespace Dyalect;

public sealed class ConsoleTextReader : TextReader
{
    private readonly DyObject read;
    private readonly DyObject readLine;
    private readonly ExecutionContext ctx;

    public ConsoleTextReader(ExecutionContext ctx, DyObject read, DyObject readLine) =>
        (this.ctx, this.read, this.readLine) = (ctx, read, readLine);

    public override int Read()
    {
        var ret = read.Invoke(ctx);

        if (ret is DyInteger i)
            return (int)i.Value;
        else if (ret is DyChar c)
            return c.Value;
        else
        {
            ctx.InvalidType(Dy.Integer, Dy.Char, ret);
            return 0;
        }
    }

    public override string? ReadLine()
    {
        var ret = readLine.Invoke(ctx);

        if (ret is DyString s)
            return s.Value;
        else
        {
            var str = ret.ToString(ctx);

            if (ctx.HasErrors)
                return null;

            return str.Value;
        }
    }
}
