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
        
        internal readonly DyObject[] Values;

        public override int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(DyType.Tuple) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");

        public DyTuple(params DyLabel[] labels) : this((DyObject[])labels) { }

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
                return ctx.InvalidType(DyType.String, index);
            
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
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;

            return -1;
        }

        public virtual bool IsReadOnly(int index) => Values[index] is DyLabel lab && !lab.Mutable;

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
            Values[index].TypeId == DyType.Label ? Values[index].GetTaggedValue() : Values[index];

        internal virtual string GetKey(int index) => Values[index].GetLabel()!;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (Values[index].TypeId == DyType.Label)
            {
                var lab = (DyLabel)Values[index];

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

        internal override DyObject GetValue(int index) => Values[index].GetTaggedValue();

        internal virtual void SetValue(int index, DyObject value)
        {
            if (Values[index] is DyLabel lab)
                lab.Value = value;
            else
                Values[index] = value;
        }

        internal virtual DyLabel? GetKeyInfo(int index) => Values[index] is DyLabel lab ? lab : null;

        internal override DyObject[] GetValues()
        {
            for (var i = 0; i < Count; i++)
                if (Values[i].TypeId == DyType.Label)
                    return ConvertToPlainValues();

            return Values;
        }

        internal DyObject[] ConvertToPlainValues()
        {
            var arr = new DyObject[Count];

            for (var i = 0; i < Count; i++)
                arr[i] = Values[i].GetTaggedValue();

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
    }
}
