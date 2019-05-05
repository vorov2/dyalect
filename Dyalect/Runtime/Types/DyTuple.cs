using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public abstract class DyTuple : DyObject, IEnumerable<DyObject>
    {
        public static DyTuple Create(DyObject arg1, DyObject arg2) =>
            new DyTuplePair(null, arg1, null, arg2 );
        public static DyTuple Create(string key1, DyObject arg1, string key2, DyObject arg2) =>
            new DyTuplePair(key1, arg1, key2, arg2);
        public static DyTuple Create(string key1, DyObject arg1, string key2, DyObject arg2, string key3, DyObject arg3) =>
            new DyTupleTriple(key1, arg1, key2, arg2, key3, arg3);
        public static DyTuple Create(string[] keys, DyObject[] args) =>
            new DyTupleVariadic(keys, args);
        public static DyTuple Create(DyObject[] args) =>
            new DyTupleVariadic(new string[args.Length], args);

        protected DyTuple() : base(StandardType.Tuple)
        {

        }

        public override object ToObject() => ToDictionary();

        public abstract IDictionary<string, object> ToDictionary();

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.GetInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else if (index.TypeId == StandardType.String)
                return GetItem(index.GetString(), ctx) ?? Err.IndexOutOfRange(this.TypeName(ctx), index.GetString()).Set(ctx);
            else
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                SetItem((int)index.GetInteger(), value, ctx);
            else if (index.TypeId == StandardType.String)
                SetItem(index.GetString(), value, ctx);
            else
                Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        protected internal override DyObject GetItem(string name, ExecutionContext ctx) => GetItem(GetOrdinal(name));

        protected internal override void SetItem(string name, DyObject value, ExecutionContext ctx) =>
            SetItem(GetOrdinal(name), value, ctx);

        protected internal abstract void SetItem(int index, DyObject value, ExecutionContext ctx);

        protected internal abstract int GetOrdinal(string name);

        protected internal abstract DyObject GetItem(int index);

        protected internal abstract string GetKey(int index);

        protected string DefaultKey() => Guid.NewGuid().ToString();

        public IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return GetItem(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public abstract int Count { get; }
    }

    internal sealed class DyTuplePair : DyTuple
    {
        private readonly string key1;
        private readonly string key2;
        private DyObject value1;
        private DyObject value2;

        public override int Count => 2;

        public DyTuplePair(string key1, DyObject value1, string key2, DyObject value2)
        {
            this.key1 = key1;
            this.key2 = key2;
            this.value1 = value1;
            this.value2 = value2;
        }

        public override IDictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict.Add(key1 ?? DefaultKey(), value1);
            dict.Add(key2 ?? DefaultKey(), value2);
            return dict;
        }

        protected internal override DyObject GetItem(int index)
        {
            if (index == 0)
                return value1;
            if (index == 1)
                return value2;
            return null;
        }

        protected internal override int GetOrdinal(string name)
        {
            if (name == key1)
                return 0;
            if (name == key2)
                return 1;
            return -1;
        }

        protected internal override string GetKey(int index)
        {
            if (index == 0)
                return key1;
            return key2;
        }

        protected internal override void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (index == 0)
                value1 = value;
            else if (index == 1)
                value2 = value;
            else
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
        }
    }

    internal sealed class DyTupleTriple : DyTuple
    {
        private readonly string key1;
        private readonly string key2;
        private readonly string key3;
        private DyObject value1;
        private DyObject value2;
        private DyObject value3;

        public override int Count => 3;

        public DyTupleTriple(string key1, DyObject value1, string key2, DyObject value2, string key3, DyObject value3)
        {
            this.key1 = key1;
            this.key2 = key2;
            this.key3 = key3;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
        }

        public override IDictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict.Add(key1 ?? DefaultKey(), value1);
            dict.Add(key2 ?? DefaultKey(), value2);
            dict.Add(key3 ?? DefaultKey(), value3);
            return dict;
        }

        protected internal override DyObject GetItem(int index)
        {
            if (index == 0)
                return value1;
            if (index == 1)
                return value2;
            if (index == 2)
                return value3;
            return null;
        }

        protected internal override int GetOrdinal(string name)
        {
            if (name == key1)
                return 0;
            if (name == key2)
                return 1;
            if (name == key3)
                return 2;
            return -1;
        }

        protected internal override string GetKey(int index)
        {
            if (index == 0)
                return key1;
            if (index == 1)
                return key2;
            return key3;
        }

        protected internal override void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (index == 0)
                value1 = value;
            else if (index == 1)
                value2 = value;
            else if (index == 2)
                value3 = value;

            else
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
        }
    }

    internal sealed class DyTupleVariadic : DyTuple
    {
        private readonly DyObject[] values;
        private readonly string[] keys;

        public override int Count => keys.Length;

        internal DyTupleVariadic(string[] keys, DyObject[] values)
        {
            this.keys = keys;
            this.values = values;
        }

        public override IDictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();

            for (var i = 0; i < keys.Length; i++)
            {
                var k = keys[i] ?? Guid.NewGuid().ToString();

                try
                {
                    dict.Add(k, values[i].ToObject());
                }
                catch
                {
                    dict.Add(Guid.NewGuid().ToString(), values[i].ToObject());
                }
            }

            return dict;
        }

        protected internal override int GetOrdinal(string name) => Array.IndexOf(keys, name);

        protected internal override DyObject GetItem(int index)
        {
            if (index < 0 || index >= values.Length)
                return null;
            return values[index];
        }

        protected internal override string GetKey(int index) => keys[index];

        protected internal override void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (index < 0 || index >= values.Length)
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                values[index] = value;
        }
    }

    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public static readonly DyTupleTypeInfo Instance = new DyTupleTypeInfo();

        private DyTupleTypeInfo() : base(StandardType.Tuple, true)
        {

        }

        public override string TypeName => StandardType.TupleName;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var tup = (DyTuple)arg;
            var sb = new StringBuilder();
            sb.Append('(');

            for (var i = 0; i < tup.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                var k = tup.GetKey(i);
                var val = tup.GetItem(i).ToString(ctx);

                if (ctx.Error != null)
                    return DyString.Empty;

                if (k != null)
                {
                    sb.Append(k);
                    sb.Append(": ");
                }

                sb.Append(val.GetString());
            }

            sb.Append(')');
            return new DyString(sb.ToString());
        }

        private DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var tup = (DyTuple)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < tup.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(iterate().GetEnumerator());
        }

        private DyObject GetKeys(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var tup = (DyTuple)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < tup.Count; i++)
                {
                    var k = tup.GetKey(i);

                    if (k != null)
                        yield return new DyString(k);
                }
            }

            return new DyIterator(iterate().GetEnumerator());
        }

        private DyObject GetFirst(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return ((DyTuple)self).GetItem(0);
        }

        private DyObject GetSecond(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return ((DyTuple)self).GetItem(1);
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Create(name, LenAdapter);

            if (name == "indices")
                return DyForeignFunction.Create(name, GetIndices);

            if (name == "keys")
                return DyForeignFunction.Create(name, GetKeys);

            if (name == "fst")
                return DyForeignFunction.Create(name, GetFirst);

            if (name == "snd")
                return DyForeignFunction.Create(name, GetSecond);

            return null;
        }
    }
}
