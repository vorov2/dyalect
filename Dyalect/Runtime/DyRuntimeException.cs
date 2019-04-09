using System;

namespace Dyalect.Runtime
{
    public class DyRuntimeException : DyException
    {
        public DyRuntimeException(string message, Exception ex) : base(message, ex)
        {

        }

        public DyRuntimeException(string message) : base(message)
        {

        }
    }
}
