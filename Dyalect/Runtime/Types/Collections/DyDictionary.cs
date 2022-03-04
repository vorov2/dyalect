using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyDictionary : DyEnumerable
    {
        internal static readonly DyDictionaryTypeInfo Type = new();
        internal readonly Dictionary<DyObject,DyObject> Map;

        public override int Count => Map.Count;

        public DyObject this[DyObject key]
        {
            get => Map[key];
            set => Map[key] = value;
        }

        internal DyDictionary() : base(Type) =>
            Map = new Dictionary<DyObject, DyObject>();

        internal DyDictionary(Dictionary<DyObject, DyObject> dict) : base(Type) =>
            Map = dict;

        public void Add(DyObject key, DyObject value)
        {
            Version++;
            Map.Add(key, value);
        }

        public bool TryAdd(DyObject key, DyObject value)
        {
            Version++;
            return Map.TryAdd(key, value);
        }

        public bool TryGet(DyObject key, out DyObject? value) =>
            Map.TryGetValue(key, out value);

        public bool Remove(DyObject key)
        {
            Version++;
            return Map.Remove(key);
        }

        public bool ContainsKey(DyObject key) => Map.ContainsKey(key);

        public void Clear()
        {
            Version++;
            Map.Clear();
        }

        public override object ToObject() => Map;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (!Map.TryGetValue(index, out var value))
                return ctx.KeyNotFound();
            else
                return value;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (!Map.TryAdd(index, value))
                Map[index] = value;
            else
                Version++;
        }

        public override IEnumerator<DyObject> GetEnumerator() => new DyDictionaryEnumerator(this);

        public override int GetHashCode() => Map.GetHashCode();

        protected internal override bool HasItem(string name, ExecutionContext ctx) => ContainsKey((DyString)name);
    }
}
