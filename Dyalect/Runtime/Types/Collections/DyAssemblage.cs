using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyAssemblage : DyObject
    {
        internal readonly DyObject[] Values;
        private readonly DyLabel[] keys;

        public int Count => Values.Length;
        
        public DyAssemblage(DyObject[] locals, DyLabel[] keys) : base(DyType.Object) =>
            (Values, this.keys) = (locals, keys);

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < keys.Length; i++)
                if (keys[i].Label == name)
                    return i;

            return -1;
        }

        public bool IsReadOnly(int index) => !keys[index].Mutable;

        public override object ToObject() => throw new NotSupportedException();

        public override int GetHashCode() => throw new NotSupportedException();
    }
}