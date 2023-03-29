using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect.Runtime;

public static class Extensions
{
    public static T Type<T>(this ExecutionContext ctx) where T : DyTypeInfo => 
        ctx.RuntimeContext.Types.OfType<T>().First();
}
