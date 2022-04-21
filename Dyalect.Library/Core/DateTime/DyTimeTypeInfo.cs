using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        public override string TypeName => "Time";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Gt | SupportedOperations.Gte | SupportedOperations.Lt
            | SupportedOperations.Lte;

        public DyTimeTypeInfo() => AddMixin(DyType.Comparable);
    }
}
