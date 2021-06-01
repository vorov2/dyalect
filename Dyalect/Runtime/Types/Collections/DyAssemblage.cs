using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Runtime.Types
{
    internal sealed  class DyAssemblage : DyTuple
    {
        private readonly DyLabel[] keys;

        public DyAssemblage(DyObject[] values, DyLabel[] keys) : base(values) =>
            this.keys = keys;

        public override int GetOrdinal(string name)
        {
            for (var i = 0; i < keys.Length; i++)
                if (keys[i].Label == name)
                    return i;

            return -1;
        }

        public override bool IsReadOnly(int index) => !keys[index].Mutable;

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) => Values[index];

        internal override string GetKey(int index) => keys[index].Label;

        internal override void SetValue(int index, DyObject value) => Values[index] = value;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            var ki = keys[index];

            if (!ki.Mutable)
            {
                ctx.FieldReadOnly();
                return;
            }

            if (ki.TypeAnnotation is not null && ki.TypeAnnotation.Value != value.TypeId)
            {
                ctx.InvalidType(value);
                return;
            }

            Values[index] = value;
        }

        internal override DyLabel? GetKeyInfo(int index) => keys[index];
    }
}
