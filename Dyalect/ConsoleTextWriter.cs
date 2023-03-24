using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.IO;
using System.Text;

namespace Dyalect;

public sealed class ConsoleTextWriter : TextWriter
{
    private readonly DyObject write;
    private readonly DyObject? writeLine;
    private readonly ExecutionContext ctx;

    public override Encoding Encoding => Encoding.UTF8;

    public ConsoleTextWriter(ExecutionContext ctx, DyObject write) : this(ctx, write, null) { }
    
    public ConsoleTextWriter(ExecutionContext ctx, DyObject write, DyObject? writeLine) =>
        (this.ctx, this.write, this.writeLine) = (ctx, write, writeLine);

    public override void Write(string? value) => write.Invoke(ctx, DyString.Get(value));

    public override void WriteLine(string? value) => (writeLine ?? write).Invoke(ctx, DyString.Get(value));
}
