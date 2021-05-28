using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyAssemblage : DyObject
    {
        internal readonly DyObject[] Values;
        private readonly string[] keys;
        private readonly int[] readOnly;

        public int Count => Values.Length;
        
        public DyAssemblage(DyObject[] locals, string[] keys, int[] readOnly) : base(DyType.Object) =>
            (Values, this.keys, this.readOnly) = (locals, keys, readOnly);

        public int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;

            return -1;
        }

        public override object ToObject() => throw new NotSupportedException();

        public override int GetHashCode() => throw new NotSupportedException();
    }
}