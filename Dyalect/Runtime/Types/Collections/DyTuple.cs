using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection
    {
        internal readonly DyObject[] Values;

        public override int Count => Values.Length;

        public DyTuple(DyTypeInfo typeInfo, DyObject[] values) : base(typeInfo) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");

        protected DyTuple(DyTypeInfo typeInfo, DyObject[] values, int typeId) : base(typeInfo) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");

        public static DyTuple Create(ExecutionContext ctx, params DyObject[] args) => 
            new(ctx.RuntimeContext.Tuple, args);

        public Dictionary<DyObject, DyObject> ConvertToDictionary(ExecutionContext ctx)
        {
            var dict = new Dictionary<DyObject, DyObject>();

            for (var i = 0; i < Count; i++)
            {
                var ki = GetKeyInfo(i);
                var v = GetValue(i);
                var key = new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char,
                    ki is null ? DefaultKey() : ki.Label);
                dict[key] = v;
            }

            return dict;
        }

        internal bool TryGetItem(string name, ExecutionContext ctx, out DyObject item)
        {
            item = null!;
            var i = GetOrdinal(name);

            if (i is -1)
                return false;

            item = GetItem(i, ctx);
            return true;
        }

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.DecType.TypeCode == DyTypeCode.Integer)
                return GetItem((int)index.GetInteger(), ctx);

            if (index.DecType.TypeCode != DyTypeCode.String && index.DecType.TypeCode != DyTypeCode.Char)
                return ctx.InvalidType(index);
            
            return TryGetItem(index.GetString(), ctx, out var item)
                ? item : ctx.FieldNotFound();
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.DecType.TypeCode == DyTypeCode.String)
            {
                var i = GetOrdinal(index.GetString());

                if (i is -1)
                {
                    ctx.FieldNotFound();
                    return;
                }

                CollectionSetItem(i, value, ctx);
            }
            else
                base.SetItem(index, value, ctx);
        }

        public virtual int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;

            return -1;
        }

        public virtual bool IsReadOnly(int index) => Values[index] is DyLabel lab && !lab.Mutable;

        protected override DyObject CollectionGetItem(int index, ExecutionContext ctx) =>
            Values[index].DecType.TypeCode == DyTypeCode.Label ? Values[index].GetTaggedValue() : Values[index];

        internal virtual string GetKey(int index) => Values[index].GetLabel()!;

        protected override void CollectionSetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (Values[index].DecType.TypeCode == DyTypeCode.Integer)
            {
                var lab = (DyLabel)Values[index];

                if (!lab.Mutable)
                {
                    ctx.FieldReadOnly();
                    return;
                }

                if (lab.TypeAnnotation is not null && ReferenceEquals(value.DecType, lab.TypeAnnotation))
                {
                    ctx.InvalidType(value);
                    return;
                }
                
                lab.Value = value;
            }
            else
                ctx.FieldReadOnly();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            GetOrdinal(name) is not -1;

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
                if (Values[i].DecType.TypeCode == DyTypeCode.Label)
                    return ConvertToPlainValues();

            return Values;
        }

        private DyObject[] ConvertToPlainValues()
        {
            var arr = new DyObject[Count];

            for (var i = 0; i < Count; i++)
                arr[i] = Values[i].GetTaggedValue();

            return arr;
        }

        internal DyObject ToString(ExecutionContext ctx)
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

                    sb.Append(ki.Label);
                    sb.Append(':');
                    sb.Append(' ');
                }

                var str = v.ToString(ctx);

                if (ctx.HasErrors)
                    return ctx.RuntimeContext.Nil.Instance;

                sb.Append(str);
            }

            sb.Append(')');
            return new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, sb.ToString());
        }
    }
}
