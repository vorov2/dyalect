using Dyalect.Codegen;
using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyStringTypeInfo : DyCollectionTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not | SupportedOperations.Add
        | SupportedOperations.Gt | SupportedOperations.Lt | SupportedOperations.Gte | SupportedOperations.Lte
        | SupportedOperations.Get | SupportedOperations.Len | SupportedOperations.Iter | SupportedOperations.Lit;

    public override string ReflectedTypeName => nameof(DyType.String);

    public override int ReflectedTypeId => DyType.String;

    public DyStringTypeInfo() => AddMixin(DyType.Collection, DyType.Comparable);

    #region Operations
    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        var str1 = left.TypeId == DyType.String || left.TypeId == DyType.Char ? left.GetString() : left.ToString(ctx).Value;

        if (ctx.HasErrors)
            return DyNil.Instance;

        var str2 = right.TypeId == DyType.String || right.TypeId == DyType.Char ? right.GetString() : right.ToString(ctx).Value;
        return new DyString(str1 + str2);
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString() == right.GetString() ? DyBool.True : DyBool.False;
        return base.EqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString() != right.GetString() ? DyBool.True : DyBool.False;
        return base.NeqOp(left, right, ctx); //Important! Should redirect to base
    }

    protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString().CompareTo(right.GetString()) > 0 ? DyBool.True : DyBool.False;
        return base.GtOp(left, right, ctx);
    }

    protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (left.TypeId == right.TypeId || right.TypeId == DyType.Char)
            return left.GetString().CompareTo(right.GetString()) < 0 ? DyBool.True : DyBool.False;
        return base.LtOp(left, right, ctx);
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var len = arg.GetString().Length;
        return DyInteger.Get(len);
    }

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx)
    {
        var str = self.GetString();

        if (field.TypeId == DyType.String)
            return str.Contains(field.GetString()) ? DyBool.True : DyBool.False;
        else if (field.TypeId == DyType.Char)
            return str.Contains(field.GetChar()) ? DyBool.True : DyBool.False;
        else
            return ctx.InvalidType(DyType.String, DyType.Char, field);
    }

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) => new DyString(arg.GetString());

    protected override DyObject ToLiteralOp(DyObject arg, ExecutionContext ctx) => new DyString(StringUtil.Escape(arg.GetString()));

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

    protected override DyObject SetOp(DyObject self, DyObject index, DyObject value, ExecutionContext ctx)
    {
        self.SetItem(index, value, ctx);
        return DyNil.Instance;
    }
    #endregion

    #region Instance
    [InstanceMethod]
    internal static DyObject Slice(ExecutionContext ctx, DyString self, int index = 0, [Default]int? size = null)
    {
        if (size is null)
            size = self.Count - 1;

        if (index == 0 && size == self.Count - 1)
            return self;

        if (index < 0)
            index = self.Count + index;

        if (index >= self.Count)
            return ctx.IndexOutOfRange(index);

        if (size < 0)
            size = self.Count + size - 1;

        if (size >= self.Count)
            return ctx.IndexOutOfRange(size);

        var len = size.Value - index + 1;

        if (len < 0)
            return ctx.IndexOutOfRange();

        if (len == 0)
            return DyString.Empty;

        return new DyString(self.Value.Substring(index, len));
    }

    [InstanceMethod]
    internal static int IndexOf(ExecutionContext ctx, string self, string value, int index = 0, [Default]int? count = null)
    {
        if (count is null)
            count = self.Length - index;

        if (index < 0 || index > self.Length || count < 0 || count > self.Length - index)
        {
            ctx.IndexOutOfRange();
            return default;
        }

        return self.IndexOf(value, index, count.Value);
    }

    [InstanceMethod]
    internal static int LastIndexOf(ExecutionContext ctx, string self, string value, [Default]int? index = null, [Default]int? count = null)
    {
        if (index is null)
            index = self.Length - 1;

        if (count is null)
            count = index + 1;

        if (index < 0 || index > self.Length || count < 0
            || index - count + 1 < 0)
        {
            ctx.IndexOutOfRange();
            return default;
        }

        return self.LastIndexOf(value, index.Value, count.Value);
    }

    [InstanceMethod]
    internal static string[] Split(string self, params string[] separators) =>
        self.Split(separators, StringSplitOptions.RemoveEmptyEntries);

    [InstanceMethod]
    internal static string Capitalize(string self) => self.Length == 0
        ? "" : char.ToUpper(self[0]) + self[1..].ToLower();

    [InstanceMethod]
    internal static string Upper(DyString self) => self.Value.ToUpper();

    [InstanceMethod]
    internal static string Lower(DyString self) => self.Value.ToLower();

    [InstanceMethod]
    internal static bool StartsWith(string self, string value) => self.StartsWith(value);

    [InstanceMethod]
    internal static bool EndsWith(string self, string value) => self.EndsWith(value);

    [InstanceMethod]
    internal static string? Substring(ExecutionContext ctx, string self, int index, [Default]int? count = null)
    {
        if (index < 0)
            index = self.Length + index;

        if (index >= self.Length)
        {
            ctx.IndexOutOfRange();
            return default;
        }

        if (count is null)
            return self[index..];

        if (count < 0 || count + index > self.Length)
        {
            ctx.IndexOutOfRange();
            return default;
        }

        return self.Substring(index, count.Value);
    }

    [InstanceMethod]
    internal static string Trim(string self, [VarArg]char[] chars) => self.Trim(chars);

    [InstanceMethod]
    internal static string TrimStart(string self, [VarArg]char[] chars) => self.TrimStart(chars);

    [InstanceMethod]
    internal static string TrimEnd(string self, [VarArg]char[] chars) => self.TrimEnd(chars);

    [InstanceMethod]
    internal static bool IsEmpty(string self) => string.IsNullOrWhiteSpace(self);

    [InstanceMethod]
    internal static string PadLeft(string self, int width, [ParameterName("char")]char c = ' ') =>
        self.PadLeft(width, c);

    [InstanceMethod]
    internal static string PadRight(string self, int width, [ParameterName("char")]char c = ' ') =>
        self.PadRight(width, c);

    [InstanceMethod]
    internal static string Replace(string self, string value, string other, bool ignoreCase = false) =>
        self.Replace(value, other, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    [InstanceMethod]
    internal static string? Remove(ExecutionContext ctx, string self, int index, [Default]int? count = null)
    {
        if (count is null)
            count = self.Length - index;

        if (index + count > self.Length)
        {
            ctx.IndexOutOfRange();
            return default;
        }

        return self.Remove(index, count.Value);
    }

    [InstanceMethod]
    internal static string Reverse(string self)
    {
        var sb = new StringBuilder(self.Length);

        for (var i = 0; i < self.Length; i++)
            sb.Append(self[self.Length - i - 1]);
        
        return sb.ToString();
    }

    [InstanceMethod]
    internal static DyObject ToCharArray(string self) =>
        new DyArray(self.ToCharArray().Select(c => new DyChar(c)).ToArray());

    [InstanceMethod]
    internal static string? Format(ExecutionContext ctx, string self, params DyObject[] values)
    {
        var arr = new object[values.Length];

        for (var i = 0; i < values.Length; i++)
        {
            var o = values[i].ToString(ctx);

            if (ctx.HasErrors)
                return default;

            arr[i] = o;
        }

        return self.Format(arr);
    }

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Integer => long.TryParse(self.GetString(), out var i8) ? new DyInteger(i8) : DyInteger.Zero,
            DyType.Float => double.TryParse(self.GetString(), out var r8) ? new DyFloat(r8) : DyFloat.Zero,
            _ => base.CastOp(self, targetType, ctx)
        };
    #endregion

    #region Statics
    [StaticMethod]
    internal static string? Concat(ExecutionContext ctx, params DyObject[] values)
    {
        var arr = new List<string>();

        if (!Collect(ctx, values, arr))
            return default;

        return string.Concat(arr);
    }

    [StaticMethod(Method.String)]
    internal static string? New(ExecutionContext ctx, params DyObject[] values) => Concat(ctx, values);

    private static bool Collect(ExecutionContext ctx, DyObject[] values, List<string> arr)
    {
        if (ctx.HasErrors)
            return false;

        for (var i = 0; i < values.Length; i++)
        {
            var a = values[i];

            if (a.TypeId == DyType.String || a.TypeId == DyType.Char)
                arr.Add(a.GetString());
            else
            {
                var res = DyString.ToString(a, ctx);

                if (ctx.HasErrors)
                    return false;

                arr.Add(res);
            }
        }

        return true;
    }

    [StaticMethod]
    internal static string? Join(ExecutionContext ctx, [VarArg]DyObject[] values, string separator = ",")
    {
        var strArr = new List<string>();

        if (!Collect(ctx, values, strArr))
            return default;

        return string.Join(separator, strArr);
    }

    [StaticMethod]
    internal static DyObject Empty() => DyString.Empty;

    [StaticMethod]
    internal static DyObject Default() => DyString.Empty;

    [StaticMethod]
    internal static string Repeat(string value, int count)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < count; i++)
            sb.Append(value);

        return sb.ToString();
    }

    [StaticMethod(Method.Format)]
    internal static string? StaticFormat(ExecutionContext ctx, string template, params DyObject[] values) => Format(ctx, template, values);
    #endregion
}
