using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;
using Dyalect.Compiler;

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

        protected override bool TestEquality(DyObject obj)
        {
            var t = (DyArray)obj;

            if (Values.Length != t.Values.Length)
                return false;

            for (var i = 0; i < Values.Length; i++)
            {
                if (Values[i].Equals(t.Values[i]))
                    return false;
            }

            return true;
        }

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

        private DyObject GetIterator(DyObject obj, ExecutionContext ctx) => new DyArrayIterator((DyArray)obj);

        protected override DyFunction GetTrait(string name, ExecutionContext ctx)
        {
            if (name == "iterator")
                return new DyMemberFunction(name, GetIterator);

            return null;
        }
    }

    internal sealed class DyArrayIterator : DyObject
    {
        internal DyArray Array { get; }

        internal int Index { get; set; } = -1;

        public DyArrayIterator(DyArray arr) : base(StandardType.ArrayIterator)
        {
            Array = arr;
        }

        public override object ToObject() => ((IEnumerable<object>)Array.ToObject()).GetEnumerator();

        protected override bool TestEquality(DyObject obj) => Array.Equals(((DyArrayIterator)obj).Array);
    }

    internal sealed class DyArrayIteratorTypeInfo : DyTypeInfo
    {
        public static readonly DyTypeInfo Instance = new DyArrayIteratorTypeInfo();

        public DyArrayIteratorTypeInfo() : base(StandardType.ArrayIterator)
        {

        }

        protected override DyObject MoveNextOp(DyObject obj, ExecutionContext ctx)
        {
            var iter = (DyArrayIterator)obj;
            iter.Index++;
            return iter.Index < iter.Array.Values.Length ? DyBool.True : DyBool.False;
        }

        protected override DyObject GetCurrentOp(DyObject obj, ExecutionContext ctx)
        {
            var iter = (DyArrayIterator)obj;
            return iter.Array.Values[iter.Index];
        }

        protected override DyFunction GetTrait(string name, ExecutionContext ctx)
        {
            if (name == Traits.NextName)
                return new DyMemberFunction(name, MoveNextOp);

            if (name == Traits.CurName)
                return new DyMemberFunction(name, GetCurrentOp);

            return null;
        }

        public override string TypeName => StandardType.ArrayIteratorName;

        public override DyObject Create(ExecutionContext ctx, params DyObject[] args) =>
            throw new NotImplementedException();
    }
}
