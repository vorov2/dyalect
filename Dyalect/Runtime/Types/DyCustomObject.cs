using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    public class DyCustomObject : DyObject
    {
        public DyCustomObject(params ValueTuple<string, DyObject>[] fields) : base(DyType.Object)
        {

        }

        public override object ToObject() => this;

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            return base.GetItem(name, ctx);
        }

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            return base.TryGetItem(name, ctx, out value);
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx)
        {
            return base.HasItem(name, ctx);
        }
    }

    public class DyCustomObjectTypeInfo : DyTypeInfo
    {
        public DyCustomObjectTypeInfo() : base(DyType.Object)
        {

        }
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Object;
    }
}
