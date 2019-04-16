using System.Linq;
using System.Collections.Generic;
using System;

namespace Dyalect.Runtime.Types
{
    public class DyArray : DyObject
    {
        internal readonly DyObject[] Values;

        internal DyArray(DyObject[] values) : base(StandardType.Array)
        {
            Values = values;
        }

        public override object AsObject() => Values;

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
                return GetItem((int)index.AsInteger()) ?? Err.IndexOutOfRange(this.TypeName(ctx), index).Set(ctx);
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
                SetItem((int)index.AsInteger(), value, ctx);
        }
    }
}
