using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public abstract class DyTypeInfo : DyObject
    {
        [Flags]
        public enum SupportedOperations
        {
            Add = 0x01,
            Sub = 0x02,
            Mul = 0x04,
            Div = 0x08,
            Rem = 0x10,
            Shl = 0x20,
            Shr = 0x40,
            And = 0x80,
            Or  = 0x100,
            Xor = 0x200,
            Eq  = 0x400,
            Neq = 0x800,
            Gt  = 0x1000,
            Lt  = 0x2000,
            Gte = 0x4000,
            Lte = 0x8000,
            Neg = 0x10000,
            BitNot = 0x20000,
            Bit =  0x40000,
            Plus = 0x80000,
            Not =  0x100000,
            Get =  0x200000,
            Set =  0x400000
        }

        protected abstract SupportedOperations GetSupportedOperations();

        private DyBool Support(SupportedOperations op)
        {
            return (GetSupportedOperations() & op) == op ? DyBool.True : DyBool.False;
        }

        private readonly Dictionary<int, DyFunction> members = new Dictionary<int, DyFunction>();
        private readonly Dictionary<int, DyFunction> staticMembers = new Dictionary<int, DyFunction>();

        public override object ToObject() => this;

        public override string ToString() => TypeName.PutInBrackets();

        public abstract string TypeName { get; }

        public int TypeCode { get; internal set; }

        public bool SupportInstanceMembers { get; }

        internal DyTypeInfo(int typeCode, bool supportInstanceMembers) : base(DyType.TypeInfo)
        {
            TypeCode = typeCode;
            SupportInstanceMembers = supportInstanceMembers;
        }

        #region Binary Operations
        //x + y
        private DyFunction add;
        protected virtual DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Add, left, right);
        internal DyObject Add(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (right.TypeId == DyType.String && TypeCode != DyType.String)
                return ctx.Types[DyType.String].Add(ctx, left, right);

            if (add != null)
                return add.Clone(ctx, left).Call1(right, ctx);

            return AddOp(left, right, ctx);
        }

        //x - y
        private DyFunction sub;
        protected virtual DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            ctx.OperationNotSupported(Builtins.Sub, left, right);
        internal DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (sub != null)
                return sub.Clone(ctx, left).Call1(right, ctx);
            return SubOp(left, right, ctx);
        }

        //x * y
        private DyFunction mul;
        protected virtual DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            ctx.OperationNotSupported(Builtins.Mul, left, right);
        internal DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (mul != null)
                return mul.Clone(ctx, left).Call1(right, ctx);
            return MulOp(left, right, ctx);
        }

        //x / y
        private DyFunction div;
        protected virtual DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            ctx.OperationNotSupported(Builtins.Div, left, right);
        internal DyObject Div(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (div != null)
                return div.Clone(ctx, left).Call1(right, ctx);
            return DivOp(left, right, ctx);
        }

        //x % y
        private DyFunction rem;
        protected virtual DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) => 
            ctx.OperationNotSupported(Builtins.Rem, left, right);
        internal DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (rem != null)
                return rem.Clone(ctx, left).Call1(right, ctx);
            return RemOp(left, right, ctx);
        }

        //x << y
        private DyFunction shl;
        protected virtual DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Shl, left, right);
        internal DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (shl != null)
                return shl.Clone(ctx, left).Call1(right, ctx);
            return ShiftLeftOp(left, right, ctx);
        }

        //x >> y
        private DyFunction shr;
        protected virtual DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Shr, left, right);
        internal DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (shr != null)
                return shr.Clone(ctx, left).Call1(right, ctx);
            return ShiftRightOp(left, right, ctx);
        }

        //x & y
        private DyFunction and;
        protected virtual DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.And, left, right);
        internal DyObject And(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (and != null)
                return and.Clone(ctx, left).Call1(right, ctx);
            return AndOp(left, right, ctx);
        }

        //x | y
        private DyFunction or;
        protected virtual DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Or, left, right);
        internal DyObject Or(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (or != null)
                return or.Clone(ctx, left).Call1(right, ctx);
            return OrOp(left, right, ctx);
        }

        //x ^ y
        private DyFunction xor;
        protected virtual DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Xor, left, right);
        internal DyObject Xor(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (xor != null)
                return xor.Clone(ctx, left).Call1(right, ctx);
            return XorOp(left, right, ctx);
        }

        //x == y
        private DyFunction eq;
        protected virtual DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ReferenceEquals(left, right) ? DyBool.True : DyBool.False;
        internal DyObject Eq(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (eq != null)
                return eq.Clone(ctx, left).Call1(right, ctx);
            if (right.TypeId == DyType.Bool)
                return left.GetBool() == right.GetBool() ? DyBool.True : DyBool.False;
            return EqOp(left, right, ctx);
        }

        //x != y
        private DyFunction neq;
        protected virtual DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Eq(ctx, left, right) == DyBool.True ? DyBool.False : DyBool.True;
        internal DyObject Neq(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (neq != null)
                return eq.Clone(ctx, left).Call1(right, ctx);
            return NeqOp(left, right, ctx);
        }

        //x > y
        private DyFunction gt;
        protected virtual DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Gt, left, right);
        internal DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (gt != null)
                return gt.Clone(ctx, left).Call1(right, ctx);
            return GtOp(left, right, ctx);
        }

        //x < y
        private DyFunction lt;
        protected virtual DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Lt, left, right);
        internal DyObject Lt(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (lt != null)
                return lt.Clone(ctx, left).Call1(right, ctx);
            return LtOp(left, right, ctx);
        }

        //x >= y
        private DyFunction gte;
        protected virtual DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = Gt(ctx, left, right) == DyBool.True || Eq(ctx, left, right) == DyBool.True;
            return ret ? DyBool.True : DyBool.False;
        }
        internal DyObject Gte(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (gte != null)
                return gte.Clone(ctx, left).Call1(right, ctx);
            return GteOp(left, right, ctx);
        }

        //x <= y
        private DyFunction lte;
        protected virtual DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = Lt(ctx, left, right) == DyBool.True || Eq(ctx, left, right) == DyBool.True;
            return ret ? DyBool.True : DyBool.False;
        }
        internal DyObject Lte(ExecutionContext ctx, DyObject left, DyObject right)
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
            ctx.OperationNotSupported(Builtins.Neg, arg);
        internal DyObject Neg(ExecutionContext ctx, DyObject arg)
        {
            if (neg != null)
                return neg.Clone(ctx, arg).Call0(ctx);
            return NegOp(arg, ctx);
        }

        //+x
        private DyFunction plus;
        protected virtual DyObject PlusOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Plus, arg);
        internal DyObject Plus(ExecutionContext ctx, DyObject arg)
        {
            if (plus != null)
                return plus.Clone(ctx, arg).Call0(ctx);
            return PlusOp(arg, ctx);
        }

        //!x
        private DyFunction not;
        protected virtual DyObject NotOp(DyObject arg, ExecutionContext ctx) =>
            arg.GetBool() ? DyBool.False : DyBool.True;
        internal DyObject Not(ExecutionContext ctx, DyObject arg)
        {
            if (not != null)
                return not.Clone(ctx, arg).Call0(ctx);
            return NotOp(arg, ctx);
        }

        //~x
        private DyFunction bitnot;
        protected virtual DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.BitNot, arg);
        internal DyObject BitwiseNot(ExecutionContext ctx, DyObject arg)
        {
            if (bitnot != null)
                return bitnot.Clone(ctx, arg).Call0(ctx);
            return BitwiseNotOp(arg, ctx);
        }

        //x.len
        private DyFunction len;
        protected virtual DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Len, arg);
        internal DyObject Length(ExecutionContext ctx, DyObject arg)
        {
            if (len != null)
                return len.Clone(ctx, arg).Call0(ctx);
            return LengthOp(arg, ctx);
        }

        //x.toString
        private DyFunction tos;
        protected virtual DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.ToString());
        internal DyObject ToString(ExecutionContext ctx, DyObject arg)
        {
            if (tos != null)
            {
                var retval = tos.Clone(ctx, arg).Call0(ctx);
                return retval.TypeId == DyType.String ? (DyString)retval : DyString.Empty;
            }

            return ToStringOp(arg, ctx);
        }
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

            return ctx.StaticOperationNotSupported(ctx.Composition.Members[nameId], TypeName);
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
            var name = ctx.Composition.Members[nameId];
            return HasMemberDirect(self, name, nameId, ctx);
        }

        private DyBool HasMemberDirect(DyObject self, string name, int nameId, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Add: return Support(SupportedOperations.Add);
                case Builtins.Sub: return Support(SupportedOperations.Sub);
                case Builtins.Mul: return Support(SupportedOperations.Mul);
                case Builtins.Div: return Support(SupportedOperations.Div);
                case Builtins.Rem: return Support(SupportedOperations.Rem);
                case Builtins.Shl: return Support(SupportedOperations.Shl);
                case Builtins.Shr: return Support(SupportedOperations.Shr);
                case Builtins.And: return Support(SupportedOperations.And);
                case Builtins.Or: return Support(SupportedOperations.Or);
                case Builtins.Xor: return Support(SupportedOperations.Xor);
                case Builtins.Eq: return Support(SupportedOperations.Eq);
                case Builtins.Neq: return Support(SupportedOperations.Neq);
                case Builtins.Gt: return Support(SupportedOperations.Gt);
                case Builtins.Lt: return Support(SupportedOperations.Lt);
                case Builtins.Gte: return Support(SupportedOperations.Gte);
                case Builtins.Lte: return Support(SupportedOperations.Lte);
                case Builtins.Neg: return Support(SupportedOperations.Neg);
                case Builtins.BitNot: return Support(SupportedOperations.BitNot);
                case Builtins.Plus: return Support(SupportedOperations.Plus);
                case Builtins.Get: return Support(SupportedOperations.Get);
                case Builtins.Set: return Support(SupportedOperations.Set);
                case Builtins.Not:
                case Builtins.ToStr:
                case Builtins.Clone:
                case Builtins.Has:
                    return DyBool.True;
                default:
                    return nameId != -1 && GetMemberDirect(self, nameId, ctx) != null ? DyBool.True : DyBool.False;
            }
        }

        internal DyObject GetMember(DyObject self, int nameId, Unit unit, ExecutionContext ctx)
        {
            nameId = unit.MemberIds[nameId];
            var value = GetMemberDirect(self, nameId, ctx);

            if (value != null)
                return value;

            return ctx.OperationNotSupported(ctx.Composition.Members[nameId], self);
        }

        internal DyObject GetMemberDirect(DyObject self, int nameId, ExecutionContext ctx)
        {
            if (SupportInstanceMembers)
            {
                var ret = self.GetOrdinal(ctx.Composition.Members[nameId]);

                if (ret != -1)
                    return self.GetItem(ret, ctx);
            }

            if (!members.TryGetValue(nameId, out var value))
            {
                var name = ctx.Composition.Members[nameId];
                value = InternalGetMember(self, name, ctx);

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

        private DyObject Clone(ExecutionContext ctx, DyObject obj) => obj.Clone();

        private DyObject Has(ExecutionContext ctx, DyObject self, DyObject member)
        {
            if (member.TypeId != DyType.String)
                return ctx.InvalidType(DyTypeNames.String, member);
            var name = member.GetString();
            if (ctx.Composition.MembersMap.TryGetValue(name, out var nameId))
                return HasMemberDirect(self, name, nameId, ctx);
            else
                return HasMemberDirect(self, name, -1, ctx);
        }

        private DyFunction InternalGetMember(DyObject self, string name, ExecutionContext ctx)
        {
            switch (name)
            {
                case Builtins.Add: return DyForeignFunction.Member(name, Add, -1, new Par("other"));
                case Builtins.Sub: return DyForeignFunction.Member(name, Sub, -1, new Par("other"));
                case Builtins.Mul: return DyForeignFunction.Member(name, Mul, -1, new Par("other"));
                case Builtins.Div: return DyForeignFunction.Member(name, Div, -1, new Par("other"));
                case Builtins.Rem: return DyForeignFunction.Member(name, Rem, -1, new Par("other"));
                case Builtins.Shl: return DyForeignFunction.Member(name, ShiftLeft, -1, new Par("other"));
                case Builtins.Shr: return DyForeignFunction.Member(name, ShiftRight, -1, new Par("other"));
                case Builtins.And: return DyForeignFunction.Member(name, And, -1, new Par("other"));
                case Builtins.Or:  return DyForeignFunction.Member(name, Or, -1, new Par("other"));
                case Builtins.Xor: return DyForeignFunction.Member(name, Xor, -1, new Par("other"));
                case Builtins.Eq:  return DyForeignFunction.Member(name, Eq, -1, new Par("other"));
                case Builtins.Neq: return DyForeignFunction.Member(name, Neq, -1, new Par("other"));
                case Builtins.Gt:  return DyForeignFunction.Member(name, Gt, -1, new Par("other"));
                case Builtins.Lt:  return DyForeignFunction.Member(name, Lt, -1, new Par("other"));
                case Builtins.Gte: return DyForeignFunction.Member(name, Gte, -1, new Par("other"));
                case Builtins.Lte: return DyForeignFunction.Member(name, Lte, -1, new Par("other"));
                case Builtins.Neg: return DyForeignFunction.Member(name, Neg);
                case Builtins.Not: return DyForeignFunction.Member(name, Not);
                case Builtins.BitNot: return DyForeignFunction.Member(name, BitwiseNot);
                case Builtins.Plus: return DyForeignFunction.Member(name, Plus);
                case Builtins.Get: return DyForeignFunction.Member(name, (context, s, index) => Get(s, index, context), -1, new Par("index"));
                case Builtins.Set: return DyForeignFunction.Member(name, (context, s, index, val) => { Set(s, index, val, context); return DyNil.Instance; }, -1, new Par("index"), new Par("value"));
                case Builtins.ToStr: return DyForeignFunction.Member(name, ToString);
                case Builtins.Iterator: return self is IEnumerable<DyObject>  ? DyForeignFunction.Member(name, GetIterator) : null;
                case Builtins.Clone: return DyForeignFunction.Member(name, Clone);
                case Builtins.Has: return DyForeignFunction.Member(name, Has, -1, new Par("member"));
                default:
                    return GetMember(name, ctx);
            }
        }

        private DyObject GetIterator(ExecutionContext ctx, DyObject self)
        {
            if (self is IEnumerable<DyObject> en)
                return new DyIterator(en);
            else
                return ctx.OperationNotSupported(Builtins.Iterator, self);
        }

        protected virtual DyFunction GetMember(string name, ExecutionContext ctx) => null;

        private DyFunction InternalGetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "__deleteMember")
                return DyForeignFunction.Static(name, (context, strObj) =>
                {
                    var nm = strObj.GetString();
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
        public DyTypeTypeInfo() : base(DyType.TypeInfo, true)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.TypeInfo;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(("typeInfo " + ((DyTypeInfo)arg).TypeName).PutInBrackets());
    }
}