using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyDictionary : DyEnumerable
    {
        internal readonly Dictionary<DyObject, DyObject> Dictionary;

        public override int Count => Dictionary.Count;

        public DyObject this[DyObject key]
        {
            get => Dictionary[key];
            set => Dictionary[key] = value;
        }

        internal DyDictionary() : base(DyType.Dictionary)
        {
            Dictionary = new Dictionary<DyObject, DyObject>();
        }

        internal DyDictionary(Dictionary<DyObject, DyObject> dict) : base(DyType.Dictionary)
        {
            Dictionary = dict;
        }

        public void Add(DyObject key, DyObject value)
        {
            Version++;
            Dictionary.Add(key, value);
        }

        public bool TryAdd(DyObject key, DyObject value)
        {
            Version++;
            return Dictionary.TryAdd(key, value);
        }

        public bool TryGet(DyObject key, out DyObject? value) =>
            Dictionary.TryGetValue(key, out value);

        public DyObject GetAndRemove(DyObject key)
        {
            Dictionary.Remove(key, out var value);
            return value ?? DyNil.Instance;
        }

        public bool Remove(DyObject key)
        {
            Version++;
            return Dictionary.Remove(key);
        }

        public bool ContainsKey(DyObject key) => Dictionary.ContainsKey(key);

        public bool ContainsValue(DyObject value) => Dictionary.ContainsValue(value);

        public void Clear()
        {
            Version++;
            Dictionary.Clear();
        }

        public override object ToObject() => Dictionary;

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (!Dictionary.TryGetValue(index, out var value))
                return ctx.KeyNotFound(index);
            else
                return value;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (!Dictionary.TryAdd(index, value))
                Dictionary[index] = value;
            else
                Version++;
        }

        public override bool Equals(DyObject? other)
        {
            if (other is not DyDictionary d)
                return false;

            return d.Dictionary.Equals(Dictionary);
        }

        public override IEnumerator<DyObject> GetEnumerator() => new DyDictionaryEnumerator(this);

        public override int GetHashCode() => Dictionary.GetHashCode();
    }
}
