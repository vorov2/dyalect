using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyCustomObject : DyObject
    {
        private readonly Dictionary<string, DyObject> map;

        public DyCustomObject(params ValueTuple<string, object>[] fields) : base(DyType.Object)
        {
            this.map = new Dictionary<string, DyObject>();

            foreach (var fld in fields)
                map[fld.Item1] = TypeConverter.ConvertFrom(fld.Item2);
        }

        public DyCustomObject(IDictionary<string, object> map) : base(DyType.Object)
        {
            this.map = new Dictionary<string, DyObject>();

            foreach (var kv in map)
                this.map[kv.Key] = TypeConverter.ConvertFrom(kv.Value);
        }

        internal DyCustomObject(Dictionary<string, DyObject> map) : base(DyType.Object)
        {
            this.map = map;
        }

        public override object ToObject() => map;

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            if (!map.TryGetValue(name, out var value))
                return ctx.IndexOutOfRange(name);

            return value;
        }

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
            return map.TryGetValue(name, out value);
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx)
        {
            return map.ContainsKey(name);
        }
    }

    public sealed class DyReflectionObject : DyCustomObject
    {
        public DyReflectionObject(object instance) : base(Convert(instance))
        {

        }

        private static Dictionary<string, DyObject> Convert(object instance)
        {
            var typ = instance.GetType();
            var map = new Dictionary<string, DyObject>();

            foreach (var pi in typ.GetProperties())
            {
                var val = pi.GetValue(instance, null);
                map[pi.Name] = TypeConverter.ConvertFrom(val);
            }

            return map;
        }
    }

    public class DyCustomObjectTypeInfo : DyTypeInfo
    {
        public DyCustomObjectTypeInfo() : base(DyType.Object)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Get;

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.String)
                return ctx.IndexInvalidType(index);
            
            return self.GetItem(index.GetString(), ctx);
        }

        public override string TypeName => DyTypeNames.Object;
    }
}
