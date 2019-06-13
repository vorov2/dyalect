using Dyalect.Compiler;
using Dyalect.Debug;
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

        public DyTuple(DyObject[] values) : base(DyType.Tuple)
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
            if (index.TypeId == DyType.Integer)
                return GetItem((int)index.GetInteger(), ctx);
            else if (index.TypeId == DyType.String)
                return GetItem(index.GetString(), ctx) ?? ctx.IndexOutOfRange(this.TypeName(ctx), index.GetString());
            else
                return ctx.IndexInvalidType(this.TypeName(ctx), index);
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.Integer)
                SetItem((int)index.GetInteger(), value, ctx);
            else if (index.TypeId == DyType.String)
                SetItem(index.GetString(), value, ctx);
            else
                ctx.IndexInvalidType(this.TypeName(ctx), index);
        }

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            var i = GetOrdinal(name);

            if (i == -1)
                return ctx.IndexOutOfRange(DyTypeNames.Tuple, name);

            return GetItem(i, ctx);
        }

        protected internal override void SetItem(string name, DyObject value, ExecutionContext ctx)
        {
            var i = GetOrdinal(name);

            if (i == -1)
                ctx.IndexOutOfRange(DyTypeNames.Tuple, name);

            SetItem(i, value, ctx);
        }

        protected internal override int GetOrdinal(string name)
        {
            for (var i = 0; i < Values.Length; i++)
                if (Values[i].GetLabel() == name)
                    return i;
            return -1;
        }

        protected internal override DyObject GetItem(int index, ExecutionContext ctx)
        {
            if (index < 0 || index >= Values.Length)
                return ctx.IndexOutOfRange(DyTypeNames.Tuple, index);
            return Values[index].TypeId == DyType.Label ? Values[index].GetTaggedValue() : Values[index];
        }

        internal string GetKey(int index) => Values[index].GetLabel();

        protected internal override void SetItem(int index, DyObject value, ExecutionContext ctx)
        {
            if (index < 0 || index >= Values.Length)
                ctx.IndexOutOfRange(this.TypeName(ctx), index);
            else
            {
                if (Values[index].TypeId == DyType.Label)
                    ((DyLabel)Values[index]).Value = value;
                else
                    Values[index] = value;
            }
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx)
        {
            return GetOrdinal(name) != -1;
        }

        private string DefaultKey() => Guid.NewGuid().ToString();

        public IEnumerator<DyObject> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return Values[i].TypeId == DyType.Label ? Values[i].GetTaggedValue() : Values[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyTupleTypeInfo : DyTypeInfo
    {
        public DyTupleTypeInfo() : base(DyType.Tuple)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not 
            | SupportedOperations.Get | SupportedOperations.Set;

        public override string TypeName => DyTypeNames.Tuple;

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
                var val = tup.GetItem(i, ctx).ToString(ctx);

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

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId != right.TypeId)
                return DyBool.False;

            var t1 = (DyTuple)left;
            var t2 = (DyTuple)right;

            if (t1.Count != t2.Count)
                return DyBool.False;

            for (var i = 0; i < t1.Count; i++)
            {
                if (ctx.Types[t1.Values[i].TypeId].Eq(ctx, t1.Values[i], t2.Values[i]) == DyBool.False)
                    return DyBool.False;

                if (ctx.HasErrors)
                    return DyNil.Instance;
            }

            return DyBool.True;
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject GetOp(DyObject self, int index, ExecutionContext ctx) => self.GetItem(index, ctx);

        protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
        {
            self.SetItem(index, value, ctx);
            return DyNil.Instance;
        }

        private DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var tup = (DyTuple)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < tup.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(iterate());
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

            return new DyIterator(iterate());
        }

        private DyObject GetFirst(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return self.GetItem(0, ctx);
        }

        private DyObject GetSecond(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            return self.GetItem(1, ctx);
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Len:
                    return DyForeignFunction.Member(name, Length);
                case "indices":
                    return DyForeignFunction.Member(name, GetIndices, -1, Statics.EmptyParameters);
                case "keys":
                    return DyForeignFunction.Member(name, GetKeys, -1, Statics.EmptyParameters);
                case "fst":
                    return DyForeignFunction.Member(name, GetFirst, -1, Statics.EmptyParameters);
                case "snd":
                    return DyForeignFunction.Member(name, GetSecond, -1, Statics.EmptyParameters);
                default:
                    return DyForeignFunction.Static("$$$AutoInvoke", (c, self) =>
                    {
                        var idx = self.GetOrdinal(name);
                        if (idx == -1)
                            return ctx.OperationNotSupported(name, self);
                        return self.GetItem(idx, ctx);
                    }, -1, new Par("self"));
            }
        }

        private DyObject GetPair(ExecutionContext ctx, DyObject fst, DyObject snd)
        {
            return new DyTuple(new DyObject[] { fst, snd });
        }

        private DyObject GetTriple(ExecutionContext ctx, DyObject fst, DyObject snd, DyObject thd)
        {
            return new DyTuple(new DyObject[] { fst, snd, thd });
        }

        private DyObject MakeNew(ExecutionContext ctx, DyObject obj) => obj;

        protected override DyObject GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "pair")
                return DyForeignFunction.Static(name, GetPair, -1, new Par("first"), new Par("second"));

            if (name == "triple")
                return DyForeignFunction.Static(name, GetTriple, -1, new Par("first"), new Par("second"), new Par("third"));

            if (name == "new")
                return DyForeignFunction.Static(name, MakeNew, 0, new Par("values"));

            return null;
        }
    }
}
