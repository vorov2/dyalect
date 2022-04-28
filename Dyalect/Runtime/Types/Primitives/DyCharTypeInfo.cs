using Dyalect.Codegen;
using Dyalect.Parser;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyCharTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Add | SupportedOperations.Sub
        | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(Dy.Char);

    public override int ReflectedTypeId => Dy.Char;

    public DyCharTypeInfo() => AddMixin(Dy.Comparable);

    #region Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyChar((char)(left.GetChar() + right.GetInteger()));

        if (right.TypeId == Dy.Char)
            return new DyString(left.GetString() + right.GetString());

        if (right.TypeId == Dy.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);

        return base.AddOp(ctx, left, right);
    }

    protected override DyObject SubOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId == Dy.Integer)
            return new DyChar((char)(left.GetChar() - right.GetInteger()));

        if (right.TypeId == Dy.Char)
            return DyInteger.Get(left.GetChar() - right.GetChar());

        return base.SubOp(ctx, left, right);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar() == right.GetChar() ? True : False;

        if (right.TypeId == Dy.String)
        {
            var str = right.GetString();
            return str.Length == 1 && left.GetChar() == str[0] ? True : False;
        }

        return base.EqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar() != right.GetChar() ? True : False;

        if (right.TypeId == Dy.String)
        {
            var str = right.GetString();
            return str.Length != 1 || left.GetChar() != str[0] ? True : False;
        }

        return base.NeqOp(ctx, left, right); //Important! Should redirect to base
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar().CompareTo(right.GetChar()) > 0 ? True : False;

        if (right.TypeId == Dy.String)
            return left.GetString().CompareTo(right.GetString()) > 0 ? True : False;

        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar().CompareTo(right.GetChar()) < 0 ? True : False;

        if (right.TypeId == Dy.String)
            return left.GetString().CompareTo(right.GetString()) < 0 ? True : False;

        return base.LtOp(ctx, left, right);
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => DyInteger.Get(self.GetChar()),
            Dy.Float => new DyFloat(self.GetChar()),
            _ => base.CastOp(ctx, self, targetType)
        };

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(arg.GetString());

    protected override DyObject ToLiteralOp(ExecutionContext ctx, DyObject arg) => new DyString(StringUtil.Escape(arg.GetString(), "'"));
    #endregion

    [InstanceMethod] internal static bool IsLower(char self) => char.IsLower(self);
    
    [InstanceMethod] internal static bool IsUpper(char self) => char.IsUpper(self);
    
    [InstanceMethod] internal static bool IsControl(char self) => char.IsControl(self);
    
    [InstanceMethod] internal static bool IsDigit(char self) => char.IsDigit(self);
    
    [InstanceMethod] internal static bool IsLetter(char self) => char.IsLetter(self);
    
    [InstanceMethod] internal static bool IsLetterOrDigit(char self) => char.IsLetterOrDigit(self);
    
    [InstanceMethod] internal static bool IsWhiteSpace(char self) => char.IsWhiteSpace(self);
    
    [InstanceMethod] internal static char Lower(char self) => char.ToLower(self);
    
    [InstanceMethod] internal static char Upper(char self) => char.ToUpper(self);

    [InstanceMethod] internal static int Order(char self) => self;

    [StaticMethod(Method.Char)]
    internal static DyObject CreateChar(ExecutionContext ctx, DyObject value)
    {
        if (value.TypeId == Dy.Char)
            return value;

        if (value.TypeId == Dy.String)
        {
            var str = value.ToString();
            return str is not null && str.Length > 0 ? new(str[0]) : DyChar.Empty;
        }

        if (value.TypeId == Dy.Integer)
            return new DyChar((char)value.GetInteger());

        if (value.TypeId == Dy.Float)
            return new DyChar((char)value.GetFloat());

        return ctx.InvalidCast(value.GetTypeInfo(ctx).ReflectedTypeName, nameof(Dy.Char));
    }

    [StaticProperty]
    internal static DyChar Max() => DyChar.Max;

    [StaticProperty]
    internal static DyChar Min() => DyChar.Min;

    [StaticProperty]
    internal static DyChar Default() => DyChar.Empty;
}
