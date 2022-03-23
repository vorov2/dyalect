using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;

namespace Dyalect
{
    public sealed class ConsoleTextReader : TextReader
    {
        private readonly DyFunction read;
        private readonly DyFunction readLine;
        private readonly ExecutionContext ctx;

        public ConsoleTextReader(ExecutionContext ctx, DyFunction read, DyFunction readLine) =>
            (this.ctx, this.read, this.readLine) = (ctx, read, readLine);

        public override int Read()
        {
            var ret = read.Call(ctx);

            if (ret.TypeId == DyType.Integer)
                return (int)ret.GetInteger();
            else if (ret.TypeId == DyType.Char)
                return ret.GetChar();
            else
            {
                ctx.InvalidType(DyType.Char, ret);
                return 0;
            }
        }

        public override string? ReadLine()
        {
            var ret = readLine.Call(ctx);

            if (ret.TypeId == DyType.String)
                return ret.GetString();
            else
            {
                var str = ret.ToString(ctx);

                if (ctx.HasErrors)
                    return null;

                return str.GetString();
            }
        }
    }
}
