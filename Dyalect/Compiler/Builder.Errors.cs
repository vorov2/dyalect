using Dyalect.Parser;
using Dyalect.Strings;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    //This part is responsible for emitting warnings and errors
    partial class Builder
    {
        private readonly HashSet<int> disabledWarnings = new HashSet<int>();
        private readonly HashSet<int> enabledWarnings = new HashSet<int>();

        internal List<BuildMessage> Messages { get; } = new List<BuildMessage>(); //A list of all generated messages
        internal int ErrorCount { get; private set; } //Number of errors

        private void AddMessage(BuildMessage msg)
        {
            Messages.Add(msg);

            if (msg.Type == BuildMessageType.Error)
            {
                ErrorCount++;

                //An error limit - if reached we stop compilation
                if (Messages.Count >= ERROR_LIMIT)
                {
                    //Generate "too many errors" error message and terminate
                    Messages.Add(new BuildMessage(CompilerErrors.TooManyErrors, BuildMessageType.Error,
                        (int)CompilerError.TooManyErrors, msg.Line, msg.Column, unit.FileName));
                    throw new TerminationException();
                }
            }
        }

        private void AddError(CompilerError error, Location loc, params object[] args)
        {
            var str = string.Format(CompilerErrors.ResourceManager.GetString(error.ToString()) ?? error.ToString(), args);
            AddMessage(new BuildMessage(str, BuildMessageType.Error, (int)error, Line(loc), Col(loc), unit.FileName));
        }

        private void AddWarning(CompilerWarning warning, Location loc, params object[] args)
        {
            if (options.NoWarnings)
                return;

            if ((options.IgnoreWarnings.Contains((int)warning) || disabledWarnings.Contains((int)warning))
                && !enabledWarnings.Contains((int)warning))
                return;

            var str = string.Format(CompilerErrors.ResourceManager.GetString(warning.ToString()) ?? warning.ToString(), args);
            AddMessage(new BuildMessage(str, BuildMessageType.Warning, (int)warning, Line(loc), Col(loc), unit.FileName));
        }

        public bool Success => ErrorCount == 0;
    }
}
