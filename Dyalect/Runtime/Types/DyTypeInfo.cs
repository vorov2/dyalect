using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyTypeInfo : DyObject
    {
        private readonly Dictionary<int, DyFunction> members = new Dictionary<int, DyFunction>();
        private readonly Dictionary<int, DyFunction> staticMembers = new Dictionary<int, DyFunction>();

        public override object ToObject() => this;

        public override string ToString() => TypeName.PutInBrackets();

        public abstract string TypeName { get; }

        public int TypeCode { get; internal set; }

        public bool SupportInstanceMembers { get; }

        internal DyTypeInfo(int typeCode, bool supportInstanceMembers) : base(StandardType.TypeInfo)
        {
            TypeCode = typeCode;
            SupportInstanceMembers = supportInstanceMembers;
        }

        #region Binary Operations
        //x + y
        private DyFunction add;
        protected virtual DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            return Err.OperationNotSupported(Builtins.Add, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        }
        internal DyObject Add(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == StandardType.String && TypeCode != StandardType.String)// || right.TypeId == StandardType.Char)
                return ctx.Types[StandardType.String].Add(left, right, ctx);

            if (add != null)
                return add.Clone(ctx, left).Call1(right, ctx);

            return AddOp(left, right, ctx);
        }

        //x - y
        private DyFunction sub;
        protected virtual DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(Builtins.Sub, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Sub(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (sub != null)
                return sub.Clone(ctx, left).Call1(right, ctx);
            return SubOp(left, right, ctx);
        }

        //x * y
        private DyFunction mul;
        protected virtual DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Mul, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Mul(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (mul != null)
                return mul.Clone(ctx, left).Call1(right, ctx);
            return MulOp(left, right, ctx);
        }

        //x / y
        private DyFunction div;
        protected virtual DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(Builtins.Div, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Div(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (div != null)
                return div.Clone(ctx, left).Call1(right, ctx);
            return DivOp(left, right, ctx);
        }

        //x % y
        private DyFunction rem;
        protected virtual DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            Err.OperationNotSupported(Builtins.Rem, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Rem(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (rem != null)
                return rem.Clone(ctx, left).Call1(right, ctx);
            return RemOp(left, right, ctx);
        }

        //x << y
        private DyFunction shl;
        protected virtual DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Shl, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject ShiftLeft(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (shl != null)
                return shl.Clone(ctx, left).Call1(right, ctx);
            return ShiftLeftOp(left, right, ctx);
        }

        //x >> y
        private DyFunction shr;
        protected virtual DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Shr, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject ShiftRight(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (shr != null)
                return shr.Clone(ctx, left).Call1(right, ctx);
            return ShiftRightOp(left, right, ctx);
        }

        //x & y
        private DyFunction and;
        protected virtual DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.And, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject And(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (and != null)
                return and.Clone(ctx, left).Call1(right, ctx);
            return AndOp(left, right, ctx);
        }

        //x | y
        private DyFunction or;
        protected virtual DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Or, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Or(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (or != null)
                return or.Clone(ctx, left).Call1(right, ctx);
            return OrOp(left, right, ctx);
        }

        //x ^ y
        private DyFunction xor;
        protected virtual DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Xor, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Xor(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (xor != null)
                return xor.Clone(ctx, left).Call1(right, ctx);
            return XorOp(left, right, ctx);
        }

        //x == y
        private DyFunction eq;
        protected virtual DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ReferenceEquals(left, right) ? DyBool.True : DyBool.False;
        internal DyObject Eq(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (eq != null)
                return eq.Clone(ctx, left).Call1(right, ctx);
            return EqOp(left, right, ctx);
        }

        //x != y
        private DyFunction neq;
        protected virtual DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Eq(left, right, ctx) == DyBool.True ? DyBool.False : DyBool.True;
        internal DyObject Neq(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (neq != null)
                return eq.Clone(ctx, left).Call1(right, ctx);
            return NeqOp(left, right, ctx);
        }

        //x > y
        private DyFunction gt;
        protected virtual DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Gt, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Gt(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (gt != null)
                return gt.Clone(ctx, left).Call1(right, ctx);
            return GtOp(left, right, ctx);
        }

        //x < y
        private DyFunction lt;
        protected virtual DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Lt, left.TypeName(ctx), right.TypeName(ctx)).Set(ctx);
        internal DyObject Lt(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (lt != null)
                return lt.Clone(ctx, left).Call1(right, ctx);
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
                return gte.Clone(ctx, left).Call1(right, ctx);
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
                return lte.Clone(ctx, left).Call1(right, ctx);
            return LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        //-x
        private DyFunction neg;
        protected virtual DyObject NegOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Neg, arg.TypeName(ctx)).Set(ctx);
        internal DyObject Neg(DyObject arg, ExecutionContext ctx)
        {
            if (neg != null)
                return neg.Clone(ctx, arg).Call0(ctx);
            return NegOp(arg, ctx);
        }

        //+x
        private DyFunction plus;
        protected virtual DyObject PlusOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Plus, arg.TypeName(ctx)).Set(ctx);
        internal DyObject Plus(DyObject arg, ExecutionContext ctx)
        {
            if (plus != null)
                return plus.Clone(ctx, arg).Call0(ctx);
            return PlusOp(arg, ctx);
        }

        //!x
        private DyFunction not;
        protected virtual DyObject NotOp(DyObject arg, ExecutionContext ctx) =>
            arg.GetBool() ? DyBool.False : DyBool.True;
        internal DyObject Not(DyObject arg, ExecutionContext ctx)
        {
            if (not != null)
                return not.Clone(ctx, arg).Call0(ctx);
            return NotOp(arg, ctx);
        }

        //~x
        private DyFunction bitnot;
        protected virtual DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.BitNot, arg.TypeName(ctx)).Set(ctx);
        internal DyObject BitwiseNot(DyObject arg, ExecutionContext ctx)
        {
            if (bitnot != null)
                return bitnot.Clone(ctx, arg).Call0(ctx);
            return BitwiseNotOp(arg, ctx);
        }

        //x.len
        private DyFunction len;
        protected virtual DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            Err.OperationNotSupported(Builtins.Len, arg.TypeName(ctx)).Set(ctx);
        internal DyObject Length(DyObject arg, ExecutionContext ctx)
        {
            if (len != null)
                return len.Clone(ctx, arg).Call0(ctx);
            return LengthOp(arg, ctx);
        }
        internal DyObject LenAdapter(ExecutionContext ctx, DyObject self, DyObject[] args) => LengthOp(self, ctx);

        //x.toString
        private DyFunction tos;
        protected virtual DyString ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.ToString());
        internal DyString ToString(DyObject arg, ExecutionContext ctx)
        {
            if (tos != null)
            {
                var retval = tos.Clone(ctx, arg).Call0(ctx);
                return retval.TypeId == StandardType.String ? (DyString)retval : DyString.Empty;
            }

            return ToStringOp(arg, ctx);
        }
        internal DyObject ToStringAdapter(ExecutionContext ctx, DyObject self, DyObject[] args) => ToStringOp(self, ctx);
        #endregion

        #region Other Operations
        internal DyObject GetStaticMember(int nameId, Unit unit, ExecutionContext ctx)
        {
            nameId = unit.MemberIds[nameId];

            if (!staticMembers.TryGetValue(nameId, out var value))
            {
                var name = ctx.Composition.Members[nameId];
                value = InternalGetStaticMember(name, ctx);

                if (value != null)
                    staticMembers.Add(nameId, value);
            }

            if (value != null)
                return value;

            return Err.StaticOperationNotSupported(ctx.Composition.Members[nameId], TypeName).Set(ctx);
        }

        internal void SetStaticMember(int nameId, DyObject value, Unit unit, ExecutionContext ctx)
        {
            var func = value as DyFunction;
            nameId = unit.MemberIds[nameId];
            staticMembers.Remove(nameId);

            if (func != null)
                staticMembers.Add(nameId, func);
        }

        internal DyObject HasMember(DyObject self, int nameId, Unit unit, ExecutionContext ctx)
        {
            nameId = unit.MemberIds[nameId];
            return GetMemberDirect(self, nameId, ctx) != null ? DyBool.True : DyBool.False;
        }

        internal DyObject GetMember(DyObject self, int nameId, Unit unit, ExecutionContext ctx)
        {
            nameId = unit.MemberIds[nameId];
            var value = GetMemberDirect(self, nameId, ctx);

            if (value != null)
                return value;

            return Err.OperationNotSupported(ctx.Composition.Members[nameId], TypeName).Set(ctx);
        }

        internal DyObject GetMemberDirect(DyObject self, int nameId, ExecutionContext ctx)
        {
            if (SupportInstanceMembers)
            {
                var ret = self.GetItem(ctx.Composition.Members[nameId], ctx);

                if (ret != null)
                    return ret;
            }

            if (!members.TryGetValue(nameId, out var value))
            {
                var name = ctx.Composition.Members[nameId];
                value = InternalGetMember(name, ctx);

                if (value != null)
                    members.Add(nameId, value);
            }

            if (value != null)
                return value.Clone(ctx, self);

            return value;
        }

        internal void SetMember(int nameId, DyObject value, Unit unit, ExecutionContext ctx)
        {
            var func = value as DyFunction;
            nameId = unit.MemberIds[nameId];
            var name = ctx.Composition.Members[nameId];
            SetBuiltin(name, func);
            members.Remove(nameId);

            if (func != null)
                members.Add(nameId, func);
        }

        private void SetBuiltin(string name, DyFunction func)
        {
            switch (name)
            {
                case Builtins.Add: add = func; break;
                case Builtins.Sub: sub = func; break;
                case Builtins.Mul: mul = func; break;
                case Builtins.Div: div = func; break;
                case Builtins.Rem: rem = func; break;
                case Builtins.Shl: shl = func; break;
                case Builtins.Shr: shr = func; break;
                case Builtins.And: and = func; break;
                case Builtins.Or: or = func; break;
                case Builtins.Xor: xor = func; break;
                case Builtins.Eq: eq = func; break;
                case Builtins.Neq: neq = func; break;
                case Builtins.Gt: gt = func; break;
                case Builtins.Lt: lt = func; break;
                case Builtins.Gte: gte = func; break;
                case Builtins.Lte: lte = func; break;
                case Builtins.Neg: neg = func; break;
                case Builtins.Not: not = func; break;
                case Builtins.BitNot: bitnot = func; break;
                case Builtins.Len: len = func; break;
                case Builtins.ToStr: tos = func; break;
                case Builtins.Plus: plus = func; break;
            }
        }

        private DyFunction InternalGetMember(string name, ExecutionContext ctx)
        {
            if (name == Builtins.ToStr)
                return DyForeignFunction.Member(name, ToStringAdapter, -1, Statics.EmptyParameters);

            if (name == Builtins.Iterator)
                return DyForeignFunction.Member(name, GetIterator, -1, Statics.EmptyParameters);

            return GetMember(name, ctx);
        }

        private DyObject GetIterator(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            if (self is IEnumerable<DyObject> en)
                return new DyIterator(en.GetEnumerator());
            else
                return Err.OperationNotSupported(Builtins.Iterator, TypeName).Set(ctx);
        }

        protected virtual DyFunction GetMember(string name, ExecutionContext ctx) => null;

        private DyFunction InternalGetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "__deleteMember")
                return DyForeignFunction.Static(name, (context, args) =>
                {
                    var nm = args.TakeOne(DyString.Empty).GetString();
                    if (context.Composition.MembersMap.TryGetValue(nm, out var nameId))
                    {
                        SetBuiltin(nm, null);
                        members.Remove(nameId);
                        staticMembers.Remove(nameId);
                    }
                    return DyNil.Instance;
                }, -1, new Par("name"));

            return GetStaticMember(name, ctx);
        }

        protected virtual DyFunction GetStaticMember(string name, ExecutionContext ctx) => null;

        internal protected virtual DyObject Get(DyObject obj, DyObject index, ExecutionContext ctx) => obj.GetItem(index, ctx);

        internal protected virtual void Set(DyObject obj, DyObject index, DyObject val, ExecutionContext ctx) => obj.SetItem(index, val, ctx);
        #endregion
    }

    internal sealed class DyTypeTypeInfo : DyTypeInfo
    {
        public DyTypeTypeInfo() : base(StandardType.TypeInfo, true)
        {

        }

        public override string TypeName => StandardType.TypeInfoName;

        protected override DyString ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());
    }
}