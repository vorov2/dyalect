using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public class DyTuple : DyCollection, IEnumerable<DyObject>
    {
        internal static readonly DyTuple Empty = new(Array.Empty<DyObject>());
        private static readonly Dictionary<int, object?>? allMutableFields = new();

        internal readonly DyObject[] Values;
        private Dictionary<int, object?>? mutableFields;

        public override int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(DyType.Tuple) =>
            Values = values ?? throw new DyException("Unable to create a tuple with no values.");

        public static DyTuple Create(params DyObject[] args) => new(args);

        public void SetMutableField(int index)
        {
            if (index == -1)
                mutableFields = allMutableFields;
            else
            {
                mutableFields ??= new();
                mutableFields[index] = null;
            }
        }

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

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            
            if (index.TypeId == DyType.String || index.TypeId == DyType.Char)
            {
                var i = GetOrdinal(index.GetString());

                if (i == -1)
                    return ctx.IndexOutOfRange();

                return GetItem(i, ctx);
            }
            
            return ctx.InvalidType(index);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId is DyType.String)
            {
                var i = GetOrdinal(index.GetString());

                if (i == -1)
                    ctx.IndexOutOfRange();

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
            if (mutableFields is null
                || (!ReferenceEquals(mutableFields, allMutableFields) && !mutableFields.ContainsKey(index)))
            {
                ctx.FieldReadOnly();
                return;
            }

            if (Values[index].TypeId == DyType.Label)
                ((DyLabel)Values[index]).Value = value;
            else
                Values[index] = value;
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
    }
}
