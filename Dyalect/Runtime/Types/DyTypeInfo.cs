using Dyalect.Compiler;
using Dyalect.Debug;
using System.Collections.Generic;
namespace Dyalect.Runtime.Types;

public abstract class DyTypeInfo : DyObject
{
    internal bool Closed { get; set; }

    public override string TypeName => nameof(Dy.TypeInfo);
    
    protected virtual SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    private bool Support(DyObject self, SupportedOperations op) =>
        (GetSupportedOperations() & op) == op || (self.Supports() & op) == op;

    public override object ToObject() => this;

    public override string ToString() => $"TypeInfo<{ReflectedTypeName}>";

    public override int GetHashCode() => HashCode.Combine(TypeId, ReflectedTypeId);

    public abstract string ReflectedTypeName { get; }

    public abstract int ReflectedTypeId { get; }

    protected DyTypeInfo() : base(Dy.TypeInfo) => AddMixin(Dy.Object);

    #region Binary Operations
    //x + y
    private DyFunction? add;
    protected virtual DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        //TODO: validate logic
        if (right.TypeId == Dy.String && left.TypeId != Dy.String)
        {
            try
            {
                return left.Concat(right, ctx);
            }
            catch (DyCodeException ex)
            {
                ctx.Error = ex.Error;
                return Nil;
            }
        }

        return ctx.OperationNotSupported(Builtins.Add, left, right);
    }
    public DyObject Add(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (add is not null)
            return add.PrepareFunction(ctx, left);
        return AddOp(ctx, left, right);
    }

    //x - y
    private DyFunction? sub;
    protected virtual DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Sub, left, right);
    public DyObject Sub(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (sub is not null)
            return sub.PrepareFunction(ctx, left);
        return SubOp(ctx, left, right);
    }

    //x * y
    private DyFunction? mul;
    protected virtual DyObject MulOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Mul, left, right);
    public DyObject Mul(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (mul is not null)
            return mul.PrepareFunction(ctx, left);
        return MulOp(ctx, left, right);
    }

    //x / y
    private DyFunction? div;
    protected virtual DyObject DivOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Div, left, right);
    public DyObject Div(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (div is not null)
            return div.PrepareFunction(ctx, left);
        return DivOp(ctx, left, right);
    }

    //x % y
    private DyFunction? rem;
    protected virtual DyObject RemOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Rem, left, right);
    public DyObject Rem(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (rem is not null)
            return rem.PrepareFunction(ctx, left);
        return RemOp(ctx, left, right);
    }

    //x <<< y
    private DyFunction? shl;
    protected virtual DyObject ShiftLeftOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Shl, left, right);
    public DyObject ShiftLeft(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shl is not null)
            return shl.PrepareFunction(ctx, left);
        return ShiftLeftOp(ctx, left, right);
    }

    //x >>> y
    private DyFunction? shr;
    protected virtual DyObject ShiftRightOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Shr, left, right);
    public DyObject ShiftRight(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (shr is not null)
            return shr.PrepareFunction(ctx, left);
        return ShiftRightOp(ctx, left, right);
    }

    //x &&& y
    private DyFunction? and;
    protected virtual DyObject AndOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.And, left, right);
    public DyObject And(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (and is not null)
            return and.PrepareFunction(ctx, left);
        return AndOp(ctx, left, right);
    }

    //x ||| y
    private DyFunction? or;
    protected virtual DyObject OrOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Or, left, right);
    public DyObject Or(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (or is not null)
            return or.PrepareFunction(ctx, left);
        return OrOp(ctx, left, right);
    }

    //x ^^^ y
    private DyFunction? xor;
    protected virtual DyObject XorOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Xor, left, right);
    public DyObject Xor(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (xor is not null)
            return xor.PrepareFunction(ctx, left);
        return XorOp(ctx, left, right);
    }

    //x == y
    private DyFunction? eq;
    protected virtual DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ReferenceEquals(left, right) ? True : False;
    public DyObject Eq(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (eq is not null)
            return eq.PrepareFunction(ctx, left);
        if (right.TypeId == Dy.Bool)
            return ReferenceEquals(left, right) ? True : False;
        return EqOp(ctx, left, right);
    }

    //x != y
    private DyFunction? neq;
    protected virtual DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        EqOp(ctx, left, right).IsFalse() ? True : False;
    public DyObject Neq(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (neq is not null)
            return neq.PrepareFunction(ctx, left);
        return NeqOp(ctx, left, right);
    }

    //x > y
    private DyFunction? gt;
    protected virtual DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Gt, left, right);
    public DyObject Gt(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (gt is not null)
            return gt.PrepareFunction(ctx, left);
        return GtOp(ctx, left, right);
    }

    //x < y
    private DyFunction? lt;
    protected virtual DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        ctx.OperationNotSupported(Builtins.Lt, left, right);
    public DyObject Lt(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (lt is not null)
            return lt.PrepareFunction(ctx, left);
        return LtOp(ctx, left, right);
    }

    //x >= y
    private DyFunction? gte;
    protected virtual DyObject GteOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        left.Greater(right, ctx) || left.Equals(right, ctx) ? True : False;
    public DyObject Gte(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (gte is not null)
            return gte.PrepareFunction(ctx, left);
        return GteOp(ctx, left, right);
    }

    //x <= y
    private DyFunction? lte;
    protected virtual DyObject LteOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        left.Lesser(right, ctx) || left.Equals(right, ctx) ? True : False;
    public DyObject Lte(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (lte is not null)
            return lte.PrepareFunction(ctx, left);
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
            return neg.PrepareFunction(ctx, arg);
        return NegOp(ctx, arg);
    }

    //+x
    private DyFunction? plus;
    protected virtual DyObject PlusOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.Plus, arg);
    public DyObject Plus(ExecutionContext ctx, DyObject arg)
    {
        if (plus is not null)
            return plus.PrepareFunction(ctx, arg);
        return PlusOp(ctx, arg);
    }

    //!x
    private DyFunction? not;
    protected virtual DyObject NotOp(ExecutionContext ctx, DyObject arg) =>
        arg.IsFalse() ? True : False;
    public DyObject Not(ExecutionContext ctx, DyObject arg)
    {
        if (not is not null)
            return not.PrepareFunction(ctx, arg);
        return NotOp(ctx, arg);
    }

    //~x
    private DyFunction? bitnot;
    protected virtual DyObject BitwiseNotOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.BitNot, arg);
    public DyObject BitwiseNot(ExecutionContext ctx, DyObject arg)
    {
        if (bitnot is not null)
            return bitnot.PrepareFunction(ctx, arg);
        return BitwiseNotOp(ctx, arg);
    }

    //x.Length
    private DyFunction? len;
    protected virtual DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        ctx.OperationNotSupported(Builtins.Length, arg);
    public DyObject Length(ExecutionContext ctx, DyObject arg)
    {
        if (len is not null)
            return len.PrepareFunction(ctx, arg);
        return LengthOp(ctx, arg);
    }

    //x.ToString
    private DyFunction? tos;
    protected virtual DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => new DyString(arg.ToString());
    public DyObject ToString(ExecutionContext ctx, DyObject arg)
    {
        if (tos is not null)
            return tos.PrepareFunction(ctx, arg);

        //Validate logic
        try
        {
            return ToStringOp(ctx, arg, Nil);
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    //x.ToLiteral
    private DyFunction? lit;
    [Obsolete]
    protected virtual DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => ToStringOp(ctx, arg, DyNil.Instance);
    [Obsolete]
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
    [Obsolete]
    public DyObject ToLiteral(ExecutionContext ctx, DyObject arg)
    {
        if (lit is not null)
            return lit.PrepareFunction(ctx, arg);
        return ToLiteralOp(ctx, arg);
    }

    //Clone
    private DyObject Clone(ExecutionContext ctx, DyObject obj) => obj.Clone();

    //Iterate
    private DyObject GetIterator(ExecutionContext ctx, DyObject self) =>
        self is IEnumerable<DyObject> en ? DyIterator.Create(en) : ctx.OperationNotSupported(Builtins.Iterator, self);
    #endregion

    #region Other Operations
    //x[y]
    private DyFunction? get;
    protected virtual DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) => self.GetItem(index, ctx);
    [Obsolete]
    internal DyObject GetDirect(ExecutionContext ctx, DyObject self, DyObject index) => GetOp(ctx, self, index);
    public DyObject Get(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (get is not null)
            return get.PrepareFunction(ctx, self);

        return GetOp(ctx, self, index);
    }

    //x[y] = z
    private DyFunction? set;
    protected virtual DyObject SetOp(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) =>
        ctx.OperationNotSupported(Builtins.Set, self);
    [Obsolete]
    internal DyObject SetDirect(ExecutionContext ctx, DyObject self, DyObject index, DyObject value) => SetOp(ctx, self, index, value);
    public DyObject Set(ExecutionContext ctx, DyObject self, DyObject index, DyObject value)
    {
        if (set is not null)
            return set.PrepareFunction(ctx, self);

        return SetOp(ctx, self, index, value);
    }

    //as
    private readonly Dictionary<int, DyFunction> conversions = new();
    protected virtual DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            _ when targetType.ReflectedTypeId == self.TypeId => self,
            Dy.Bool => self.IsFalse() ? False : True,
            Dy.String => self.ToString(ctx),
            Dy.Char => new DyChar(self.ToString(ctx).GetString()[0]),
            _ => ctx.InvalidCast(self.TypeName, targetType.ReflectedTypeName)
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
            return contains.PrepareFunction(ctx, self);

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

    internal bool TryGetStaticMember(ExecutionContext ctx, HashString name, out DyObject? value)
    {
        var func = LookupStaticMember(name, ctx);

        if (func is not null)
        {
            if (func is DyFunction f && f.Auto)
                value = f.BindOrRun(ctx, this);
            else
                value = func;
            
            return true;
        }

        value = null;
        return false;
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

    internal bool TryGetInstanceMember(ExecutionContext ctx, DyObject self, HashString name, out DyObject? value)
    {
        var func = LookupInstanceMember(self, name, ctx);

        if (func is not null)
        {
            value = func.BindOrRun(ctx, self);
            return true;
        }

        value = null;
        return false;
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

            if ((set == Builtins.Length & (GetSupportedOperations() & SupportedOperations.Len) == SupportedOperations.Len)
                || set == Builtins.String || (Members.TryGetValue(set, out var get) && !get.Auto))
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
            case Builtins.Length:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                len = func;
                break;
            case Builtins.String:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                tos = func; 
                break;
            case Builtins.ToLiteral:
                if (func is not null && func.Auto)
                    ctx.InvalidOverload(name);
                lit = func;
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
            Builtins.Add => Support(self, SupportedOperations.Add) ? (add is null ? Binary(name, AddOp) : add) : null,
            Builtins.Sub => Support(self, SupportedOperations.Sub) ? (sub is null ? Binary(name, SubOp) : sub) : null,
            Builtins.Mul => Support(self, SupportedOperations.Mul) ? (mul is null ? Binary(name, MulOp) : mul) : null,
            Builtins.Div => Support(self, SupportedOperations.Div) ? (div is null ? Binary(name, DivOp) : div) : null,
            Builtins.Rem => Support(self, SupportedOperations.Rem) ? (rem is null ? Binary(name, RemOp) : rem) : null,
            Builtins.Shl => Support(self, SupportedOperations.Shl) ? (shl is null ? Binary(name, ShiftLeftOp) : shl) : null,
            Builtins.Shr => Support(self, SupportedOperations.Shr) ? (shr is null ? Binary(name, ShiftRightOp) : shr) : null,
            Builtins.And => Support(self, SupportedOperations.And) ? (and is null ? Binary(name, AndOp) : and) : null,
            Builtins.Or => Support(self, SupportedOperations.Or) ? (or is null ? Binary(name, OrOp) : or) : null,
            Builtins.Xor => Support(self, SupportedOperations.Xor) ? (xor is null ? Binary(name, XorOp) : xor) : null,
            Builtins.Eq => eq is null ? Binary(name, EqOp) : eq,
            Builtins.Neq => neq is null ? Binary(name, NeqOp) : neq,
            Builtins.Gt => Support(self, SupportedOperations.Gt) ? (gt is null ? Binary(name, GtOp) : gt) : null,
            Builtins.Lt => Support(self, SupportedOperations.Lt) ? (lt is null ? Binary(name, LtOp) : lt) : null,
            Builtins.Gte => Support(self, SupportedOperations.Gte) ? (gte is null ? Binary(name, GteOp) : gte) : null,
            Builtins.Lte => Support(self, SupportedOperations.Lte) ? (lte is null ? Binary(name, LteOp) : lte) : null,
            Builtins.Neg => Support(self, SupportedOperations.Neg) ? (neg is null ? Unary(name, NegOp) : neg) : null,
            Builtins.Not => not is null ? Unary(name, NotOp) : not,
            Builtins.BitNot => Support(self, SupportedOperations.BitNot) ? (bitnot is null ? Unary(name, BitwiseNotOp) : bitnot) : null,
            Builtins.Plus => Support(self, SupportedOperations.Plus) ? (plus is null ? Unary(name, PlusOp) : plus) : null,
            Builtins.Get => Support(self, SupportedOperations.Get) ? (get is null ? Binary(name, GetOp, "index") : get) : null,
            Builtins.Set => Support(self, SupportedOperations.Set) ? (set is null ? Ternary(name, SetOp, "index", "value") : set) : null,
            Builtins.Length => Support(self, SupportedOperations.Len) ? (len is null ? Unary(name, LengthOp) : len) : null,
            Builtins.String => tos is null ? Binary(name, ToStringOp, new Par("format", Nil)) : tos,
            Builtins.ToLiteral => lit is null ? Unary(name, ToLiteralOp) : lit,
            Builtins.Iterator => Support(self, SupportedOperations.Iter) ? Unary(name, GetIterator) : null,
            Builtins.Clone => Unary(name, Clone),
            Builtins.Has => Binary(name, Has, "member"),
            Builtins.Type => Unary(name, (ct, o) => ct.RuntimeContext.Types[o.TypeId]),
            Builtins.Contains => Support(self, SupportedOperations.In) ? (contains is null ? Binary(name, ContainsOp, "value") : contains) : null,
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
