using System.Linq;
using System.Text;
using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DySetTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => DyTypeNames.Set;

    public override int ReflectedTypeId => DyType.Set;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.Lit;

    public DySetTypeInfo() => AddMixin(DyType.Collection);

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        var self = (DySet)left;
        return self.Equals(ctx, right) ? DyBool.True : DyBool.False;
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var self = (DySet)arg;
        return DyInteger.Get(self.Count);
    }

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx)
    {
        var set = (DySet)self;
        return set.Contains(field) ? DyBool.True : DyBool.False;
    }

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => ToLiteralOrString(arg, ctx, literal: false);

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => ToLiteralOrString(arg, ctx, literal: true);

    private DyObject ToLiteralOrString(DyObject arg, ExecutionContext ctx, bool literal)
    {
        var self = (DySet)arg;
        var sb = new StringBuilder("Set (");
        var c = 0;

        foreach (var v in self)
        {
            if (c++ > 0)
                sb.Append(", ");

            sb.Append(literal ? v.ToLiteral(ctx) : v.ToString(ctx));

            if (ctx.HasErrors)
                return DyNil.Instance;
        }

        sb.Append(')');
        return new DyString(sb.ToString());
    }

    private DyObject AddItem(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        return set.Add(value) ? DyBool.True : DyBool.False;
    }
    
    private DyObject Remove(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        return set.Remove(value) ? DyBool.True : DyBool.False;
    }
    
    private DyObject Clear(ExecutionContext ctx, DyObject self)
    {
        var set = (DySet)self;
        set.Clear();
        return DyNil.Instance;
    }
    
    private DyObject ToArray(ExecutionContext ctx, DyObject self)
    {
        var set = (DySet)self;
        return set.ToArray(ctx);
    }
    
    private DyObject ToTuple(ExecutionContext ctx, DyObject self)
    {
        var set = (DySet)self;
        return set.ToTuple(ctx);
    }
    
    private DyObject IntersectWith(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        set.IntersectWith(ctx, value);
        return DyNil.Instance;
    }
    
    private DyObject UnionWith(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        set.UnionWith(ctx, value);
        return DyNil.Instance;
    }
    
    private DyObject ExceptOf(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        set.ExceptWith(ctx, value);
        return DyNil.Instance;
    }
    
    private DyObject Overlaps(ExecutionContext ctx, DyObject self, DyObject value)
    {
        var set = (DySet)self;
        return set.Overlaps(ctx, value) ? DyBool.True : DyBool.False;
    }

    private DyObject IsSubsetOf(ExecutionContext ctx, DyObject self, DyObject other)
    {
        var seq = (DySet)self;
        return seq.IsSubsetOf(ctx, other) ? DyBool.True : DyBool.False;
    }

    private DyObject IsSupersetOf(ExecutionContext ctx, DyObject self, DyObject other)
    {
        var seq = (DySet)self;
        return seq.IsSupersetOf(ctx, other) ? DyBool.True : DyBool.False;
    }
    
    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Add => Func.Member(name, AddItem, -1, new Par("value")),
            Method.Remove => Func.Member(name, Remove, -1, new Par("value")),
            Method.Clear => Func.Member(name, Clear),
            Method.ToArray => Func.Member(name, ToArray),
            Method.ToTuple => Func.Member(name, ToTuple),
            Method.ExceptOf => Func.Member(name, ExceptOf, -1, new Par("other")),
            Method.IntersectWith => Func.Member(name, IntersectWith, -1, new Par("other")),
            Method.UnionWith => Func.Member(name, UnionWith, -1, new Par("other")),
            Method.OverlapsWith => Func.Member(name, Overlaps, -1, new Par("other")),
            Method.IsSubsetOf => Func.Member(name, IsSubsetOf, -1, new Par("other")),
            Method.IsSupersetOf => Func.Member(name, IsSupersetOf, -1, new Par("other")),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    private DyObject New(ExecutionContext ctx, DyObject arg) => new DySet(((DyTuple)arg).GetValues());

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Set => Func.Static(name, New, 0, new Par("values", DyNil.Instance)),
            _ => base.InitializeStaticMember(name, ctx)
        };

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Array => new DyArray(((DySet)self).ToArray()),
            DyType.Tuple => new DyTuple(((DySet)self).ToArray()),
            _ => base.CastOp(self, targetType, ctx)
        };
}
