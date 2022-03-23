using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Text;

namespace Dyalect
{
    public sealed class ConsoleTextWriter : TextWriter
    {
        private readonly DyFunction writer;
        private readonly ExecutionContext ctx;

        public override Encoding Encoding => Encoding.UTF8;

        public ConsoleTextWriter(ExecutionContext ctx, DyFunction writer) =>
            (this.ctx, this.writer) = (ctx, writer);

        public override void Write(string? value)
        {
            writer.Call(ctx, new DyString(value ?? ""));
        }
    }
}
