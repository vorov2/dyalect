﻿using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyWrapper : DyObject
    {
        private readonly Dictionary<string, DyObject> map;

        public DyWrapper(params ValueTuple<string, object>[] fields) : base(DyType.Object)
        {
            map = new();

            foreach (var (fld, val) in fields)
                map[fld] = TypeConverter.ConvertFrom(val);
        }

        public DyWrapper(IDictionary<string, object> dict) : base(DyType.Object)
        {
            map = new();

            foreach (var (fld, val) in dict)
                this.map[fld] = TypeConverter.ConvertFrom(val);
        }

        internal DyWrapper(Dictionary<string, DyObject> map) : base(DyType.Object) => this.map = map;

        public override object ToObject() => map;

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.String)
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
