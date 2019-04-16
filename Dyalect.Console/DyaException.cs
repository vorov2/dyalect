using System;

namespace Dyalect
{
    public sealed class DyaException : Exception
    {
        public DyaException(string message) : base(message, null)
        {

        }
    }
}
