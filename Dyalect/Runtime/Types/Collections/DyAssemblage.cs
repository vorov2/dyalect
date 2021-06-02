using System;

namespace Dyalect.Runtime.Types
{
    internal sealed  class DyAssemblage : DyObject
    {
        private readonly DyLabel[] keys;
        internal readonly DyObject[] Values;

        public DyAssemblage(DyObject[] values, DyLabel[] keys) : base(DyType.Assemblage) =>
            (this.keys, Values) = (keys, values);

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < keys.Length; i++)
                if (keys[i].Label == name)
                    return i;

            return -1;
        }

        public bool IsReadOnly(int index) => !keys[index].Mutable;

        internal override void SetPrivate(ExecutionContext ctx, string name, DyObject value)
        {
            var idx = GetOrdinal(name);

            if (idx is -1)
            {
                ctx.FieldNotFound();
                return;
            }

            Values[idx] = value;
        }

        public override int GetHashCode() => HashCode.Combine(keys, Values);

        public override object ToObject() => this;
    }
}
