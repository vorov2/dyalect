using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyTypeInfo : DyObject
{
    internal bool Closed { get; set; }

    public override string TypeName => DyTypeNames.TypeInfo;
    
    protected abstract SupportedOperations GetSupportedOperations();

    private bool Support(DyObject self, SupportedOperations op) =>
        (GetSupportedOperations() & op) == op || (self.Supports() & op) == op;

    public override object ToObject() => this;

    public override string ToString() => "{" + ReflectedTypeName + "}";

    public abstract string ReflectedTypeName { get; }

    public abstract int ReflectedTypeId { get; }

    protected DyTypeInfo() : base(DyType.TypeInfo) => AddMixin(DyType.Object);

    #region Binary Operations
    //x + y
    private DyFunction? add;
    protected virtual DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == DyType.String && left.TypeId != DyType.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);
        return ctx.OperationNotSupported(Builtins.Add, left, right);
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
        ctx.OperationNotSupported(Builtins.Sub, left, right);
    public DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (sub is not null)
            return sub.BindToInstance(ctx, left).Call(ctx, right);
        return SubOp(left, right, ctx);
    }

    //x * y
    private DyFunction? mul;
    protected virtual DyObject MulOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Mul, left, right);
    public DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (mul is not null)
            return mul.BindToInstance(ctx, left).Call(ctx, right);
        return MulOp(left, right, ctx);
    }

    //x / y
    private DyFunction? div;
    protected virtual DyObject DivOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Div, left, right);
    public DyObject Div(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (div is not null)
            return div.BindToInstance(ctx, left).Call(ctx, right);
        return DivOp(left, right, ctx);
    }

    //x % y
    private DyFunction? rem;
    protected virtual DyObject RemOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Rem, left, right);
    public DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (rem is not null)
            return rem.BindToInstance(ctx, left).Call(ctx, right);
        return RemOp(left, right, ctx);
    }

    //x <<< y
    private DyFunction? shl;
    protected virtual DyObject ShiftLeftOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Shl, left, right);
    public DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shl is not null)
            return shl.BindToInstance(ctx, left).Call(ctx, right);
        return ShiftLeftOp(left, right, ctx);
    }

    //x >>> y
    private DyFunction? shr;
    protected virtual DyObject ShiftRightOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Shr, left, right);
    public DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shr is not null)
            return shr.BindToInstance(ctx, left).Call(ctx, right);
        return ShiftRightOp(left, right, ctx);
    }

    //x &&& y
    private DyFunction? and;
    protected virtual DyObject AndOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.And, left, right);
    public DyObject And(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (and is not null)
            return and.BindToInstance(ctx, left).Call(ctx, right);
        return AndOp(left, right, ctx);
    }

    //x ||| y
    private DyFunction? or;
    protected virtual DyObject OrOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Or, left, right);
    public DyObject Or(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (or is not null)
            return or.BindToInstance(ctx, left).Call(ctx, right);
        return OrOp(left, right, ctx);
    }

    //x ^^^ y
    private DyFunction? xor;
    protected virtual DyObject XorOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Xor, left, right);
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
        if (right.TypeId == DyType.Bool)
            return ReferenceEquals(left, right) ? DyBool.True : DyBool.False;
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
        ctx.OperationNotSupported(Builtins.Gt, left, right);
    public DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (gt is not null)
            return gt.BindToInstance(ctx, left).Call(ctx, right);
        return GtOp(left, right, ctx);
    }

    //x < y
    private DyFunction? lt;
    protected virtual DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Lt, left, right);
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
        ctx.OperationNotSupported(Builtins.Neg, arg);
    public DyObject Neg(ExecutionContext ctx, DyObject arg)
    {
        if (neg is not null)
            return neg.BindToInstance(ctx, arg).Call(ctx);
        return NegOp(arg, ctx);
    }

    //+x
    private DyFunction? plus;
    protected virtual DyObject PlusOp(DyObject arg, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Plus, arg);
    public DyObject Plus(ExecutionContext ctx, DyObject arg)
    {
        if (plus is not null)
            return plus.BindToInstance(ctx, arg).Call(ctx);
        return PlusOp(arg, ctx);
    }

    //!x
    private DyFunction? not;
    protected virtual DyObject NotOp(DyObject arg, ExecutionContext ctx) =>
        arg.IsFalse() ? DyBool.True : DyBool.False;
    public DyObject Not(ExecutionContext ctx, DyObject arg)
    {
        if (not is not null)
            return not.BindToInstance(ctx, arg).Call(ctx);
        return NotOp(arg, ctx);
    }

    //~x
    private DyFunction? bitnot;
    protected virtual DyObject BitwiseNotOp(DyObject arg, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.BitNot, arg);
    public DyObject BitwiseNot(ExecutionContext ctx, DyObject arg)
    {
        if (bitnot is not null)
            return bitnot.BindToInstance(ctx, arg).Call(ctx);
        return BitwiseNotOp(arg, ctx);
    }

    //x.Length
    private DyFunction? len;
    protected virtual DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Len, arg);
    public DyObject Length(ExecutionContext ctx, DyObject arg)
    {
        if (len is not null)
            return len.BindToInstance(ctx, arg).Call(ctx);
        return LengthOp(arg, ctx);
    }

    //x.ToString
    private DyFunction? tos;
    protected virtual DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(arg.ToString());
    internal string? ToStringDirect(ExecutionContext ctx, DyObject arg)
    {
        var res = ToStringOp(arg, Default(), ctx);

        if (ctx.HasErrors)
            return null;

        if (res.TypeId != DyType.String)
        {
            ctx.InvalidType(DyType.String, res);
            return null;
        }

        return res.GetString();
    }
    public DyObject ToString(ExecutionContext ctx, DyObject arg)
    {
        if (tos is not null)
        {
            var retval = tos.BindToInstance(ctx, arg).Call(ctx);
            return retval.TypeId == DyType.String ? retval : DyString.Empty;
        }

        return ToStringOp(arg, Default(), ctx);
    }
    public DyObject ToStringWithFormat(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        if (tos is not null)
        {
            var retval = tos.Parameters.Length == 0 ?
                tos.BindToInstance(ctx, arg).Call(ctx) : tos.BindToInstance(ctx, arg).Call(ctx, format);
            return retval.TypeId == DyType.String ? retval : DyString.Empty;
        }

        return ToStringOp(arg, format, ctx);
    }

    //x.ToLiteral
    private DyFunction? tol;
    protected virtual DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToStringOp(arg, DyNil.Instance, ctx);
    internal string? ToLiteralDirect(ExecutionContext ctx, DyObject arg)
    {
        var res = ToLiteralOp(arg, ctx);

        if (ctx.HasErrors)
            return null;

        if (res.TypeId != DyType.String)
        {
            ctx.InvalidType(DyType.String, res);
            return null;
        }

        return res.GetString();
    }
    public DyObject ToLiteral(ExecutionContext ctx, DyObject arg)
    {
        if (tol is not null)
        {
            var retval = tol.BindToInstance(ctx, arg).Call(ctx);
            return retval.TypeId == DyType.String ? retval : DyString.Empty;
        }

        return ToLiteralOp(arg, ctx);
    }
    #endregion

    #region Other Operations
    //x[y]
    private DyFunction? get;
    protected virtual DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);
    internal DyObject GetDirect(ExecutionContext ctx, DyObject self, DyObject index) => GetOp(self, index, ctx);
    public DyObject Get(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (get is not null)
            return get.BindToInstance(ctx, self).Call(ctx, index);

        return GetOp(self, index, ctx);
    }

    //x[y] = z
    private DyFunction? set;
    protected virtual DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Set, self);
    internal DyObject SetDirect(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) => SetOp(self, index, value, ctx);
    public DyObject Set(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        if (set is not null)
            return set.BindToInstance(ctx, self).Call(ctx, index, value);

        return SetOp(self, index, value, ctx);
    }

    //as
    private readonly Dictionary<int, DyFunction> conversions = new();
    protected virtual DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Bool => self.IsFalse() ? DyBool.False : DyBool.True,
            DyType.String => ToString(ctx, self),
            DyType.Char => new DyChar((ToString(ctx, self)?.GetString() ?? "\0")[0]),
            _ when targetType.ReflectedTypeId == self.TypeId => self,
            _ => ctx.InvalidCast(self.GetTypeInfo(ctx).ReflectedTypeName, targetType.ReflectedTypeName)
        };
    public DyObject Cast(ExecutionContext ctx, DyObject self, DyObject targetType)
    {
        if (targetType.TypeId != DyType.TypeInfo)
            return ctx.InvalidType(DyType.TypeInfo, targetType);

        var ti = (DyTypeInfo)targetType;

        if (ti.ReflectedTypeId == self.TypeId)
            return self;

        if (conversions.TryGetValue(ti.ReflectedTypeId, out var func))
            return func.BindToInstance(ctx, self).Call(ctx);

        return CastOp(self, (DyTypeInfo)targetType, ctx);
    }
    public void SetCastFunction(DyTypeInfo type, DyFunction func)
    {
        conversions.Remove(type.ReflectedTypeId);
        conversions.Add(type.ReflectedTypeId, func);
    }

    //Contains
    private DyFunction? contains;
    protected virtual DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx) =>
        ctx.OperationNotSupported(Builtins.Contains, self);
    public DyObject Contains(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (contains is not null)
            return contains.BindToInstance(ctx, self).Call(ctx, field);

        return ContainsOp(self, field, ctx);
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
            "TypeInfo" => Func.Static(name, (c, obj) => c.RuntimeContext.Types[obj.TypeId], -1, new Par("value")),
            Builtins.Has => Func.Member(name, Has, -1, new Par("member")),
            Builtins.DelMember => Func.Static(name,
                (context, strObj) =>
                {
                    var nm = strObj.GetString();
                    SetBuiltin(ctx, nm, null);
                    Members.Remove(name);
                    StaticMembers.Remove(name);
                    return Default();
                }, -1, new Par("name")),
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

    protected void AddDefaultMixin1(string name)
    {
        Members.Add(name, Func.Member(name, (ctx, _) => ctx.NotImplemented(name)));
    }

    protected void AddDefaultMixin2(string name, string p1)
    {
        Members.Add(name, Func.Member(name, 
            (ExecutionContext ctx, DyObject _, DyObject _) => ctx.NotImplemented(name), -1, new Par(p1)));
    }

    internal bool CheckType(DyTypeInfo typeInfo) => ReflectedTypeId == typeInfo.ReflectedTypeId || mixins.Contains(typeInfo.ReflectedTypeId);

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
        if (member.TypeId != DyType.String)
            return ctx.InvalidType(member);

        var name = member.GetString();

        //We're calling against type itself, it means that we need to check
        // a presence of a static member
        if (self is null)
            return HasStaticMember(name, ctx) ? DyBool.True : DyBool.False;
        
        return HasInstanceMember(self, name, ctx) ? DyBool.True : DyBool.False;
    }

    private DyFunction? InitializeInstanceMembers(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Builtins.Add => Support(self, SupportedOperations.Add) ? Func.Member(name, Add, -1, new Par("other")) : null,
            Builtins.Sub => Support(self, SupportedOperations.Sub) ? Func.Member(name, Sub, -1, new Par("other")) : null,
            Builtins.Mul => Support(self, SupportedOperations.Mul) ? Func.Member(name, Mul, -1, new Par("other")) : null,
            Builtins.Div => Support(self, SupportedOperations.Div) ? Func.Member(name, Div, -1, new Par("other")) : null,
            Builtins.Rem => Support(self, SupportedOperations.Rem) ? Func.Member(name, Rem, -1, new Par("other")) : null,
            Builtins.Shl => Support(self, SupportedOperations.Shl) ? Func.Member(name, ShiftLeft, -1, new Par("other")) : null,
            Builtins.Shr => Support(self, SupportedOperations.Shr) ? Func.Member(name, ShiftRight, -1, new Par("other")) : null,
            Builtins.And => Support(self, SupportedOperations.And) ? Func.Member(name, And, -1, new Par("other")) : null,
            Builtins.Or => Support(self, SupportedOperations.Or) ? Func.Member(name, Or, -1, new Par("other")) : null,
            Builtins.Xor => Support(self, SupportedOperations.Xor) ? Func.Member(name, Xor, -1, new Par("other")) : null,
            Builtins.Eq => Func.Member(name, Eq, -1, new Par("other")),
            Builtins.Neq => Func.Member(name, Neq, -1, new Par("other")),
            Builtins.Gt => Support(self, SupportedOperations.Gt) ? Func.Member(name, Gt, -1, new Par("other")) : null,
            Builtins.Lt => Support(self, SupportedOperations.Lt) ? Func.Member(name, Lt, -1, new Par("other")) : null,
            Builtins.Gte => Support(self, SupportedOperations.Gte) ? Func.Member(name, Gte, -1, new Par("other")) : null,
            Builtins.Lte => Support(self, SupportedOperations.Lte) ? Func.Member(name, Lte, -1, new Par("other")) : null,
            Builtins.Neg => Support(self, SupportedOperations.Neg) ? Func.Member(name, Neg) : null,
            Builtins.Not => Func.Member(name, Not),
            Builtins.BitNot => Support(self, SupportedOperations.BitNot) ? Func.Member(name, BitwiseNot) : null,
            Builtins.Plus => Support(self, SupportedOperations.Plus) ? Func.Member(name, Plus) : null,
            Builtins.Get => Support(self, SupportedOperations.Get) ? Func.Member(name, Get, -1, new Par("index")) : null,
            Builtins.Set => Support(self, SupportedOperations.Set) ? Func.Member(name, Set, -1, new Par("index"), new Par("value")) : null,
            Builtins.Len => Support(self, SupportedOperations.Len) ? Func.Member(name, Length) : null,
            Builtins.ToStr => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
            Builtins.ToLit => Support(self, SupportedOperations.Lit) ? Func.Member(name, ToLiteral) : null,
            Builtins.Iterator => Support(self, SupportedOperations.Iter) ? Func.Member(name, GetIterator) : null,
            Builtins.Clone => Func.Member(name, Clone),
            Builtins.Has => Func.Member(name, Has, -1, new Par("member")),
            Builtins.Type => Func.Member(name, (ct, o) => ct.RuntimeContext.Types[o.TypeId]),
            Builtins.Contains => Func.Member(name, Contains, -1, new Par("value")),
            _ => InitializeInstanceMember(self, name, ctx)
        };

    private string GetBuiltinName(string name) =>
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

    protected DyObject Default() => DyNil.Instance;

    protected DyObject Nil => DyNil.Instance;

    private DyObject Clone(ExecutionContext ctx, DyObject obj) => obj.Clone();

    private DyObject GetIterator(ExecutionContext ctx, DyObject self) => self is IEnumerable<DyObject> en 
        ? DyIterator.Create(en) : ctx.OperationNotSupported(Builtins.Iterator, self);

    public override int GetHashCode() => HashCode.Combine(TypeId, TypeId, ReflectedTypeName);
}
