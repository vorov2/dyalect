using System;

namespace Dyalect.Command
{
    public sealed class CommandException : Exception
    {
        public CommandException(string message) : base(message, null)
        {

        }
    }
}
