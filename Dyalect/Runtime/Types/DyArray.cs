using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyObject, IEnumerable<DyObject>
    {
        internal readonly List<DyObject> Values;

        internal DyArray(List<DyObject> values) : base(StandardType.Array)
        {
            Values = values;
        }

        public override object ToObject() => Values.Select(v => v.ToObject()).ToArray();

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == StandardType.Integer)
                return GetItem((int)index.GetInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
        }

        private DyObject GetItem(int index)
        {
            if (index < 0 || index >= Values.Count)
                return null;
            return Values[index];
        }

        private void SetItem(int index, DyObject obj, ExecutionContext ctx)
        {
            if (index < 0 || index >= Values.Count)
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            else
                Values[index] = obj;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
            else
                SetItem((int)index.GetInteger(), value, ctx);
        }

        public IEnumerator<DyObject> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyArrayTypeInfo : DyTypeInfo
    {
        public DyArrayTypeInfo() : base(StandardType.Array, false)
        {

        }

        public override string TypeName => StandardType.ArrayName;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyArray)arg).Values.Count;
            return DyInteger.Get(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx)
        {
            var arr = (DyArray)arg;
            var sb = new StringBuilder();
            sb.Append('[');

            for (var i = 0; i < arr.Values.Count; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                var str = arr.Values[i].ToString(ctx);

                if (ctx.Error != null)
                    return DyString.Empty;

                sb.Append(str.GetString());
            }

            sb.Append(']');
            return new DyString(sb.ToString());
        }

        private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            ((DyArray)self).Values.Add(args.TakeOne(DyNil.Instance));
            return DyNil.Instance;
        }

        private DyObject InsertItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var index = args.TakeOne(DyNil.Instance);

            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(TypeName, index.TypeName(ctx)).Set(ctx);

            var i = (int)index.GetInteger();
            var value = args.TakeAt(1, DyNil.Instance);

            if (i < 0 || i >= arr.Values.Count)
                return Err.IndexOutOfRange(TypeName, i).Set(ctx);

            arr.Values.Insert(i, value);
            return DyNil.Instance;
        }

        private DyObject AddRange(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(null);

            if (val == null)
                return DyNil.Instance;

            var iter = DyIterator.GetIterator(val, ctx);

            if (ctx.HasErrors)
                return DyNil.Instance;

            while (true)
            {
                var res = iter.Call(ctx);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                if (!ReferenceEquals(res, DyNil.Terminator))
                    arr.Values.Add(res);
                else
                    break;
            }

            return DyNil.Instance;
        }

        private DyObject RemoveItem(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var val = args.TakeOne(DyNil.Instance);
            return ((DyArray)self).Values.Remove(val) ? DyBool.True : DyBool.False;
        }

        private DyObject RemoveItemAt(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var index = args.TakeOne(DyNil.Instance);

            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(TypeName, index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.GetInteger();
            var arr = (DyArray)self;

            if (idx < 0 || idx >= arr.Values.Count)
                return Err.IndexOutOfRange(TypeName, idx).Set(ctx);

            arr.Values.RemoveAt(idx);
            return DyNil.Instance;
        }

        private DyObject ClearItems(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            ((DyArray)self).Values.Clear();
            return DyNil.Instance;
        }

        private DyObject IndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(DyNil.Instance);
            var i = arr.Values.IndexOf(val);
            return DyInteger.Get(i);
        }

        private DyObject LastIndexOf(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(DyNil.Instance);
            var i = arr.Values.LastIndexOf(val);
            return DyInteger.Get(i);
        }

        private DyObject GetIndices(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            IEnumerable<DyObject> iterate()
            {
                for (var i = 0; i < arr.Values.Count; i++)
                    yield return DyInteger.Get(i);
            }

            return new DyIterator(iterate().GetEnumerator());
        }

        private DyObject GetSlice(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            var start = (int)args.TakeOne(DyInteger.Zero).GetInteger();
            var endo = args.TakeAt(1, null);
            var end = ReferenceEquals(endo, DyNil.Instance) ? arr.Values.Count : (int)endo.GetInteger();

            if (start == 0 && start == end)
                return self;

            if (start < 0 || start >= arr.Values.Count)
                return Err.IndexOutOfRange(TypeName, start).Set(ctx);

            if (end < 0 || end >= arr.Values.Count)
                return Err.IndexOutOfRange(TypeName, end).Set(ctx);

            var newArr = new DyObject[end - start];
            arr.Values.CopyTo(start, newArr, 0, end - start);
            return new DyArray(new List<DyObject>(newArr));
        }

        private DyObject SortBy(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var argo = args.TakeOne(null);

            if (ReferenceEquals(argo, DyNil.Instance))
                return Sort(ctx, self, null);

            var fun = argo as DyFunction;

            if (fun == null)
            {
                return Err.InvalidType(StandardType.FunctionName, 
                    args == null || args[0] == null ? StandardType.NilName : args[0].TypeName(ctx))
                    .Set(ctx);
            }

            arr.Values.Sort((x, y) => {
                var ret = fun.Call2(x, y, ctx);
                return ret.TypeId != StandardType.Integer ? 0 : (int)ret.GetInteger();
            });
            return DyNil.Instance;
        }

        private DyObject Sort(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            arr.Values.Sort((x, y) => {
                var res = ctx.Types[x.TypeId].Gt(x, y, ctx);
                return res == DyBool.True 
                    ? 1 
                    : ctx.Types[x.TypeId].Eq(x, y, ctx) == DyBool.True ? 0 : -1;
            });
            return DyNil.Instance;
        }

        private DyObject Compact(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;

            if (arr.Values.Count == 0)
                return DyNil.Instance;

            var idx = 0;

            while (idx < arr.Values.Count)
            {
                var e = arr.Values[idx];

                if (ReferenceEquals(e, DyNil.Instance))
                    arr.Values.RemoveAt(idx);
                else
                    idx++;
            }

            return DyNil.Instance;
        }

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Member(name, LenAdapter, Statics.EmptyParameters);

            if (name == "add")
                return DyForeignFunction.Member(name, AddItem, new Par("item"));

            if (name == "insert")
                return DyForeignFunction.Member(name, InsertItem, new Par("index"), new Par("item"));

            if (name == "addRange")
                return DyForeignFunction.Member(name, AddRange, new Par("seq"));

            if (name == "remove")
                return DyForeignFunction.Member(name, RemoveItem, new Par("item"));

            if (name == "removeAt")
                return DyForeignFunction.Member(name, RemoveItemAt, new Par("index"));

            if (name == "clear")
                return DyForeignFunction.Member(name, ClearItems, Statics.EmptyParameters);

            if (name == "indexOf")
                return DyForeignFunction.Member(name, IndexOf, new Par("item"));

            if (name == "lastIndexOf")
                return DyForeignFunction.Member(name, LastIndexOf, new Par("item"));

            if (name == "indices")
                return DyForeignFunction.Member(name, GetIndices, Statics.EmptyParameters);

            if (name == "slice")
                return DyForeignFunction.Member(name, GetSlice, new Par("start"), new Par("len", DyNil.Instance));

            if (name == "sort")
                return DyForeignFunction.Member(name, SortBy, new Par("comparator", DyNil.Instance));

            if (name == "compact")
                return DyForeignFunction.Member(name, Compact, Statics.EmptyParameters);

            return null;
        }
    }
}
