using Dyalect.Debug;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyMap : DyObject, IEnumerable<DyObject>
    {
        internal sealed class Enumerator : IEnumerator<DyObject>
        {
            private readonly DyMap obj;
            private readonly IEnumerator enumerator;
            private readonly int version;

            public Enumerator(DyMap obj)
            {
                this.obj = obj;
                version = obj.version;
                enumerator = obj.Map.GetEnumerator();
            }

            public DyObject Current
            {
                get
                {
                    var obj = (KeyValuePair<DyObject, DyObject>)enumerator.Current;
                    return new DyTuple(new DyObject[] {
                        new DyLabel("key", obj.Key),
                        new DyLabel("value", obj.Value)
                        });
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() =>
                version != obj.version ? throw new IterationException() : enumerator.MoveNext();

            public void Reset() => enumerator.Reset();
        }

        internal sealed class Enumerable : IEnumerable<DyObject>
        {
            private readonly DyMap obj;

            public Enumerable(DyMap obj) => this.obj = obj;

            public IEnumerator<DyObject> GetEnumerator() => new Enumerator(obj);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal readonly Dictionary<DyObject,DyObject> Map;
        private int version;

        public int Count => Map.Count;

        public DyObject this[DyObject key]
        {
            get => Map[key];
            set => Map[key] = value;
        }

        internal DyMap() : base(DyType.Map) =>
            Map = new Dictionary<DyObject, DyObject>();

        internal DyMap(Dictionary<DyObject, DyObject> dict) : base(DyType.Map) =>
            Map = dict;

        public void Add(DyObject key, DyObject value)
        {
            version++;
            Map.Add(key, value);
        }

        public bool TryAdd(DyObject key, DyObject value)
        {
            version++;
            return Map.TryAdd(key, value);
        }

        public bool TryGet(DyObject key, out DyObject? value) =>
            Map.TryGetValue(key, out value);

        public bool Remove(DyObject key)
        {
            version++;
            return Map.Remove(key);
        }

        public bool ContainsKey(DyObject key) => Map.ContainsKey(key);

        public void Clear()
        {
            version++;
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
                version++;
        }

        public IEnumerator<DyObject> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override int GetHashCode() => Map.GetHashCode();
    }

    internal sealed class DyMapTypeInfo : DyTypeInfo
    {
        public DyMapTypeInfo() : base(DyType.Map) { }

        public override string TypeName => DyTypeNames.Map;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set | SupportedOperations.Len;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyMap)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var map = (DyMap)arg;
            var sb = new StringBuilder();
            sb.Append("Map {");
            var i = 0;

            foreach (var kv in map.Map)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(kv.Key.ToString(ctx) + " = " + kv.Value.ToString(ctx));
                i++;
            }

            sb.Append('}');
            return new DyString(sb.ToString());
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyMap)self;
            if (!map.TryAdd(key, value))
                return ctx.KeyAlreadyPresent();
            return DyNil.Instance;
        }

        private DyObject TryAddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = (DyMap)self;
            if (!map.TryAdd(key, value))
                return DyBool.False;
            return DyBool.True;
        }

        private DyObject TryGetItem(ExecutionContext ctx, DyObject self, DyObject key)
        {
            var map = (DyMap)self;
            if (!map.TryGet(key, out var value))
                return DyNil.Instance;
            return value!;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject key) =>
            ((DyMap)self).Remove(key) ? DyBool.True : DyBool.False;

        private DyObject ClearItems(ExecutionContext ctx, DyObject self)
        {
            ((DyMap)self).Clear();
            return DyNil.Instance;
        }

        protected override DyObject? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            return name switch
            {
                "add" => DyForeignFunction.Member(name, AddItem, -1, new Par("key"), new Par("value")),
                "tryAdd" => DyForeignFunction.Member(name, TryAddItem, -1, new Par("key"), new Par("value")),
                "tryGet" => DyForeignFunction.Member(name, TryGetItem, -1, new Par("key")),
                "remove" => DyForeignFunction.Member(name, RemoveItem, -1, new Par("key")),
                "clear" => DyForeignFunction.Member(name, ClearItems),
                _ => base.InitializeInstanceMember(self, name, ctx),
            };
        }

        private DyObject New(ExecutionContext ctx, DyObject values)
        {
            if (values == DyNil.Instance)
                return new DyMap();
            else if (values is DyTuple tup)
                return new DyMap(tup.ConvertToDictionary());
            else
                return ctx.InvalidType(values);
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Map")
                return DyForeignFunction.Static(name, New, -1, new Par("values", DyNil.Instance));
            else if (name == "fromTuple")
                return DyForeignFunction.Static(name, New, -1, new Par("values"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
