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
            None = 0xFF,
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
            Set =  0x400000,
            Len =  0x800000,
            Iter = 0x1000000
        }

        protected abstract SupportedOperations GetSupportedOperations();

        private bool Support(SupportedOperations op) => (GetSupportedOperations() & op) == op;

        public override object ToObject() => this;

        public override string ToString() => TypeName.PutInBrackets();

        public abstract string TypeName { get; }

        public int TypeCode { get; internal set; }

        protected DyTypeInfo(int typeCode) : base(DyType.TypeInfo) =>
            TypeCode = typeCode;

        #region Binary Operations
        //x + y
        private DyFunction? add;
        protected virtual DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId == DyType.String && TypeCode != DyType.String)
                return ctx.RuntimeContext.Types[DyType.String].Add(ctx, left, right);
            return ctx.OperationNotSupported(Builtins.Add, left.GetTypeName(ctx));
        }

        public DyObject Add(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (add is not null)
                return add.BindToInstance(ctx, left).Call(ctx, right);

            return AddOp(left, right, ctx);
        }

        //x - y
        private DyFunction? sub;
        protected virtual DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Sub, left.GetTypeName(ctx));
        public DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (sub is not null)
                return sub.BindToInstance(ctx, left).Call(ctx, right);
            return SubOp(left, right, ctx);
        }

        //x * y
        private DyFunction? mul;
        protected virtual DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Mul, left.GetTypeName(ctx));
        public DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (mul is not null)
                return mul.BindToInstance(ctx, left).Call(ctx, right);
            return MulOp(left, right, ctx);
        }

        //x / y
        private DyFunction? div;
        protected virtual DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Div, left.GetTypeName(ctx));
        public DyObject Div(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (div is not null)
                return div.BindToInstance(ctx, left).Call(ctx, right);
            return DivOp(left, right, ctx);
        }

        //x % y
        private DyFunction? rem;
        protected virtual DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Rem, left.GetTypeName(ctx));
        public DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (rem is not null)
                return rem.BindToInstance(ctx, left).Call(ctx, right);
            return RemOp(left, right, ctx);
        }

        //x << y
        private DyFunction? shl;
        protected virtual DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Shl, left.GetTypeName(ctx));
        public DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (shl is not null)
                return shl.BindToInstance(ctx, left).Call(ctx, right);
            return ShiftLeftOp(left, right, ctx);
        }

        //x >> y
        private DyFunction? shr;
        protected virtual DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Shr, left.GetTypeName(ctx));
        public DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (shr is not null)
                return shr.BindToInstance(ctx, left).Call(ctx, right);
            return ShiftRightOp(left, right, ctx);
        }

        //x & y
        private DyFunction? and;
        protected virtual DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.And, left.GetTypeName(ctx));
        public DyObject And(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (and is not null)
                return and.BindToInstance(ctx, left).Call(ctx, right);
            return AndOp(left, right, ctx);
        }

        //x | y
        private DyFunction? or;
        protected virtual DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Or, left.GetTypeName(ctx));
        public DyObject Or(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (or is not null)
                return or.BindToInstance(ctx, left).Call(ctx, right);
            return OrOp(left, right, ctx);
        }

        //x ^ y
        private DyFunction? xor;
        protected virtual DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Xor, left.GetTypeName(ctx));
        public DyObject Xor(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (xor is not null)
                return xor.BindToInstance(ctx, left).Call(ctx, right);
            return XorOp(left, right, ctx);
        }

        //x == y
        private DyFunction? eq;
        protected virtual DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ReferenceEquals(left, right) ? DyBool.True : DyBool.False;
        public DyObject Eq(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (eq is not null)
                return eq.BindToInstance(ctx, left).Call(ctx, right);
            if (right.TypeId is DyType.Bool)
                return left.GetBool() == right.GetBool() ? DyBool.True : DyBool.False;
            return EqOp(left, right, ctx);
        }

        //x != y
        private DyFunction? neq;
        protected virtual DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            Eq(ctx, left, right) == DyBool.True ? DyBool.False : DyBool.True;
        public DyObject Neq(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (neq is not null)
                return neq.BindToInstance(ctx, left).Call(ctx, right);
            return NeqOp(left, right, ctx);
        }

        //x > y
        private DyFunction? gt;
        protected virtual DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Gt, left.GetTypeName(ctx));
        public DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (gt is not null)
                return gt.BindToInstance(ctx, left).Call(ctx, right);
            return GtOp(left, right, ctx);
        }

        //x < y
        private DyFunction? lt;
        protected virtual DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Lt, left.GetTypeName(ctx));
        public DyObject Lt(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (lt is not null)
                return lt.BindToInstance(ctx, left).Call(ctx, right);
            return LtOp(left, right, ctx);
        }

        //x >= y
        private DyFunction? gte;
        protected virtual DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = ReferenceEquals(Gt(ctx, left, right), DyBool.True)
                || ReferenceEquals(Eq(ctx, left, right), DyBool.True);
            return ret ? DyBool.True : DyBool.False;
        }
        public DyObject Gte(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (gte is not null)
                return gte.BindToInstance(ctx, left).Call(ctx, right);
            return GteOp(left, right, ctx);
        }

        //x <= y
        private DyFunction? lte;
        protected virtual DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            var ret = ReferenceEquals(Lt(ctx, left, right), DyBool.True)
                || ReferenceEquals(Eq(ctx, left, right), DyBool.True);
            return ret ? DyBool.True : DyBool.False;
        }
        public DyObject Lte(ExecutionContext ctx, DyObject left, DyObject right)
        {
            if (lte is not null)
                return lte.BindToInstance(ctx, left).Call(ctx, right);
            return LteOp(left, right, ctx);
        }
        #endregion

        #region Unary Operations
        //-x
        private DyFunction? neg;
        protected virtual DyObject NegOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Neg, arg.GetTypeName(ctx));
        public DyObject Neg(ExecutionContext ctx, DyObject arg)
        {
            if (neg is not null)
                return neg.BindToInstance(ctx, arg).Call(ctx);
            return NegOp(arg, ctx);
        }

        //+x
        private DyFunction? plus;
        protected virtual DyObject PlusOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Plus, arg.GetTypeName(ctx));
        public DyObject Plus(ExecutionContext ctx, DyObject arg)
        {
            if (plus is not null)
                return plus.BindToInstance(ctx, arg).Call(ctx);
            return PlusOp(arg, ctx);
        }

        //!x
        private DyFunction? not;
        protected virtual DyObject NotOp(DyObject arg, ExecutionContext ctx) =>
            arg.GetBool() ? DyBool.False : DyBool.True;
        public DyObject Not(ExecutionContext ctx, DyObject arg)
        {
            if (not is not null)
                return not.BindToInstance(ctx, arg).Call(ctx);
            return NotOp(arg, ctx);
        }

        //~x
        private DyFunction? bitnot;
        protected virtual DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.BitNot, arg.GetTypeName(ctx));
        public DyObject BitwiseNot(ExecutionContext ctx, DyObject arg)
        {
            if (bitnot is not null)
                return bitnot.BindToInstance(ctx, arg).Call(ctx);
            return BitwiseNotOp(arg, ctx);
        }

        //x.len
        private DyFunction? len;
        protected virtual DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Len, arg.GetTypeName(ctx));
        public DyObject Length(ExecutionContext ctx, DyObject arg)
        {
            if (len is not null)
                return len.BindToInstance(ctx, arg).Call(ctx);
            return LengthOp(arg, ctx);
        }

        //x.toString
        private DyFunction? tos;
        protected virtual DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString(arg.ToString());
        public DyObject ToString(ExecutionContext ctx, DyObject arg)
        {
            if (tos is not null)
            {
                var retval = tos.BindToInstance(ctx, arg).Call(ctx);
                return retval.TypeId is DyType.String ? (DyString)retval : DyString.Empty;
            }

            return ToStringOp(arg, ctx);
        }
        #endregion

        #region Other Operations
        //x[y]
        private DyFunction? get;
        protected virtual DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Get, self.GetTypeName(ctx));
        public DyObject Get(ExecutionContext ctx, DyObject self, DyObject index)
        {
            if (get is not null)
                return get.BindToInstance(ctx, self).Call(ctx, index);

            return GetOp(self, index, ctx);
        }

        //x[y] = z
        private DyFunction? set;
        protected virtual DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) =>
            ctx.OperationNotSupported(Builtins.Set, self.GetTypeName(ctx));
        public DyObject Set(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
        {
            if (set is not null)
                return set.BindToInstance(ctx, self).Call(ctx, index, value);

            return SetOp(self, index, value, ctx);
        }
        #endregion

        #region Statics
        private readonly Dictionary<string, DyObject> staticMembers = new();

        internal bool HasStaticMember(string name, ExecutionContext ctx) => LookupStaticMember(name, ctx) is not null;

        internal DyObject GetStaticMember(string name, ExecutionContext ctx)
        {
            var ret = LookupStaticMember(name, ctx);

            if (ret is null)
                return ctx.OperationNotSupported("static " + name, TypeName);

            if (ret is DyFunction f && f.Auto)
                ret = f.BindOrRun(ctx, this);

            return ret;
        }

        private DyObject? LookupStaticMember(string name, ExecutionContext ctx)
        {
            if (!staticMembers.TryGetValue(name, out var value))
            {
                value = InitializeStaticMembers(name, ctx);

                if (value is not null)
                    staticMembers.Add(name, value);
            }

            return value;
        }

        internal void SetStaticMember(string name, DyObject value)
        {
            staticMembers.Remove(name);

            if (value is DyFunction func)
                staticMembers.Add(name, func);
        }

        private DyObject? InitializeStaticMembers(string name, ExecutionContext ctx) =>
            name switch
            {
                "TypeInfo" => Func.Static(name, (c, obj) => c.RuntimeContext.Types[obj.TypeId], -1, new Par("value")),
                "has" => Func.Member(name, Has, -1, new Par("member")),
                "id" => Func.Auto(name, (ctx, self) => DyInteger.Get(((DyTypeInfo)self).TypeCode)),
                "name" => Func.Auto(name, (ctx, self) => new DyString(((DyTypeInfo)self).TypeName)),
                "__deleteMember" => Func.Static(name,
                    (context, strObj) =>
                    {
                        var nm = strObj.GetString();
                        if (nm is "getItem") nm = "op_get";
                        if (nm is "setItem") nm = "op_set";
                        SetBuiltin(nm, null);
                        Members.Remove(name);
                        staticMembers.Remove(name);
                        return DyNil.Instance;
                    }, -1, new Par("name")),
                _ => InitializeStaticMember(name, ctx)
            };

        protected virtual DyObject? InitializeStaticMember(string name, ExecutionContext ctx) => null;
        #endregion

        #region Instance
        protected readonly Dictionary<string, DyFunction> Members = new();

        internal bool HasInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            LookupInstanceMember(self, name, ctx) is not null;

        internal DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var value = LookupInstanceMember(self, name, ctx);

            if (value is not null)
                return value.BindOrRun(ctx, self);
            
            return ctx.OperationNotSupported(name, self.GetTypeName(ctx));
        }

        private DyFunction? LookupInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            if (!Members.TryGetValue(name, out var value))
            {
                value = InitializeInstanceMembers(self, name, ctx);

                if (value is not null)
                    Members.Add(name, value);
            }

            return value;
        }

        internal void SetInstanceMember(ExecutionContext ctx, string name, DyObject value)
        {
            var func = value as DyFunction;
            SetBuiltin(name, func);
            Members.Remove(name);

            if (func is not null)
                Members[name] = func;
        }

        private void SetBuiltin(string name, DyFunction? func)
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
                case Builtins.Set: set = func; break;
                case Builtins.Get: get = func; break;
            }
        }

        private DyObject Has(ExecutionContext ctx, DyObject self, DyObject member)
        {
            if (member.TypeId is not DyType.String)
                return ctx.InvalidType(member);

            var name = member.GetString();

            //We're calling against type itself, it means that we need to check
            // a presence of a static member
            if (self is null)
                return (DyBool)HasStaticMember(name, ctx);
            
            return (DyBool)HasInstanceMember(self, name, ctx);
        }

        private DyFunction? InitializeInstanceMembers(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                Builtins.Add => Support(SupportedOperations.Add) ? Func.Member(name, Add, -1, new Par("other")) : null,
                Builtins.Sub => Support(SupportedOperations.Sub) ? Func.Member(name, Sub, -1, new Par("other")) : null,
                Builtins.Mul => Support(SupportedOperations.Mul) ? Func.Member(name, Mul, -1, new Par("other")) : null,
                Builtins.Div => Support(SupportedOperations.Div) ? Func.Member(name, Div, -1, new Par("other")) : null,
                Builtins.Rem => Support(SupportedOperations.Rem) ? Func.Member(name, Rem, -1, new Par("other")) : null,
                Builtins.Shl => Support(SupportedOperations.Shl) ? Func.Member(name, ShiftLeft, -1, new Par("other")) : null,
                Builtins.Shr => Support(SupportedOperations.Shr) ? Func.Member(name, ShiftRight, -1, new Par("other")) : null,
                Builtins.And => Support(SupportedOperations.And) ? Func.Member(name, And, -1, new Par("other")) : null,
                Builtins.Or => Support(SupportedOperations.Or) ? Func.Member(name, Or, -1, new Par("other")) : null,
                Builtins.Xor => Support(SupportedOperations.Xor) ? Func.Member(name, Xor, -1, new Par("other")) : null,
                Builtins.Eq => Func.Member(name, Eq, -1, new Par("other")),
                Builtins.Neq => Func.Member(name, Neq, -1, new Par("other")),
                Builtins.Gt => Support(SupportedOperations.Gt) ? Func.Member(name, Gt, -1, new Par("other")) : null,
                Builtins.Lt => Support(SupportedOperations.Lt) ? Func.Member(name, Lt, -1, new Par("other")) : null,
                Builtins.Gte => Support(SupportedOperations.Gte) ? Func.Member(name, Gte, -1, new Par("other")) : null,
                Builtins.Lte => Support(SupportedOperations.Lte) ? Func.Member(name, Lte, -1, new Par("other")) : null,
                Builtins.Neg => Support(SupportedOperations.Neg) ? Func.Member(name, Neg) : null,
                Builtins.Not => Func.Member(name, Not),
                Builtins.BitNot => Support(SupportedOperations.BitNot) ? Func.Member(name, BitwiseNot) : null,
                Builtins.Plus => Support(SupportedOperations.Plus) ? Func.Member(name, Plus) : null,
                Builtins.Get or "getItem" => Support(SupportedOperations.Get) ? Func.Member(name, Get, -1, new Par("index")) : null,
                Builtins.Set or "setItem" => Support(SupportedOperations.Set) ? Func.Member(name, Set, -1, new Par("index"), new Par("value")) : null,
                Builtins.Len => Support(SupportedOperations.Len) ? Func.Member(name, Length) : null,
                Builtins.ToStr => Func.Member(name, ToString),
                Builtins.Iterator => Support(SupportedOperations.Iter) ? Func.Member(name, GetIterator) : null,
                Builtins.Clone => Func.Member(name, Clone),
                Builtins.Has => Func.Member(name, Has, -1, new Par("member")),
                Builtins.Type => Func.Member(name, (context, o) => context.RuntimeContext.Types[TypeCode]),
                _ => InitializeInstanceMember(self, name, ctx)
            };

        protected virtual DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) => null;
        #endregion

        private DyObject Clone(ExecutionContext ctx, DyObject obj) => obj.Clone();

        private DyObject GetIterator(ExecutionContext ctx, DyObject self) => 
            self is IEnumerable<DyObject> en 
            ? DyIterator.Create(en)
            : ctx.OperationNotSupported(Builtins.Iterator, self.GetTypeName(ctx));
        
        public override int GetHashCode() => TypeCode.GetHashCode();
    }
}