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
            return len == 1 ? DyInteger.One
                : len == 2 ? DyInteger.Two
                : len == 3 ? DyInteger.Three
                : new DyInteger(len);
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

        private DyObject AddRange(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var arr = (DyArray)self;
            var val = args.TakeOne(null);

            if (val == null)
                return DyNil.Instance;

            DyFunction iter = null;

            if (val.TypeId == StandardType.Iterator)
                iter = val as DyFunction;
            else
            {
                iter = ctx.Assembly.Types[val.TypeId].GetTraitOp(val, "iterator", ctx) as DyFunction;

                if (ctx.HasErrors)
                    return DyNil.Instance;

                iter = iter.Call(ctx) as DyFunction;
            }

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

        protected override DyFunction GetTrait(string name, ExecutionContext ctx)
        {
            if (name == "add")
                return DyForeignFunction.Create(name, AddItem);

            if (name == "addRange")
                return DyForeignFunction.Create(name, AddRange);

            if (name == "remove")
                return DyForeignFunction.Create(name, RemoveItem);

            if (name == "removeAt")
                return DyForeignFunction.Create(name, RemoveItemAt);

            if (name == "clear")
                return DyForeignFunction.Create(name, ClearItems);

            return null;
        }
    }
}
