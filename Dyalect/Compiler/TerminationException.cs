using System;

namespace Dyalect.Compiler
{
    //Кидаем это исключение, когда нам надоедает компилировать. Например, выстрелил
    //лимит на максимальное количество ошибок. Это исключение никогда не перебрасывается.
    public sealed class TerminationException : Exception
    {
    }
}
