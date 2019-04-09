using System.Collections.Generic;

namespace Dyalect.Parser
{
    internal sealed class Map
    {
        private Dictionary<int, int> map;

        public Map(int capacity)
        {
            this.map = new Dictionary<int, int>(capacity);
        }

        public bool ContainsKey(int key)
        {
            return map.ContainsKey(key);
        }

        public int this[int key]
        {
            get
            {
                return map[key];
            }
            set
            {
                if (map.ContainsKey(key))
                    map[key] = value;
                else
                    map.Add(key, value);
            }
        }
    }
}
