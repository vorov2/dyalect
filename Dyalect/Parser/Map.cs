using System.Collections.Generic;

namespace Dyalect.Parser
{
    internal sealed class Map
    {
        private readonly Dictionary<int, int> map;

        public Map(int capacity) => map = new(capacity);

        public bool ContainsKey(int key) => map.ContainsKey(key);

        public int this[int key]
        {
            get => map[key];
            set => map[key] = value;
        }
    }
}
