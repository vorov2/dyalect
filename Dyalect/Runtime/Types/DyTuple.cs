using Dyalect.Compiler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public sealed class DyTuple : DyObject, IEnumerable<DyObject>
    {
        internal readonly DyObject[] Values;

        public int Count => Values.Length;

        public DyTuple(DyObject[] values) : base(StandardType.Tuple)
        {
            if (values == null)
                throw new DyException("Unable to create a tuple with no values.");

            this.Values = values;
        }

        public override object ToObject() => ConvertToArray();

        public IList<object> ConvertToList() => Values.Select(e => e.ToObject()).ToList();

        public object[] ConvertToArray() => Values.Select(e => e.ToObject()).ToArray();

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

        internal int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;
            return -1;
        }

        internal DyObject GetItem(int index)
        {
            if (index < 0 || index >= Values.Length)
                return null;
            return Values[index].TypeId == StandardType.Label ? Values[index].GetTaggedValue() : Values[index];
        }

        internal string GetKey(int index) => Values[index].GetLabel();

        internal void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (index < 0 || index >= Values.Length)
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
            {
                if (Values[index].TypeId == StandardType.Label)
                    ((DyLabel)Values[index]).Value = value;
                else
                    Values[index] = value;
            }
        }

        private string DefaultKey() => Guid.NewGuid().ToString();

        public IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return GetItem(i);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public DyTupleTypeInfo() : base(StandardType.Tuple, true)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => StandardType.TupleName;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyTuple)arg).Count;
            return DyInteger.Get(len);
        }

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx)
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
            var tup = (DyTuple)self;

            if (tup.Values.Length < 2)
                Err.IndexOutOfRange(TypeName, 1).Set(ctx);
            
            return tup.GetItem(1);
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Member(name, Length);

            if (name == "indices")
                return DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters);

            if (name == "keys")
                return DyForeignFunction.Member(name, GetKeys, -1, Statics.EmptyParameters);

            if (name == "fst")
                return DyForeignFunction.Member(name, GetFirst, -1, Statics.EmptyParameters);

            if (name == "snd")
                return DyForeignFunction.Member(name, GetSecond, -1, Statics.EmptyParameters);

            return null;
        }
    }
}
