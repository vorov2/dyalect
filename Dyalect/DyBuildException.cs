using System;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect;

public class DyBuildException : DyException
{
    public DyBuildException(IEnumerable<BuildMessage> messages) : base("") =>
        Messages = messages;

    public DyBuildException(string message, Exception? innerException) : base(message, innerException) =>
        Messages = Enumerable.Empty<BuildMessage>();

    public override string Message =>
        Messages != null && Messages.Any()
            ? string.Join(Environment.NewLine, Messages.Select(m => m.ToString()).ToArray())
            : base.Message;

    public IEnumerable<BuildMessage> Messages { get; }
}
