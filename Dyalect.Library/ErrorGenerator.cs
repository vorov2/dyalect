using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library
{
    internal static class ErrorGenerator
    {
        public static DyObject IOFailed(this ExecutionContext ctx) => ctx.CustomError("IOFailed");
    }
}
