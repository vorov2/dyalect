using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Text;

namespace Dyalect
{
    public sealed class ConsoleTextWriter : TextWriter
    {
        private readonly DyFunction write;
        private readonly DyFunction? writeLine;
        private readonly ExecutionContext ctx;

        public override Encoding Encoding => Encoding.UTF8;

        public ConsoleTextWriter(ExecutionContext ctx, DyFunction write) : this(ctx, write, null) { }
        
        public ConsoleTextWriter(ExecutionContext ctx, DyFunction write, DyFunction? writeLine) =>
            (this.ctx, this.write, this.writeLine) = (ctx, write, writeLine);

        public override void Write(string? value) => write.Call(ctx, new DyString(value ?? ""));

        public override void WriteLine(string? value) => (writeLine ?? write).Call(ctx, new DyString(value ?? ""));
    }
}
