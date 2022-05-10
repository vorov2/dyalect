using Dyalect.Codegen;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyCharTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Char);

    public override int ReflectedTypeId => Dy.Char;

    public DyCharTypeInfo()
    {
        AddMixins(Dy.Order);
        SetSupportedOperations(Ops.Add | Ops.Sub | Ops.Gt | Ops.Lt | Ops.Gte | Ops.Lte);
    }

    #region Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i)
            return new DyChar((char)(((DyChar)left).Value + i.Value));

        if (right.TypeId is Dy.Char)
            return new DyString(left.ToString() + right.ToString());

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyInteger i)
            return new DyChar((char)(((DyChar)left).Value - i.Value));

        if (right is DyChar c)
            return new DyChar((char)(((DyChar)left).Value - c.Value));
        
        return base.SubOp(ctx, left, right);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return ((DyChar)left).Value == ((DyChar)right).Value ? True : False;

        if (right is DyString str)
            return str.Value.Length == 1 && ((DyChar)left).Value == str.Value[0] ? True : False;
        
        return base.EqOp(ctx, left, right);
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return ((DyChar)left).Value != ((DyChar)right).Value ? True : False;

        if (right is DyString str)
            return str.Value.Length != 1 || ((DyChar)left).Value != str.Value[0] ? True : False;

        return base.NeqOp(ctx, left, right);
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return ((DyChar)left).Value.CompareTo(((DyChar)right).Value) > 0 ? True : False;

        if (right is DyString str)
            return ((DyChar)left).Value.ToString().CompareTo(str.Value) > 0 ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return ((DyChar)left).Value.CompareTo(((DyChar)right).Value) < 0 ? True : False;

        if (right is DyString str)
            return ((DyChar)left).Value.ToString().CompareTo(str.Value) < 0 ? True : False;

        return base.LtOp(ctx, left, right);
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => DyInteger.Get(((DyChar)self).Value),
            Dy.Float => new DyFloat(((DyChar)self).Value),
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    [InstanceMethod]
    internal static bool IsLower(char self) => char.IsLower(self);

    [InstanceMethod]
    internal static bool IsUpper(char self) => char.IsUpper(self);

    [InstanceMethod]
    internal static bool IsControl(char self) => char.IsControl(self);

    [InstanceMethod]
    internal static bool IsDigit(char self) => char.IsDigit(self);

    [InstanceMethod]
    internal static bool IsLetter(char self) => char.IsLetter(self);

    [InstanceMethod]
    internal static bool IsLetterOrDigit(char self) => char.IsLetterOrDigit(self);

    [InstanceMethod]
    internal static bool IsWhiteSpace(char self) => char.IsWhiteSpace(self);

    [InstanceMethod]
    internal static char Lower(char self) => char.ToLower(self);

    [InstanceMethod]
    internal static char Upper(char self) => char.ToUpper(self);

    [InstanceMethod]
    internal static int Order(char self) => self;

    [StaticMethod(Method.Char)]
    internal static DyObject CreateChar(DyObject value)
    {
        if (value.TypeId is Dy.Char)
            return value;

        if (value is DyString str)
            return str.Value.Length > 0 ? new(str.Value[0]) : DyChar.Empty;

        if (value is DyInteger i)
            return new DyChar((char)i.Value);

        if (value is DyFloat f)
            return new DyChar((char)f.Value);

        throw new DyCodeException(DyError.InvalidCast, value.TypeName, nameof(Dy.Char));
    }

    [StaticProperty]
    internal static DyChar Max() => DyChar.Max;

    [StaticProperty]
    internal static DyChar Min() => DyChar.Min;

    [StaticProperty]
    internal static DyChar Default() => DyChar.Empty;
}
