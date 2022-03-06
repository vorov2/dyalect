using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyWrapper : DyObject
    {
        private readonly Dictionary<string, DyObject> map;

        public DyWrapper(DyWrapperTypeInfo typeInfo, params ValueTuple<string, object>[] fields) : base(typeInfo)
        {
            map = new();

            foreach (var (fld, val) in fields)
                map[fld] = TypeConverter.ConvertFrom(val);
        }

        public DyWrapper(DyWrapperTypeInfo typeInfo, IDictionary<string, object> dict) : base(typeInfo)
        {
            map = new();

            foreach (var (fld, val) in dict)
                this.map[fld] = TypeConverter.ConvertFrom(val);
        }

        internal DyWrapper(DyWrapperTypeInfo typeInfo, Dictionary<string, DyObject> map) : base(typeInfo)
        {
            this.map = map;
        }

        public override object ToObject() => map;

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.DecType.TypeCode != DyTypeCode.String)
                return ctx.InvalidType(index);

            if (!map.TryGetValue(index.GetString(), out var value))
                return ctx.IndexOutOfRange();

            return value;
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            map.ContainsKey(name);

        public override int GetHashCode() => map.GetHashCode();
    }
}
