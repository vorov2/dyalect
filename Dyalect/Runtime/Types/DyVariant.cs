using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    public sealed class DyVariant : DyObject
    {
        internal readonly string Constructor;
        internal readonly DyTuple Tuple;

        public DyVariant(string constructor, DyTuple values) : base(DyType.Variant) =>
            (Constructor, Tuple) = (constructor, values);

        public override string GetConstructor(ExecutionContext _) => Constructor;

        public override int GetHashCode() => Constructor.GetHashCode();

        public override object ToObject() => Tuple.ToObject();
    }
}
