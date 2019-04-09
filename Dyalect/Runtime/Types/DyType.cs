using System;
using System.Collections.Generic;
using Dyalect.Compiler;

namespace Dyalect.Runtime.Types
{
    public abstract class DyType : DyObject
    {
        private readonly Dictionary<string, DyObject> mixins = new Dictionary<string, DyObject>();

        public DyType() : base(StandardType.Type)
        {

        }

        internal DyType(int typeCode) : base(StandardType.Type)
        {
            TypeCode = typeCode;
        }

        #region Conversion
        public virtual bool CanConvertFrom(Type type) => false;

        public bool CanConvertFrom<T>() => CanConvertFrom(typeof(T));

        public T ConvertFrom<T>(T obj, ExecutionContext ctx) where T : DyObject => (T)ConvertFrom(obj, typeof(T), ctx);

        public DyObject ConvertFrom(object obj, ExecutionContext ctx) => ConvertFrom(obj, obj?.GetType(), ctx);

        public virtual DyObject ConvertFrom(object obj, Type type, ExecutionContext ctx) => throw new InvalidCastException();

        public virtual bool CanConvertTo(Type type) => false;

        public bool CanConvertTo<T>() => CanConvertTo(typeof(T));

        public T ConvertTo<T>(DyObject obj, ExecutionContext ctx) => (T)ConvertTo(obj, typeof(T), ctx);

        public virtual object ConvertTo(DyObject obj, Type type, ExecutionContext ctx) => throw new NotSupportedException();
        #endregion

        public abstract DyObject Create(ExecutionContext ctx, params DyObject[] args);

        public override object AsObject() => this;

        public override string AsString() => "[" + TypeName + "]";

        protected override bool TestEquality(DyObject obj) => ((DyType)obj).TypeCode == TypeCode;

        public abstract string TypeName { get; }

        public int TypeCode { get; internal set; }

        internal DyObject GetMixin(string mixinName, ExecutionContext ctx)
        {
            if (!mixins.TryGetValue(mixinName, out var value))
            {
                value = InitializeMixin(mixinName, ctx);

                if (value != null)
                    mixins.Add(mixinName, value);
            }

            if (value != null)
                return value;

            return Err.OperationNotSupported(mixinName, TypeName).Set(ctx);
        }

        internal void SetMixin(string mixinName, DyObject value, ExecutionContext ctx)
        {
            var func = (DyFunction)value;

            switch (mixinName)
            {
                case BuiltinMixins.AddName: add = func; break;
                case BuiltinMixins.SubName: sub = func; break;
                case BuiltinMixins.MulName: mul = func; break;
                case BuiltinMixins.DivName: div = func; break;
                case BuiltinMixins.RemName: rem = func; break;
                case BuiltinMixins.ShlName: shl = func; break;
                case BuiltinMixins.ShrName: shr = func; break;
                case BuiltinMixins.AndName: and = func; break;
                case BuiltinMixins.OrName:  or = func; break;
                case BuiltinMixins.XorName: xor = func; break;
                case BuiltinMixins.EqName:  eq = func; break;
                case BuiltinMixins.NeqName: neq = func; break;
                case BuiltinMixins.GtName:  gt = func; break;
                case BuiltinMixins.LtName:  lt = func; break;
                case BuiltinMixins.GteName: gte = func; break;
                case BuiltinMixins.LteName: lte = func; break;
                case BuiltinMixins.NegName: neg = func; break;
                case BuiltinMixins.NotName: not = func; break;
                case BuiltinMixins.BitName: bitnot = func; break;
                case BuiltinMixins.LenName: len = func; break;
                case BuiltinMixins.GetName: get = func; break;
                case BuiltinMixins.SetName: set = func; break;
                case BuiltinMixins.CoerceName: coerce = func; break;
            }

            mixins.Remove(mixinName);
            mixins.Add(mixinName, func);
        }

        protected virtual DyObject InitializeMixin(string mixinName, ExecutionContext ctx) => null;

        #region Binary Operations
        //x + y
        private DyFunction add;
        protected virtual DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.String)
                return new DyString(left.AsString() + right.AsString());

            return Err.OperationNotSupported(BuiltinMixins.AddName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        }

        internal DyObject Add(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (add != null)
                return add.Call2(left, right, ctx);
            return AddOp(left, right, ctx);
        }

        //x - y
        private DyFunction sub;
        protected virtual DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(BuiltinMixins.SubName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Sub(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (sub != null)
                return sub.Call2(left, right, ctx);
            return SubOp(left, right, ctx);
        }

        //x * y
        private DyFunction mul;
        protected virtual DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.MulName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Mul(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (mul != null)
                return mul.Call2(left, right, ctx);
            return MulOp(left, right, ctx);
        }

        //x / y
        private DyFunction div;
        protected virtual DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(BuiltinMixins.DivName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Div(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (div != null)
                return div.Call2(left, right, ctx);
            return DivOp(left, right, ctx);
        }

        //x % y
        private DyFunction rem;
        protected virtual DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(BuiltinMixins.RemName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Rem(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (rem != null)
                return rem.Call2(left, right, ctx);
            return RemOp(left, right, ctx);
        }

        //x << y
        private DyFunction shl;
        protected virtual DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.ShlName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject ShiftLeft(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (shl != null)
                return shl.Call2(left, right, ctx);
            return ShiftLeftOp(left, right, ctx);
        }

        //x >> y
        private DyFunction shr;
        protected virtual DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.ShrName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject ShiftRight(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (shr != null)
                return shr.Call2(left, right, ctx);
            return ShiftRightOp(left, right, ctx);
        }

        //x & y
        private DyFunction and;
        protected virtual DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.AndName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject And(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (and != null)
                return and.Call2(left, right, ctx);
            return AndOp(left, right, ctx);
        }

        //x | y
        private DyFunction or;
        protected virtual DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.OrName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Or(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (or != null)
                return or.Call2(left, right, ctx);
            return OrOp(left, right, ctx);
        }

        //x ^ y
        private DyFunction xor;
        protected virtual DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.XorName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Xor(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (xor != null)
                return xor.Call2(left, right, ctx);
            return XorOp(left, right, ctx);
        }

        //x == y
        private DyFunction eq;
        protected virtual DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ReferenceEquals(left, right) ? DyBool.True : DyBool.False;
        internal DyObject Eq(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (eq != null)
                return eq.Call2(left, right, ctx);
            return EqOp(left, right, ctx);
        }

        //x != y
        private DyFunction neq;
        protected virtual DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Eq(left, right, ctx) == DyBool.True ? DyBool.False : DyBool.True;
        internal DyObject Neq(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (neq != null)
                return eq.Call2(left, right, ctx);
            return NeqOp(left, right, ctx);
        }

        //x > y
        private DyFunction gt;
        protected virtual DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.GtName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Gt(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (gt != null)
                return gt.Call2(left, right, ctx);
            return GtOp(left, right, ctx);
        }

        //x < y
        private DyFunction lt;
        protected virtual DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.LtName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Lt(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (lt != null)
                return lt.Call2(left, right, ctx);
            return LtOp(left, right, ctx);
        }

        //x >= y
        private DyFunction gte;
        protected virtual DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = Gt(left, right, ctx) == DyBool.True || Eq(left, right, ctx) == DyBool.True;
            return ret ? DyBool.True : DyBool.False;
        }
        internal DyObject Gte(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (gte != null)
                return gte.Call2(left, right, ctx);
            return GteOp(left, right, ctx);
        }

        //x <= y
        private DyFunction lte;
        protected virtual DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = Lt(left, right, ctx) == DyBool.True || Eq(left, right, ctx) == DyBool.True;
            return ret ? DyBool.True : DyBool.False;
        }
        internal DyObject Lte(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (lte != null)
                return lte.Call2(left, right, ctx);
            return LteOp(left, right, ctx);
        }

        //coerce
        private DyFunction coerce;
        protected virtual DyObject CoerceOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(BuiltinMixins.CoerceName, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Coerce(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (coerce != null)
                return coerce.Call2(left, right, ctx);
            return CoerceOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        //-x
        private DyFunction neg;
        protected virtual DyObject NegOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.NegName, arg.TypeName(ctx)).Set(ctx);
        internal DyObject Neg(DyObject arg, ExecutionContext ctx)
        {
            if (neg != null)
                return neg.Call1(arg, ctx);
            return NegOp(arg, ctx);
        }

        //!x
        private DyFunction not;
        protected virtual DyObject NotOp(DyObject arg, ExecutionContext ctx) =>
            arg.AsBool() ? DyBool.False : DyBool.True;
        internal DyObject Not(DyObject arg, ExecutionContext ctx)
        {
            if (not != null)
                return not.Call1(arg, ctx);
            return NotOp(arg, ctx);
        }

        //~x
        private DyFunction bitnot;
        protected virtual DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.BitName, arg.TypeName(ctx)).Set(ctx);
        internal DyObject BitwiseNot(DyObject arg, ExecutionContext ctx)
        {
            if (bitnot != null)
                return bitnot.Call1(arg, ctx);
            return BitwiseNotOp(arg, ctx);
        }

        //#x
        private DyFunction len;
        protected virtual DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.LenName, arg.TypeName(ctx)).Set(ctx);
        internal DyObject Length(DyObject arg, ExecutionContext ctx)
        {
            if (len != null)
                return len.Call1(arg, ctx);
            return LengthOp(arg, ctx);
        }
        #endregion

        #region Other Operations
        //x.f
        private DyFunction get;
        protected virtual DyObject GetOp(DyObject obj, DyObject index, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.GetName, obj.TypeName(ctx)).Set(ctx);
        internal DyObject Get(DyObject obj, DyObject index, ExecutionContext ctx)
        {
            if (get != null)
                return get.Call2(obj, index, ctx);
            return GetOp(obj, index, ctx);
        }

        //x.f = y
        private DyFunction set;
        protected virtual void SetOp(DyObject obj, DyObject index, DyObject value, ExecutionContext ctx) =>
            Err.OperationNotSupported(BuiltinMixins.SetName, obj.TypeName(ctx)).Set(ctx);
        internal void Set(DyObject obj, DyObject index, DyObject value, ExecutionContext ctx)
        {
            if (set != null)
                set.Call3(obj, index, value, ctx);
            SetOp(obj, index, value, ctx);
        }
        #endregion
    }
}