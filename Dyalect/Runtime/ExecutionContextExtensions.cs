using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect.Runtime
{
    public static partial class ExecutionContextExtensions
    {
        public static DyTypeInfo QueryType(this ExecutionContext self, string name) =>
            self.Composition.Types.FirstOrDefault(t => t.TypeName == name);

        public static DyTypeInfo QueryType<T>(this ExecutionContext self) where T : DyTypeInfo =>
            self.Composition.Types.FirstOrDefault(t => t is T);
    }
}
