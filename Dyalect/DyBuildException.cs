using System.Collections.Generic;
using System.Linq;

namespace Dyalect;

public class DyBuildException : DyException
{
    public IEnumerable<BuildMessage> Messages { get; }

    public DyBuildException(IEnumerable<BuildMessage> messages) : base("") =>
        Messages = messages;

    public DyBuildException(string message, Exception? innerException) : base(message, innerException) =>
        Messages = Enumerable.Empty<BuildMessage>();

    public override string Message =>
        Messages is null || !Messages.Any() ? base.Message
            : string.Join(Environment.NewLine, Messages.Select(m => m.ToString()));
}
