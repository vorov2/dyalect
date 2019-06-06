using Dyalect.Parser;
using Dyalect.Strings;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    //Эта часть отвечает за генерацию ошибок, ворнингов и пр.
    partial class Builder
    {
        internal List<BuildMessage> Messages { get; } = new List<BuildMessage>(); //Список всех сгенерированных при билде сообщений
        internal int ErrorCount { get; private set; } //Текущее количество ошибок

        private void AddMessage(BuildMessage msg)
        {
            Messages.Add(msg);

            if (msg.Type == BuildMessageType.Error)
            {
                ErrorCount++;

                //Лимит на ошибки, после которого мы уже не хотим продолжать компиляцию. Ибо
                //и компилятору надо отдохнуть
                if (Messages.Count >= ERROR_LIMIT)
                {
                    //Здесь мы генерируем сообщение 'Слишком много ошибок' и заканчиваем.
                    Messages.Add(new BuildMessage(CompilerErrors.TooManyErrors, BuildMessageType.Error,
                        (int)CompilerError.TooManyErrors, msg.Line, msg.Column, unit.FileName));
                    throw new TerminationException();
                }
            }
        }

        private void AddError(CompilerError error, Location loc, params object[] args)
        {
            var str = string.Format(CompilerErrors.ResourceManager.GetString(error.ToString()) ?? error.ToString(), args);
            AddMessage(new BuildMessage(str, BuildMessageType.Error, (int)error, loc.Line, loc.Column, unit.FileName));
        }

        private void AddWarning(CompilerWarning warning, Location loc, params object[] args)
        {
            var str = string.Format(CompilerErrors.ResourceManager.GetString(warning.ToString()) ?? warning.ToString(), args);
            AddMessage(new BuildMessage(str, BuildMessageType.Warning, (int)warning, loc.Line, loc.Column, unit.FileName));
        }

        public bool Success => ErrorCount == 0;
    }
}
