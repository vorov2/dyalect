using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DySet : DyEnumerable
    {
        private readonly HashSet<DyObject> set;

        public DySet() : base(DyType.Set) => set = new();

        public override IEnumerator<DyObject> GetEnumerator() =>
            set.GetEnumerator();

        public override object ToObject() => set;
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                var c = set.Count;

                foreach (var v in set)
                    hash = hash * 31 + v.GetHashCode();

                return hash;
            }
        }

        public int Count => set.Count;

        public bool Add(DyObject value) => set.Add(value);

        public bool Remove(DyObject value) => set.Remove(value);

        public bool Contains(DyObject value) => set.Contains(value);

        public void Clear() => set.Clear();

        private DyObject[] InternalToArray()
        {
            var arr = new DyObject[set.Count];
            var count = 0;
            
            foreach (var v in set)
                arr[count++] = v;

            return arr;
        }
        
        public DyArray ToArray() => new(InternalToArray());

        public DyTuple ToTuple() => new(InternalToArray());

        public void IntersectWith(ExecutionContext ctx, DyObject other)
        {
            var seq = other.TypeId is DyType.Set
                ? ((DySet)other).set
                : DyIterator.ToEnumerable(ctx, other);

            if (ctx.HasErrors)
                return;
            
            set.IntersectWith(seq);
        }

        public void UnionWith(ExecutionContext ctx, DyObject other)
        {
            var seq = other.TypeId is DyType.Set
                ? ((DySet)other).set
                : DyIterator.ToEnumerable(ctx, other);

            if (ctx.HasErrors)
                return;
            
            set.UnionWith(seq);
        }

        public void ExceptWith(ExecutionContext ctx, DyObject other)
        {
            var seq = other.TypeId is DyType.Set
                ? ((DySet)other).set
                : DyIterator.ToEnumerable(ctx, other);

            if (ctx.HasErrors)
                return;
            
            set.ExceptWith(seq);
        }

        public bool Overlaps(ExecutionContext ctx, DyObject other)
        {
            var seq = other.TypeId is DyType.Set
                ? ((DySet)other).set
                : DyIterator.ToEnumerable(ctx, other);

            if (ctx.HasErrors)
                return false;
            
            return set.Overlaps(seq);
        }
    }
}