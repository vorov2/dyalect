using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection
    {
        public static readonly DyTuple Empty = new(Array.Empty<DyObject>());

        private readonly int length;
        private readonly bool? mutable;
        private readonly DyObject[] values;

        public override int Count => length;

        public DyTuple(DyObject[] values) : this(values, values.Length) { }

        internal DyTuple(DyObject[] values, bool mutable) : this(values, values.Length) =>
            this.mutable = mutable;

        public DyTuple(DyObject[] values, int length) : base(DyType.Tuple)
        {
            this.length = length;
            this.values = values ?? throw new DyException("Unable to create a tuple with no values.");
        }

        public static DyTuple Create(params DyLabel[] labels) => new DyTuple(labels, labels.Length);

        public Dictionary<DyObject, DyObject> ConvertToDictionary(ExecutionContext ctx)
        {
            var dict = new Dictionary<DyObject, DyObject>();

            for (var i = 0; i < Count; i++)
            {
                var ki = GetKeyInfo(i);
                var v = GetValue(i);
                var key = new DyString(ki is null ? DefaultKey() : ki.Label);
                dict[key] = v;
            }

            return dict;
        }

        internal bool TryGetItem(string name, ExecutionContext ctx, out DyObject item)
        {
            item = null!;
            var i = GetOrdinal(ctx, name);

            if (i is -1)
                return false;

            item = GetItem(i, ctx);
            return true;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);

            if (index.TypeId != DyType.String && index.TypeId != DyType.Char)
                return ctx.IndexOutOfRange(index);
            
            return TryGetItem(index.GetString(), ctx, out var item)
                ? item : ctx.IndexOutOfRange(index);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.String)
            {
                var i = GetOrdinal(ctx, index.GetString());

                if (i is -1)
                {
                    ctx.IndexOutOfRange(index);
                    return;
                }

                CollectionSetItem(i, value, ctx);
            }
            else
                base.SetItem(index, value, ctx);
        }

        public virtual int GetOrdinal(ExecutionContext ctx, string name)
        {
            for (var i = 0; i < Count; i++)
                if (values[i].GetLabel() == name)
                    return i;

            return -1;
        }

        public virtual bool IsReadOnly(int index) => values[index] is DyLabel lab && !lab.Mutable;

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
            values[index].TypeId == DyType.Label ? values[index].GetTaggedValue() : values[index];

        internal virtual string GetKey(int index) => values[index].GetLabel()!;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (values[index].TypeId == DyType.Label)
            {
                var lab = (DyLabel)values[index];

                if (!lab.Mutable)
                {
                    ctx.IndexReadOnly(lab.Label);
                    return;
                }

                if (!lab.VerifyType(value.TypeId))
                {
                    ctx.InvalidType(value);
                    return;
                }
                
                lab.Value = value;
            }
            else
                ctx.IndexReadOnly();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            GetOrdinal(ctx, name) is not -1;

        private static string DefaultKey() => Guid.NewGuid().ToString();

        public override IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return GetValue(i);
        }

        internal override DyObject GetValue(int index) => values[index].GetTaggedValue();

        internal virtual void SetValue(int index, DyObject value)
        {
            if (values[index] is DyLabel lab)
                lab.Value = value;
            else
                values[index] = value;
        }

        internal virtual DyLabel? GetKeyInfo(int index) => values[index] is DyLabel lab ? lab : null;

        internal override DyObject[] GetValues()
        {
            if (Count != values.Length)
                return CopyTuple();

            for (var i = 0; i < Count; i++)
                if (values[i].TypeId == DyType.Label)
                    return CopyTuple();

            return values;
        }

        internal DyObject[] GetValuesWithLabels()
        {
            if (mutable != null)
            {
                if (!mutable.Value && Count == values.Length)
                    return values;
                else
                    return CopyTupleWithLabels();
            }

            if (Count != values.Length)
                return CopyTupleWithLabels();

            for (var i = 0; i < Count; i++)
                if (values[i].IsMutable())
                    return CopyTupleWithLabels();

            return values;
        }

        private DyObject[] CopyTuple()
        {
            var arr = new DyObject[Count];

            for (var i = 0; i < Count; i++)
                arr[i] = values[i].GetTaggedValue();

            return arr;
        }
        private DyObject[] CopyTupleWithLabels()
        {
            var arr = new DyObject[Count];

            for (var i = 0; i < Count; i++)
                arr[i] = values[i].MakeImmutable();

            return arr;
        }

        internal DyObject ToString(bool literal, ExecutionContext ctx)
        {
            var sb = new StringBuilder();
            sb.Append('(');

            for (var i = 0; i < Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                    sb.Append(' ');
                }

                var v = GetValue(i);
                var ki = GetKeyInfo(i);

                if (ki is not null)
                {
                    if (ki.Mutable)
                        sb.Append("var ");

                    if (ki.Label.Length > 0 && char.IsLower(ki.Label[0]) && ki.Label.All(char.IsLetter))
                        sb.Append(ki.Label);
                    else
                        sb.Append(StringUtil.Escape(ki.Label));

                    sb.Append(':');
                    sb.Append(' ');
                }

                var str = literal ? v.ToLiteral(ctx) : v.ToString(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                sb.Append(str);
            }

            sb.Append(')');
            return new DyString(sb.ToString());
        }

        internal DyObject[] UnsafeAccessValues() => values;
    }
}
