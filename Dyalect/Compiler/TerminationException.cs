using System;

namespace Dyalect.Compiler
{
    //This exception is thrown to terminate compilation. For example, when we hit a maximum
    //number of errors. This exception is never rethrown.
    internal sealed class TerminationException : Exception { }
}
