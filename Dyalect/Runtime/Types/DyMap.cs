using Dyalect.Compiler;
using Dyalect.Debug;
using System;
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
                this.version = obj.version;
                this.enumerator = obj.Map.GetEnumerator();
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

            public bool MoveNext()
            {
                if (version != obj.version)
                    throw new DyIterator.IterationException();

                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }

        internal sealed class Enumerable : IEnumerable<DyObject>
        {
            private readonly DyMap obj;

            public Enumerable(DyMap obj)
            {
                this.obj = obj;
            }

            public IEnumerator<DyObject> GetEnumerator() => new Enumerator(obj);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal readonly Dictionary<DyObject,DyObject> Map;
        private int version;

        public int Count => Map.Count;

        public DyObject this[DyObject key]
        {
            get { return Map[key]; }
            set { Map[key] = value; }
        }

        internal DyMap() : base(DyType.Map)
        {
            this.Map = new Dictionary<DyObject, DyObject>();
        }

        internal DyMap(IDictionary<DyObject, DyObject> dict) : base(DyType.Map)
        {
            this.Map = new Dictionary<DyObject, DyObject>(dict);
        }

        public void Add(DyObject key, DyObject value) => Map.Add(key, value);

        public bool Remove(DyObject key) => Map.Remove(key);

        public bool ContainsKey(DyObject key) => Map.ContainsKey(key);

        public void Clear() => Map.Clear();

        public override object ToObject() => Map;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (!Map.TryGetValue(index, out var value))
                return ctx.KeyNotFound(index);
            else
                return value;
        }

        internal protected override DyObject GetItem(int index, ExecutionContext ctx) => GetItem(DyInteger.Get(index), ctx);

        internal protected override void SetItem(int index, DyObject obj, ExecutionContext ctx) =>
            SetItem(DyInteger.Get(index), obj, ctx);

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (!Map.TryAdd(index, value))
                ctx.KeyAlreadyPresent(index);
        }

        public IEnumerator<DyObject> GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal override int GetCount() => Map.Count;
    }

    internal sealed class DyMapTypeInfo : DyTypeInfo
    {
        public DyMapTypeInfo() : base(DyType.Map)
        {

        }

        public override string TypeName => DyTypeNames.Map;

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get | SupportedOperations.Set;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyMap)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var map = (DyMap)arg;
            var sb = new StringBuilder();
            sb.Append("Map{");
            var i = 0;

            foreach (var kv in map.Map)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(kv.Key.ToString(ctx) + ": " + kv.Value.ToString(ctx));
                i++;
            }

            sb.Append('}');
            return new DyString(sb.ToString());
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = ((DyMap)self).Map;
            if (!map.TryAdd(key, value))
                return ctx.KeyAlreadyPresent(key);
            return DyNil.Instance;
        }

        private DyObject TryAddItem(ExecutionContext ctx, DyObject self, DyObject key, DyObject value)
        {
            var map = ((DyMap)self).Map;
            if (!map.TryAdd(key, value))
                return DyBool.False;
            return DyBool.True;
        }

        private DyObject TryGetItem(ExecutionContext ctx, DyObject self, DyObject key)
        {
            var map = ((DyMap)self).Map;
            if (!map.TryGetValue(key, out var value))
                return DyNil.Instance;
            return value;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject key)
        {
            return ((DyMap)self).Remove(key) ? DyBool.True : DyBool.False;
        }

        private DyObject ClearItems(ExecutionContext ctx, DyObject self)
        {
            ((DyMap)self).Clear();
            return DyNil.Instance;
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Len:
                    return DyForeignFunction.Member(name, Length);
                case "add":
                    return DyForeignFunction.Member(name, AddItem, -1, new Par("key"), new Par("value"));
                case "tryAdd":
                    return DyForeignFunction.Member(name, TryAddItem, -1, new Par("key"), new Par("value"));
                case "tryGet":
                    return DyForeignFunction.Member(name, TryGetItem, -1, new Par("key"));
                case "remove":
                    return DyForeignFunction.Member(name, RemoveItem, -1, new Par("key"));
                case "clear":
                    return DyForeignFunction.Member(name, ClearItems);
                default:
                    return null;
            }
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

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Map")
                return DyForeignFunction.Static(name, New, -1, new Par("values", DyNil.Instance));

            return null;
        }
    }
}
