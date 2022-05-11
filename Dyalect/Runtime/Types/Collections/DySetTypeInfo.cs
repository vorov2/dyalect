using Dyalect.Codegen;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DySetTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Set);

    public override int ReflectedTypeId => Dy.Set;

    public DySetTypeInfo()
    {
        AddMixins(Dy.Lookup, Dy.Sequence, Dy.Sequence);
    }

    #region Operations
    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var self = (DySet)left;
        return self.Equals(ctx, right) ? True : False;
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var self = (DySet)arg;
        return DyInteger.Get(self.Count);
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format)
    {
        try
        {
            return new DyString("Set(" + ((IEnumerable<DyObject>)arg).ToLiteral(ctx) + ")");
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Array => new DyArray(((DySet)self).ToArray()),
            Dy.Tuple => new DyTuple(((DySet)self).ToArray()),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion


    [InstanceMethod]
    internal static bool Contains(DySet self, DyObject field) => self.Contains(field);

    [InstanceMethod(Method.Add)]
    internal static bool AddItem(DySet self, DyObject value) => self.Add(value);

    [InstanceMethod]
    internal static bool Remove(DySet self, DyObject value) => self.Remove(value);

    [InstanceMethod]
    internal static void Clear(DySet self) => self.Clear();

    [InstanceMethod]
    internal static DyObject ToArray(ExecutionContext ctx, DySet self) => self.ToArray(ctx);

    [InstanceMethod]
    internal static DyObject ToTuple(ExecutionContext ctx, DySet self) => self.ToTuple(ctx);

    [InstanceMethod]
    internal static void IntersectWith(ExecutionContext ctx, DySet self, DyObject other) =>
        self.IntersectWith(ctx, other);

    [InstanceMethod]
    internal static void UnionWith(ExecutionContext ctx, DySet self, DyObject other) =>
        self.UnionWith(ctx, other);

    [InstanceMethod]
    internal static void ExceptOf(ExecutionContext ctx, DySet self, DyObject other) =>
        self.ExceptWith(ctx, other);

    [InstanceMethod]
    internal static bool OverlapsWith(ExecutionContext ctx, DySet self, DyObject other) =>
        self.Overlaps(ctx, other);

    [InstanceMethod]
    internal static bool IsSubsetOf(ExecutionContext ctx, DySet self, DyObject other) =>
        self.IsSubsetOf(ctx, other);

    [InstanceMethod]
    internal static bool IsSupersetOf(ExecutionContext ctx, DySet self, DyObject other) =>
        self.IsSupersetOf(ctx, other);

    [StaticMethod(Method.Set)]
    internal static DyObject New([VarArg]DyObject values) => new DySet(((DyTuple)values).ToArray());
}
