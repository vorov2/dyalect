﻿using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyTypeInfo : DyObject
{
    internal bool Closed { get; set; }

    public override string TypeName => nameof(Dy.TypeInfo);
    
    protected abstract SupportedOperations GetSupportedOperations();

    private bool Support(DyObject self, SupportedOperations op) =>
        (GetSupportedOperations() & op) == op || (self.Supports() & op) == op;

    public override object ToObject() => this;

    public override string ToString() => "{" + ReflectedTypeName + "}";

    public override int GetHashCode() => HashCode.Combine(TypeId, ReflectedTypeId);

    public abstract string ReflectedTypeName { get; }

    public abstract int ReflectedTypeId { get; }

    protected DyTypeInfo() : base(Dy.TypeInfo) => AddMixin(Dy.Object);

    #region Binary Operations
    //x + y
    private DyFunction? add;
    protected virtual DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.String && left.TypeId != Dy.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);
        return ctx.OperationNotSupported(Builtins.Add, left, right);
    }
    public DyObject Add(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (add is not null)
            return add.BindToInstance(ctx, left).Call(ctx, right);

        return AddOp(ctx, left, right);
    }

    //x - y
    private DyFunction? sub;
    protected virtual DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Sub, left, right);
    public DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (sub is not null)
            return sub.BindToInstance(ctx, left).Call(ctx, right);
        return SubOp(ctx, left, right);
    }

    //x * y
    private DyFunction? mul;
    protected virtual DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Mul, left, right);
    public DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (mul is not null)
            return mul.BindToInstance(ctx, left).Call(ctx, right);
        return MulOp(ctx, left, right);
    }

    //x / y
    private DyFunction? div;
    protected virtual DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Div, left, right);
    public DyObject Div(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (div is not null)
            return div.BindToInstance(ctx, left).Call(ctx, right);
        return DivOp(ctx, left, right);
    }

    //x % y
    private DyFunction? rem;
    protected virtual DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Rem, left, right);
    public DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (rem is not null)
            return rem.BindToInstance(ctx, left).Call(ctx, right);
        return RemOp(ctx, left, right);
    }

    //x <<< y
    private DyFunction? shl;
    protected virtual DyObject ShiftLeftOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Shl, left, right);
    public DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shl is not null)
            return shl.BindToInstance(ctx, left).Call(ctx, right);
        return ShiftLeftOp(ctx, left, right);
    }

    //x >>> y
    private DyFunction? shr;
    protected virtual DyObject ShiftRightOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Shr, left, right);
    public DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shr is not null)
            return shr.BindToInstance(ctx, left).Call(ctx, right);
        return ShiftRightOp(ctx, left, right);
    }

    //x &&& y
    private DyFunction? and;
    protected virtual DyObject AndOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.And, left, right);
    public DyObject And(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (and is not null)
            return and.BindToInstance(ctx, left).Call(ctx, right);
        return AndOp(ctx, left, right);
    }

    //x ||| y
    private DyFunction? or;
    protected virtual DyObject OrOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Or, left, right);
    public DyObject Or(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (or is not null)
            return or.BindToInstance(ctx, left).Call(ctx, right);
        return OrOp(ctx, left, right);
    }

    //x ^^^ y
    private DyFunction? xor;
    protected virtual DyObject XorOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Xor, left, right);
    public DyObject Xor(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (xor is not null)
            return xor.BindToInstance(ctx, left).Call(ctx, right);
        return XorOp(ctx, left, right);
    }

    //x == y
    private DyFunction? eq;
    protected virtual DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ReferenceEquals(left, right) ? True : False;
    public DyObject Eq(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (eq is not null)
            return eq.BindToInstance(ctx, left).Call(ctx, right);
        if (right.TypeId == Dy.Bool)
            return ReferenceEquals(left, right) ? True : False;
        return EqOp(ctx, left, right);
    }

    //x != y
    private DyFunction? neq;
    protected virtual DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        Eq(ctx, left, right).IsFalse() ? True : False;
    public DyObject Neq(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (neq is not null)
            return neq.BindToInstance(ctx, left).Call(ctx, right);
        return NeqOp(ctx, left, right);
    }

    //x > y
    private DyFunction? gt;
    protected virtual DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Gt, left, right);
    public DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (gt is not null)
            return gt.BindToInstance(ctx, left).Call(ctx, right);
        return GtOp(ctx, left, right);
    }

    //x < y
    private DyFunction? lt;
    protected virtual DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Lt, left, right);
    public DyObject Lt(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (lt is not null)
            return lt.BindToInstance(ctx, left).Call(ctx, right);
        return LtOp(ctx, left, right);
    }

    //x >= y
    private DyFunction? gte;
    protected virtual DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var ret = ReferenceEquals(Gt(ctx, left, right), True)
            || ReferenceEquals(Eq(ctx, left, right), True);
        return ret ? True : False;
    }
    public DyObject Gte(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (gte is not null)
            return gte.BindToInstance(ctx, left).Call(ctx, right);
        return GteOp(ctx, left, right);
    }

    //x <= y
    private DyFunction? lte;
    protected virtual DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var ret = ReferenceEquals(Lt(ctx, left, right), True)
            || ReferenceEquals(Eq(ctx, left, right), True);
        return ret ? True : False;
    }
    public DyObject Lte(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (lte is not null)
            return lte.BindToInstance(ctx, left).Call(ctx, right);
        return LteOp(ctx, left, right);
    }
    #endregion

    #region Unary Operations
    //-x
    private DyFunction? neg;
    protected virtual DyObject NegOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.Neg, arg);
    public DyObject Neg(ExecutionContext ctx, DyObject arg)
    {
        if (neg is not null)
            return neg.BindToInstance(ctx, arg).Call(ctx);
        return NegOp(ctx, arg);
    }

    //+x
    private DyFunction? plus;
    protected virtual DyObject PlusOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.Plus, arg);
    public DyObject Plus(ExecutionContext ctx, DyObject arg)
    {
        if (plus is not null)
            return plus.BindToInstance(ctx, arg).Call(ctx);
        return PlusOp(ctx, arg);
    }

    //!x
    private DyFunction? not;
    protected virtual DyObject NotOp(ExecutionContext ctx, DyObject arg) =>
        arg.IsFalse() ? True : False;
    public DyObject Not(ExecutionContext ctx, DyObject arg)
    {
        if (not is not null)
            return not.BindToInstance(ctx, arg).Call(ctx);
        return NotOp(ctx, arg);
    }

    //~x
    private DyFunction? bitnot;
    protected virtual DyObject BitwiseNotOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.BitNot, arg);
    public DyObject BitwiseNot(ExecutionContext ctx, DyObject arg)
    {
        if (bitnot is not null)
            return bitnot.BindToInstance(ctx, arg).Call(ctx);
        return BitwiseNotOp(ctx, arg);
    }

    //x.Length
    private DyFunction? len;
    protected virtual DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.Len, arg);
    public DyObject Length(ExecutionContext ctx, DyObject arg)
    {
        if (len is not null)
            return len.BindToInstance(ctx, arg).Call(ctx);
        return LengthOp(ctx, arg);
    }

    //x.ToString
    private DyFunction? tos;
    protected virtual DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => new DyString(arg.ToString());
    internal string? ToStringDirect(ExecutionContext ctx, DyObject arg)
    {
        var res = ToStringOp(ctx, arg, Nil);

        if (ctx.HasErrors)
            return null;

        if (res.TypeId != Dy.String)
        {
            ctx.InvalidType(Dy.String, res);
            return null;
        }

        return res.GetString();
    }
    public DyObject ToString(ExecutionContext ctx, DyObject arg)
    {
        if (tos is not null)
        {
            var retval = tos.BindToInstance(ctx, arg).Call(ctx);
            return retval.TypeId == Dy.String ? retval : DyString.Empty;
        }

        return ToStringOp(ctx, arg, Nil);
    }
    public DyObject ToStringWithFormat(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        if (tos is not null)
        {
            var retval = tos.Parameters.Length == 0 ?
                tos.BindToInstance(ctx, arg).Call(ctx) : tos.BindToInstance(ctx, arg).Call(ctx, format);
            return retval.TypeId == Dy.String ? retval : DyString.Empty;
        }

        return ToStringOp(ctx, arg, format);
    }

    //x.ToLiteral
    private DyFunction? tol;
    protected virtual DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOp(ctx, arg, DyNil.Instance);
    internal string? ToLiteralDirect(ExecutionContext ctx, DyObject arg)
    {
        var res = ToLiteralOp(ctx, arg);

        if (ctx.HasErrors)
            return null;

        if (res.TypeId != Dy.String)
        {
            ctx.InvalidType(Dy.String, res);
            return null;
        }

        return res.GetString();
    }
    public DyObject ToLiteral(ExecutionContext ctx, DyObject arg)
    {
        if (tol is not null)
        {
            var retval = tol.BindToInstance(ctx, arg).Call(ctx);
            return retval.Is(Dy.String) ? retval : DyString.Empty;
        }

        return ToLiteralOp(ctx, arg);
    }

    //Clone
    private DyObject Clone(ExecutionContext ctx, DyObject obj) => obj.Clone();

    //Iterate
    private DyObject GetIterator(ExecutionContext ctx, DyObject self) => self is IEnumerable<DyObject> en
        ? DyIterator.Create(en) : ctx.OperationNotSupported(Builtins.Iterator, self);
    #endregion

    #region Other Operations
    //x[y]
    private DyFunction? get;
    protected virtual DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) => self.GetItem(index, ctx);
    internal DyObject GetDirect(ExecutionContext ctx, DyObject self, DyObject index) => GetOp(ctx, self, index);
    public DyObject Get(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (get is not null)
            return get.BindToInstance(ctx, self).Call(ctx, index);

        return GetOp(ctx, self, index);
    }

    //x[y] = z
    private DyFunction? set;
    protected virtual DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) =>
        ctx.OperationNotSupported(Builtins.Set, self);
    internal DyObject SetDirect(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) => SetOp(ctx, self, index, value);
    public DyObject Set(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        if (set is not null)
            return set.BindToInstance(ctx, self).Call(ctx, index, value);

        return SetOp(ctx, self, index, value);
    }

    //as
    private readonly Dictionary<int, DyFunction> conversions = new();
    protected virtual DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Bool => self.IsFalse() ? False : True,
            Dy.String => ToString(ctx, self),
            Dy.Char => new DyChar((ToString(ctx, self)?.GetString() ?? "\0")[0]),
            _ when targetType.ReflectedTypeId == self.TypeId => self,
            _ => ctx.InvalidCast(self.GetTypeInfo(ctx).ReflectedTypeName, targetType.ReflectedTypeName)
        };
    public DyObject Cast(ExecutionContext ctx, DyObject self, DyObject targetType)
    {
        if (targetType.TypeId != Dy.TypeInfo)
            return ctx.InvalidType(Dy.TypeInfo, targetType);

        var ti = (DyTypeInfo)targetType;

        if (ti.ReflectedTypeId == self.TypeId)
            return self;

        if (conversions.TryGetValue(ti.ReflectedTypeId, out var func))
            return func.BindToInstance(ctx, self).Call(ctx);

        return CastOp(ctx, self, (DyTypeInfo)targetType);
    }
    public void SetCastFunction(DyTypeInfo type, DyFunction func)
    {
        conversions.Remove(type.ReflectedTypeId);
        conversions.Add(type.ReflectedTypeId, func);
    }

    //Contains
    private DyFunction? contains;
    protected virtual DyObject ContainsOp(ExecutionContext ctx, DyObject self, DyObject field) =>
        ctx.OperationNotSupported(Builtins.Contains, self);
    public DyObject Contains(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (contains is not null)
            return contains.BindToInstance(ctx, self).Call(ctx, field);

        return ContainsOp(ctx, self, field);
    }
    #endregion

    #region Statics
    protected readonly Dictionary<HashString, DyFunction> StaticMembers = new();

    internal bool HasStaticMember(HashString name, ExecutionContext ctx) => LookupStaticMember(name, ctx) is not null;

    internal virtual DyObject GetStaticMember(HashString name, ExecutionContext ctx)
    {
        var ret = LookupStaticMember(name, ctx);

        if (ret is null)
            return ctx.StaticOperationNotSupported((string)name, ReflectedTypeId);

        if (ret is DyFunction f)
        {
            if (f.Private && f is DyNativeFunction n && n.UnitId != ctx.UnitId)
                return ctx.PrivateNameAccess(f.FunctionName);

            if (f.Auto)
                ret = f.BindOrRun(ctx, this);
        }

        return ret;
    }

    private DyObject? LookupStaticMember(HashString name, ExecutionContext ctx)
    {
        if (!StaticMembers.TryGetValue(name, out var value))
        {
            value = InitializeStaticMembers((string)name, ctx);

            if (value is not null)
                StaticMembers.Add(name, value);
        }

        return value;
    }

    internal virtual void SetStaticMember(ExecutionContext ctx, HashString name, DyFunction func)
    {
        if (Builtins.IsSetter(name))
        {
            var set = Builtins.GetSetterName(name);

            if (Members.TryGetValue(set, out var get) && !get.Auto)
            {
                ctx.InvalidOverload(set);
                return;
            }
        }

        if (StaticMembers.TryGetValue(name, out var oldfun))
        {
            if (oldfun.Auto != func.Auto)
            {
                ctx.InvalidOverload(name);
                return;
            }

            StaticMembers.Remove(name);
        }

        StaticMembers.Add(name, func);
    }

    private DyFunction? InitializeStaticMembers(string name, ExecutionContext ctx) =>
        name switch
        {
            "TypeInfo" => Binary(name, (c, _, obj) => c.RuntimeContext.Types[obj.TypeId], "value"),
            Builtins.Has => Binary(name, Has, "member"),
            Builtins.DelMember => Binary(name,
                (context, _, strObj) =>
                {
                    var nm = strObj.GetString();
                    SetBuiltin(ctx, nm, null);
                    Members.Remove(name);
                    StaticMembers.Remove(name);
                    return Nil;
                }, "name"),
            _ => InitializeStaticMember(name, ctx)
        };

    protected virtual DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) => null;
    #endregion

    #region Instance
    protected readonly Dictionary<HashString, DyFunction> Members = new();
    
    internal virtual bool HasInstanceMember(DyObject self, HashString name, ExecutionContext ctx) =>
        LookupInstanceMember(self, GetBuiltinName((string)name), ctx) is not null;

    internal virtual DyObject GetInstanceMember(DyObject self, HashString name, ExecutionContext ctx)
    {
        var value = LookupInstanceMember(self, name, ctx);

        if (value is not null)
            return value.BindOrRun(ctx, self);
        
        return ctx.OperationNotSupported((string)name, self);
    }

    private DyFunction? LookupInstanceMember(DyObject self, HashString name, ExecutionContext ctx)
    {
        if (!Members.TryGetValue(name, out var value))
        {
            value = InitializeInstanceMembers(self, (string)name, ctx);

            if (value is not null)
                Members.Add(name, value);
        }

        return value;
    }

    internal virtual void SetInstanceMember(ExecutionContext ctx, HashString name, DyFunction func)
    {
        if (Closed)
        {
            ctx.TypeClosed(this);
            return;
        }

        SetBuiltin(ctx, (string)name, func);

        if (Builtins.IsSetter(name))
        {
            var set = Builtins.GetSetterName(name);

            if ((set == Builtins.Len & (GetSupportedOperations() & SupportedOperations.Len) == SupportedOperations.Len)
                || set == Builtins.ToStr || (Members.TryGetValue(set, out var get) && !get.Auto))
            {
                ctx.InvalidOverload(set);
                return;
            }
        }

        if (Members.TryGetValue(name, out var oldfun))
        {
            if (oldfun.Auto != func.Auto)
            {
                ctx.InvalidOverload(name);
                return;
            }

            Members.Remove(name);
        }

        Members[name] = func;
    }

    private void SetBuiltin(ExecutionContext ctx, string name, DyFunction? func)
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
            case Builtins.Plus: plus = func; break;
            case Builtins.Set: set = func; break;
            case Builtins.Get: get = func; break;
            case Builtins.Contains:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                contains = func;
                break;
            case Builtins.Len:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                len = func;
                break;
            case Builtins.ToStr:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                tos = func; 
                break;
            case Builtins.ToLit:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                tol = func;
                break;
        }
    }

    private DyObject Has(ExecutionContext ctx, DyObject self, DyObject member)
    {
        if (member.TypeId != Dy.String)
            return ctx.InvalidType(member);

        var name = member.GetString();

        //We're calling against type itself, it means that we need to check
        // a presence of a static member
        if (self is null)
            return HasStaticMember(name, ctx) ? True : False;
        
        return HasInstanceMember(self, name, ctx) ? True : False;
    }

    private static DyFunction Ternary(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par par1, Par par2) =>
        new DyTernaryFunction(name, fun, par1, par2);

    private static DyFunction Binary(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par par = default) =>
        new DyBinaryFunction(name, fun, par.Name is null ? new Par("other") : par);

    private static DyFunction Unary(string name, Func<ExecutionContext, DyObject, DyObject> fun) =>
        new DyUnaryFunction(name, fun);

    private DyFunction? InitializeInstanceMembers(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Builtins.Add => Support(self, SupportedOperations.Add) ? Binary(name, Add) : null,
            Builtins.Sub => Support(self, SupportedOperations.Sub) ? Binary(name, Sub) : null,
            Builtins.Mul => Support(self, SupportedOperations.Mul) ? Binary(name, Mul) : null,
            Builtins.Div => Support(self, SupportedOperations.Div) ? Binary(name, Div) : null,
            Builtins.Rem => Support(self, SupportedOperations.Rem) ? Binary(name, Rem) : null,
            Builtins.Shl => Support(self, SupportedOperations.Shl) ? Binary(name, ShiftLeft) : null,
            Builtins.Shr => Support(self, SupportedOperations.Shr) ? Binary(name, ShiftRight) : null,
            Builtins.And => Support(self, SupportedOperations.And) ? Binary(name, And) : null,
            Builtins.Or => Support(self, SupportedOperations.Or) ? Binary(name, Or) : null,
            Builtins.Xor => Support(self, SupportedOperations.Xor) ? Binary(name, Xor) : null,
            Builtins.Eq => Binary(name, Eq),
            Builtins.Neq => Binary(name, Neq),
            Builtins.Gt => Support(self, SupportedOperations.Gt) ? Binary(name, Gt) : null,
            Builtins.Lt => Support(self, SupportedOperations.Lt) ? Binary(name, Lt) : null,
            Builtins.Gte => Support(self, SupportedOperations.Gte) ? Binary(name, Gte) : null,
            Builtins.Lte => Support(self, SupportedOperations.Lte) ? Binary(name, Lte) : null,
            Builtins.Neg => Support(self, SupportedOperations.Neg) ? Unary(name, Neg) : null,
            Builtins.Not => Unary(name, Not),
            Builtins.BitNot => Support(self, SupportedOperations.BitNot) ? Unary(name, BitwiseNot) : null,
            Builtins.Plus => Support(self, SupportedOperations.Plus) ? Unary(name, Plus) : null,
            Builtins.Get => Support(self, SupportedOperations.Get) ? Binary(name, Get, "index") : null,
            Builtins.Set => Support(self, SupportedOperations.Set) ? Ternary(name, Set, "index", "value") : null,
            Builtins.Len => Support(self, SupportedOperations.Len) ? Unary(name, Length) : null,
            Builtins.ToStr => Binary(name, ToStringWithFormat, new Par("format", Nil)),
            Builtins.ToLit => Support(self, SupportedOperations.Lit) ? Unary(name, ToLiteral) : null,
            Builtins.Iterator => Support(self, SupportedOperations.Iter) ? Unary(name, GetIterator) : null,
            Builtins.Clone => Unary(name, Clone),
            Builtins.Has => Binary(name, Has, "member"),
            Builtins.Type => Unary(name, (ct, o) => ct.RuntimeContext.Types[o.TypeId]),
            Builtins.Contains => Binary(name, Contains, "value"),
            _ => InitializeInstanceMember(self, name, ctx)
        };

    private static string GetBuiltinName(string name) =>
        name switch
        {
            "+" => Builtins.Add,
            "-" => Builtins.Sub,
            "*" => Builtins.Mul,
            "/" => Builtins.Div,
            "%" => Builtins.Rem,
            "<<<" => Builtins.Shl,
            ">>>" => Builtins.Shr,
            "^^^" => Builtins.Xor,
            "==" => Builtins.Eq,
            "!=" => Builtins.Neq,
            ">" => Builtins.Gt,
            "<" => Builtins.Lt,
            ">=" => Builtins.Gte,
            "<=" => Builtins.Lte,
            "!" => Builtins.Not,
            "~~~" => Builtins.BitNot,
            "|||" => Builtins.BitOr,
            "&&&" => Builtins.BitAnd,
            _ => name
        };

    protected virtual DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) => null;
    #endregion

    #region Mixins
    private readonly HashSet<int> mixins = new();
    internal void Mixin(ExecutionContext ctx, DyTypeInfo typeInfo)
    {
        if (mixins.Contains(typeInfo.ReflectedTypeId))
            return;

        foreach (var kv in typeInfo.Members)
        {
            SetBuiltin(ctx, (string)kv.Key, kv.Value);
            Members[kv.Key] = kv.Value;
        }

        mixins.Add(typeInfo.ReflectedTypeId);
        typeInfo.Closed = true;
    }

    protected void AddMixin(int typeId) => mixins.Add(typeId);

    protected void AddMixin(int typeId1, int typeId2)
    {
        mixins.Add(typeId1);
        mixins.Add(typeId2);
    }

    protected void AddMixin(int typeId1, int typeId2, int typeId3)
    {
        mixins.Add(typeId1);
        mixins.Add(typeId2);
        mixins.Add(typeId3);
    }

    protected void AddDefaultMixin1(string name) =>
        Members.Add(name, new DyUnaryFunction(name, (ctx, _) => ctx.NotImplemented(name)));

    protected void AddDefaultMixin2(string name, string p1) =>
        Members.Add(name, new DyBinaryFunction(name, (ctx, _, _) => ctx.NotImplemented(name), p1));

    protected void AddDefaultPropertyMixin(string name) =>
        Members.Add(name, new DyUnaryFunction(name, (ctx, _) => ctx.NotImplemented(name), isPropertyGetter: true));

    internal bool CheckType(DyTypeInfo typeInfo) =>
        ReflectedTypeId == typeInfo.ReflectedTypeId || mixins.Contains(typeInfo.ReflectedTypeId);
    #endregion
}
