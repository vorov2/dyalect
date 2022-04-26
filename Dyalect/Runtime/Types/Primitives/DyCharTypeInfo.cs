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

    public override string ReflectedTypeName => nameof(DyType.Char);

    public override int ReflectedTypeId => DyType.Char;

    public DyCharTypeInfo() => AddMixin(DyType.Comparable, DyType.Bounded);

    #region Operations
    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == DyType.Integer)
            return new DyChar((char)(left.GetChar() + right.GetInteger()));

        if (right.TypeId == DyType.Char)
            return new DyString(left.GetString() + right.GetString());

        if (right.TypeId == DyType.String)
            return ctx.RuntimeContext.String.Add(ctx, left, right);

        return base.AddOp(left, right, ctx);
    }

    protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId == DyType.Integer)
            return new DyChar((char)(left.GetChar() - right.GetInteger()));

        if (right.TypeId == DyType.Char)
            return DyInteger.Get(left.GetChar() - right.GetChar());

        return base.SubOp(left, right, ctx);
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar() == right.GetChar() ? DyBool.True : DyBool.False;

        if (right.TypeId == DyType.String)
        {
            var str = right.GetString();
            return str.Length == 1 && left.GetChar() == str[0] ? DyBool.True : DyBool.False;
        }

        return base.EqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar() != right.GetChar() ? DyBool.True : DyBool.False;

        if (right.TypeId == DyType.String)
        {
            var str = right.GetString();
            return str.Length != 1 || left.GetChar() != str[0] ? DyBool.True : DyBool.False;
        }

        return base.NeqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar().CompareTo(right.GetChar()) > 0 ? DyBool.True : DyBool.False;

        if (right.TypeId == DyType.String)
            return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;

        return base.GtOp(left, right, ctx);
    }

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId)
            return left.GetChar().CompareTo(right.GetChar()) < 0 ? DyBool.True : DyBool.False;

        if (right.TypeId == DyType.String)
            return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;

        return base.LtOp(left, right, ctx);
    }

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Integer => DyInteger.Get(self.GetChar()),
            DyType.Float => new DyFloat(self.GetChar()),
            _ => base.CastOp(self, targetType, ctx)
        };

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(arg.GetString());

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => new DyString(StringUtil.Escape(arg.GetString(), "'"));
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
        if (value.TypeId == DyType.Char)
            return value;

        if (value.TypeId == DyType.String)
        {
            var str = value.ToString();
            return str is not null && str.Length > 0 ? new(str[0]) : DyChar.Empty;
        }

        if (value.TypeId == DyType.Integer)
            return new DyChar((char)value.GetInteger());

        if (value.TypeId == DyType.Float)
            return new DyChar((char)value.GetFloat());

        return ctx.InvalidCast(value.GetTypeInfo(ctx).ReflectedTypeName, nameof(DyType.Char));
    }

    [StaticMethod]
    internal static DyChar Max() => DyChar.Max;

    [StaticMethod]
    internal static DyChar Min() => DyChar.Min;

    [StaticMethod]
    internal static DyChar Default() => DyChar.Empty;
}
