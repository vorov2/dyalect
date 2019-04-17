using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyObject
    {
        internal readonly DyObject[] Values;
        internal readonly string[] Keys;

        internal DyTuple(string[] keys, DyObject[] values) : base(StandardType.Tuple)
        {
            Keys = keys;
            Values = values;
        }

        public override object ToObject() => ToDictionary();

        public IDictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();

            for (var i = 0; i < Keys.Length; i++)
            {
                var k = Keys[i] ?? Guid.NewGuid().ToString();

                try
                {
                    dict.Add(k, Values[i].ToObject());
                }
                catch
                {
                    dict.Add(Guid.NewGuid().ToString(), Values[i].ToObject());
                }
            }

            return dict;
        }

        protected override bool TestEquality(DyObject obj)
        {
            var t = (DyTuple)obj;

            if (Keys.Length != t.Keys.Length)
                return false;

            for (var i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] != t.Keys[i]
                    || Values[i].Equals(t.Values[i]))
                    return false;
            }

            return true;
        }

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.GetInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else if (index.TypeId == StandardType.String)
                return GetItem(index.GetString()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        private int GetOrdinal(string name) => Array.IndexOf(Keys, name);

        private DyObject GetItem(int index)
        {
            if (index < 0 || index >= Values.Length)
                return null;
            return Values[index];
        }

        private DyObject GetItem(string index)
        {
            return GetItem(GetOrdinal(index));
        }
    }

    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public static readonly DyTupleTypeInfo Instance = new DyTupleTypeInfo();

        private DyTupleTypeInfo() : base(StandardType.Tuple)
        {

        }

        public override string TypeName => StandardType.TupleName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) => new DyTuple(new string[args.Length], args);

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Keys.Length;
            return len == 1 ? DyInteger.One
                : len == 2 ? DyInteger.Two
                : len == 3 ? DyInteger.Three
                : new DyInteger(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var tup = (DyTuple)arg;
            var sb = new StringBuilder();
            sb.Append('(');

            for (var i = 0; i < tup.Keys.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                var k = tup.Keys[i];
                var val = tup.Values[i].ToString(ctx);

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
    }
}
