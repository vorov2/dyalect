using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection
    {
        internal static readonly DyTuple Empty = new(Array.Empty<DyObject>());
        internal readonly DyObject[] Values;

        public override int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(DyType.Tuple) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");

        public static DyTuple Create(params DyObject[] args) => new(args);

        public Dictionary<DyObject, DyObject> ConvertToDictionary()
        {
            var dict = new Dictionary<DyObject, DyObject>();

            foreach (var obj in Values)
            {
                if (obj is not DyLabel lab || !dict.TryAdd(new DyString(lab.Label), lab.Value))
                    dict.Add(new DyString(DefaultKey()), obj);
            }

            return dict;
        }

        internal bool TryGetItem(string name, ExecutionContext ctx, out DyObject item)
        {
            item = null!;
            var i = GetOrdinal(name);

            if (i == -1)
                return false;

            item = GetItem(i, ctx);
            return true;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);

            if (index.TypeId is not DyType.String and not DyType.Char)
                return ctx.InvalidType(index);
            
            return TryGetItem(index.GetString(), ctx, out var item)
                ? item : ctx.IndexOutOfRange();
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.String)
            {
                var i = GetOrdinal(index.GetString());

                if (i == -1)
                {
                    ctx.IndexOutOfRange();
                    return;
                }

                CollectionSetItem(i, value, ctx);
            }
            else
                base.SetItem(index, value, ctx);
        }

        private int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;
            return -1;
        }

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
            Values[index].TypeId == DyType.Label ? Values[index].GetTaggedValue() : Values[index];

        internal string GetKey(int index) => Values[index].GetLabel()!;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (Values[index].TypeId == DyType.Label)
            {
                var lab = (DyLabel)Values[index];

                if (!lab.Mutable)
                {
                    ctx.FieldReadOnly();
                    return;
                }
                
                lab.Value = value;
            }
            else
                ctx.FieldReadOnly();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            GetOrdinal(name) != -1;

        private static string DefaultKey() => Guid.NewGuid().ToString();

        public override IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return Values[i].TypeId == DyType.Label ? Values[i].GetTaggedValue() : Values[i];
        }

        internal override DyObject GetValue(int index) => Values[index];

        internal override DyObject[] GetValues() => Values;

        internal DyObject ToString(ExecutionContext ctx)
        {
            var sb = new StringBuilder();
            sb.Append('(');

            for (var i = 0; i < Values.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                var v = Values[i];

                if (v is DyLabel lab)
                {
                    if (lab.Mutable)
                        sb.Append("var ");

                    sb.Append(lab.Label);
                    sb.Append(':');
                    sb.Append(' ');
                    v = lab.Value;
                }

                var str = v.ToString(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                sb.Append(str);
            }

            sb.Append(')');
            return new DyString(sb.ToString());
        }
    }
}
