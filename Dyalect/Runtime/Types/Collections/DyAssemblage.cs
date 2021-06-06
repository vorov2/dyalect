using System;

namespace Dyalect.Runtime.Types
{
    internal sealed  class DyAssemblage : DyObject
    {
        internal readonly DyLabel[] Keys;
        internal readonly DyObject[] Values;

        public DyAssemblage(DyObject[] values, DyLabel[] keys) : base(DyType.Assemblage) =>
            (this.Keys, Values) = (keys, values);

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < Keys.Length; i++)
                if (Keys[i].Label == name)
                    return i;

            return -1;
        }

        public bool IsReadOnly(int index) => !Keys[index].Mutable;

        public override int GetHashCode() => HashCode.Combine(Keys, Values);

        public override object ToObject() => this;
    }
}
