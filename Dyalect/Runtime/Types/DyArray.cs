using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyObject
    {
        internal readonly DyObject[] Values;

        internal DyArray(DyObject[] values) : base(StandardType.Array)
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
            if (index < 0 || index >= Values.Length)
                return null;
            return Values[index];
        }

        private void SetItem(int index, DyObject obj, ExecutionContext ctx)
        {
            if (index < 0 || index >= Values.Length)
                Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
            Values[index] = obj;
        }

        protected internal override void SetItem(DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);
            else
                SetItem((int)index.GetInteger(), value, ctx);
        }
    }

    internal sealed class DyArrayTypeInfo : DyTypeInfo
    {
        public static readonly DyArrayTypeInfo Instance = new DyArrayTypeInfo();

        private DyArrayTypeInfo() : base(StandardType.Tuple)
        {

        }

        public override string TypeName => StandardType.ArrayName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) => new DyArray(args);

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = ((DyArray)arg).Values.Length;
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

            for (var i = 0; i < arr.Values.Length; i++)
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

        private DyFunction GetIterator(DyObject obj, ExecutionContext ctx) => new DyArrayIteratorFunction((DyArray)obj);

        protected override DyFunction GetTrait(string name, ExecutionContext ctx)
        {
            if (name == "iterator")
                return new DyMemberFunction(name, GetIterator);

            return null;
        }
    }

    //This function is implemented for optimization purposes.
    internal sealed class DyArrayIteratorFunction : DyFunction
    {
        internal const string Name = "iterator";
        private readonly DyArray arr;
        private int idx;

        internal DyArrayIteratorFunction(DyArray arr) : base(0, EXT_HANDLE, 0, null, null)
        {
            this.arr = arr;
        }

        protected override string GetFunctionName() => Name;

        public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            if (idx < arr.Values.Length)
                return DyTuple.Create(DyBool.True, arr.Values[idx++]);

            return DyTuple.Create(DyBool.False, DyNil.Instance);
        }

        internal override DyFunction Clone(DyObject arg) =>
            new DyArrayIteratorFunction(arr)
            {
                Self = arg
            };
    }
}
