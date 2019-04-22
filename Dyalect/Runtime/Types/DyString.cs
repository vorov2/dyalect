using Dyalect.Compiler;
using Dyalect.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Runtime.Types
{
    public sealed class DyString : DyObject, IEnumerable<DyObject>
    {
        public static readonly DyString Empty = new DyString("");
        internal readonly string Value;

        public DyString(string str) : base(StandardType.String)
        {
            Value = str;
        }

        public override object ToObject() => Value;

        public override string ToString() => Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is DyString s)
                return Value.Equals(s.Value);
            else
                return false;
        }

        internal protected override string GetString() => Value;

        public static implicit operator string(DyString str) => str.Value;

        public static implicit operator DyString(string str) => new DyString(str);

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != StandardType.Integer)
                return Err.IndexInvalidType(this.TypeName(ctx), index.TypeName(ctx)).Set(ctx);

            var idx = (int)index.GetInteger();

            if (idx < 0 || idx >= Value.Length)
                return Err.IndexOutOfRange(this.TypeName(ctx), idx).Set(ctx);

            return new DyString(Value[idx].ToString());
        }

        public IEnumerator<DyObject> GetEnumerator() => Value.Select(c => new DyString(c.ToString())).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal sealed class DyStringTypeInfo : DyTypeInfo
    {
        public static readonly DyStringTypeInfo Instance = new DyStringTypeInfo();

        private DyStringTypeInfo() : base(StandardType.Bool)
        {

        }

        public override string TypeName => StandardType.StringName;

        #region Operations
        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var str1 = left.TypeId == StandardType.String ? left.GetString() : left.ToString(ctx).Value;
            var str2 = right.TypeId == StandardType.String ? right.GetString() : right.ToString(ctx).Value;
            return new DyString(str1 + str2);
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString() == right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.EqOp(left, right, ctx);
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString() != right.GetString() ? DyBool.True : DyBool.False;
            else
                return base.NeqOp(left, right, ctx);
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
            else
                return base.GtOp(left, right, ctx);
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (left.TypeId == right.TypeId)
                return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
            else
                return base.LtOp(left, right, ctx);
        }

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
        {
            var len = arg.GetString().Length;
            return DyInteger.Get(len);
        }

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) => StringUtil.Escape(arg.GetString());

        protected override DyFunction GetTrait(string name, ExecutionContext ctx)
        {

            return null;
        }
        #endregion
    }
}
