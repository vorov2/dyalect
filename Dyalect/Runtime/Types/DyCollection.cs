using System.Collections;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyCollection : DyObject, IEnumerable<DyObject>
    {
        internal DyCollection(int typeId) : base(typeId)
        {

        }

        public abstract IEnumerator<DyObject> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal abstract DyObject GetValue(int index);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                var c = GetCount();

                for (var i = 0; i < c; i++)
                {
                    var v = GetValue(i);
                    hash = hash * 31 + v.GetHashCode();
                }

                return hash;
            }
        }

        public override bool Equals(DyObject other)
        {
            if (!(other is DyCollection arr))
                return false;

            var c = GetCount();

            if (arr.GetCount() != c)
                return false;

            for (var i = 0; i < c; i++)
                if (!GetValue(i).Equals(arr.GetValue(i)))
                    return false;

            return true;
        }
    }
}
