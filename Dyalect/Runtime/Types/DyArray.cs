using Dyalect.Compiler;
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
        public static readonly DyArrayTypeInfo Instance = new DyArrayTypeInfo();

        private DyArrayTypeInfo() : base(StandardType.Tuple)
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

        protected override DyFunction GetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.Len)
                return DyForeignFunction.Create(name, LenAdapter);

            if (name == "add")
                return DyForeignFunction.Create(name, AddItem);

            if (name == "insert")
                return DyForeignFunction.Create(name, InsertItem);

            if (name == "addRange")
                return DyForeignFunction.Create(name, AddRange);

            if (name == "remove")
                return DyForeignFunction.Create(name, RemoveItem);

            if (name == "removeAt")
                return DyForeignFunction.Create(name, RemoveItemAt);

            if (name == "clear")
                return DyForeignFunction.Create(name, ClearItems);

            if (name == "indexOf")
                return DyForeignFunction.Create(name, IndexOf);

            if (name == "lastIndexOf")
                return DyForeignFunction.Create(name, LastIndexOf);

            if (name == "indices")
                return DyForeignFunction.Create(name, GetIndices);

            return null;
        }
    }
}
