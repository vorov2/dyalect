using Dyalect.Codegen;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyStringTypeInfo : DyCollTypeInfo
{
    readonly struct FormatData : IFormattable
    {
        public readonly DyObject Object;
        public readonly ExecutionContext Context;

        public FormatData(DyObject obj, ExecutionContext ctx) =>
            (Object, Context) = (obj, ctx);

        public string ToString(string? format, IFormatProvider? formatProvider) =>
            Object.ToString(Context, DyString.Get(format)).ToString();
    }

    public override string ReflectedTypeName => nameof(Dy.String);

    public override int ReflectedTypeId => Dy.String;

    public DyStringTypeInfo()
    {
        AddMixins(Dy.Lookup, Dy.Order, Dy.Equatable, Dy.Sequence, Dy.Show);
        SetSupportedOperations(Ops.Add);
    }

    #region Operations
    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        try
        {
            var other = right.TypeId == Dy.String || right.TypeId == Dy.Char ? right.ToString() : right.ToString(ctx).Value;
            return new DyString(((DyString)left).Value + other);
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId || right.TypeId == Dy.Char)
            return ((DyString)left).Value == right.ToString() ? True : False;
        return base.EqOp(ctx, left, right);
    }

    protected override DyObject NeqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId || right.TypeId == Dy.Char)
            return ((DyString)left).Value != right.ToString() ? True : False;
        return base.NeqOp(ctx, left, right);
    }

    protected override DyObject GtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId || right.TypeId == Dy.Char)
            return ((DyString)left).Value.CompareTo(right.ToString()) > 0 ? True : False;
        return base.GtOp(ctx, left, right);
    }

    protected override DyObject LtOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (left.TypeId == right.TypeId || right.TypeId == Dy.Char)
            return ((DyString)left).Value.CompareTo(right.ToString()) < 0 ? True : False;
        return base.LtOp(ctx, left, right);
    }

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var len = ((DyString)arg).Value.Length;
        return DyInteger.Get(len);
    }

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) => arg;

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index)
    {
        if (index is not DyInteger i)
            return ctx.IndexOutOfRange(index);

        var str = (DyString)self;
        var ix = (int)(i.Value < 0 ? str.Count + i.Value : i.Value);

        if (ix < 0 || ix >= str.Count)
            return ctx.IndexOutOfRange(index);

        return new DyChar(str.Value[ix]);
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            Dy.Integer => long.TryParse(self.ToString(), out var i8) ? new DyInteger(i8) : DyInteger.Zero,
            Dy.Float => double.TryParse(self.ToString(), out var r8) ? new DyFloat(r8) : DyFloat.Zero,
            _ => base.CastOp(ctx, self, targetType)
        };
    #endregion

    private static int CorrectIndex(int index, string str) => index < 0 ? index + str.Length : index;

    [InstanceMethod]
    internal static bool Contains(string self, string field) => self.Contains(field);

    [InstanceMethod]
    internal static DyObject Slice(DyString self, int index = 0, int? size = null)
    {
        index = CorrectIndex(index, self.Value);
        size ??= self.Count - 1;

        if (index == 0 && size == self.Count - 1)
            return self;

        if (index >= self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange, index);

        if (size < 0)
            size = self.Count + size - 1;

        if (size >= self.Count)
            throw new DyCodeException(DyError.IndexOutOfRange, size);

        var len = size.Value - index + 1;

        if (len < 0)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (len == 0)
            return DyString.Empty;

        return new DyString(self.Value.Substring(index, len));
    }

    [InstanceMethod]
    internal static int IndexOf(string self, string value, int index = 0, int? count = null)
    {
        index = CorrectIndex(index, self);
        count ??= self.Length - index;

        if (index < 0 || index > self.Length || count < 0 || count > self.Length - index)
            throw new DyCodeException(DyError.IndexOutOfRange);

        return self.IndexOf(value, index, count.Value);
    }

    [InstanceMethod]
    internal static int LastIndexOf(string self, string value, int? index = null, int? count = null)
    {
        index ??= self.Length - 1;
        index = CorrectIndex(index.Value, self);
        count ??= index + 1;

        if (index < 0 || index > self.Length || count < 0 || index - count + 1 < 0)
            throw new DyCodeException(DyError.IndexOutOfRange);

        return self.LastIndexOf(value, index.Value, count.Value);
    }

    [InstanceMethod]
    internal static string[] Split(string self, params string[] separators) =>
        self.Split(separators, StringSplitOptions.RemoveEmptyEntries);

    [InstanceMethod]
    internal static string Capitalize(string self) =>
        self.Length == 0 ? "" : char.ToUpper(self[0]) + self[1..].ToLower();

    [InstanceMethod]
    internal static string Upper(string self) => self.ToUpper();

    [InstanceMethod]
    internal static string Lower(string self) => self.ToLower();

    [InstanceMethod]
    internal static bool StartsWith(string self, string value) => self.StartsWith(value);

    [InstanceMethod]
    internal static bool EndsWith(string self, string value) => self.EndsWith(value);

    [InstanceMethod]
    internal static string? Substring(string self, int index, int? count = null)
    {
        index = CorrectIndex(index, self);

        if (index >= self.Length)
            throw new DyCodeException(DyError.IndexOutOfRange);

        if (count is null)
            return self[index..];

        if (count < 0 || count + index > self.Length)
            throw new DyCodeException(DyError.IndexOutOfRange);

        return self.Substring(index, count.Value);
    }

    [InstanceMethod]
    internal static string Trim(string self, params char[] chars) => self.Trim(chars);

    [InstanceMethod]
    internal static string TrimStart(string self, params char[] chars) => self.TrimStart(chars);

    [InstanceMethod]
    internal static string TrimEnd(string self, params char[] chars) => self.TrimEnd(chars);

    [InstanceMethod]
    internal static bool IsEmpty(string self) => string.IsNullOrWhiteSpace(self);

    [InstanceMethod]
    internal static string PadLeft(string self, int width, [ParameterName("char")] char c = ' ') =>
        self.PadLeft(width, c);

    [InstanceMethod]
    internal static string PadRight(string self, int width, [ParameterName("char")] char c = ' ') =>
        self.PadRight(width, c);

    [InstanceMethod]
    internal static string Replace(string self, string value, string other, bool ignoreCase = false) =>
        self.Replace(value, other, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    [InstanceMethod]
    internal static string? Remove(string self, int index, int? count = null)
    {
        count ??= self.Length - index;

        if (index + count > self.Length)
            throw new DyCodeException(DyError.IndexOutOfRange);

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
            arr[i] = new FormatData(values[i], ctx);

        return string.Format(self, arr);
    }

    [StaticMethod]
    internal static string? Concat(ExecutionContext ctx, params DyObject[] values)
    {
        var xs = new List<string>();
        Collect(ctx, values, xs);
        return string.Concat(xs);
    }

    [StaticMethod(Method.String)]
    internal static string? New(ExecutionContext ctx, params DyObject[] values) => Concat(ctx, values);

    private static void Collect(ExecutionContext ctx, DyObject[] values, List<string> xs)
    {
        for (var i = 0; i < values.Length; i++)
        {
            var a = values[i];

            if (a.TypeId is Dy.String or Dy.Char)
                xs.Add(a.ToString());
            else
            {
                var res = a.ToString(ctx);
                xs.Add(res.Value);
            }
        }
    }

    [StaticMethod]
    internal static string? Join(ExecutionContext ctx, [VarArg]DyObject[] values, string separator = ",")
    {
        var xs = new List<string>();
        Collect(ctx, values, xs);
        return string.Join(separator, xs);
    }

    [StaticProperty]
    internal static DyObject Empty() => DyString.Empty;

    [StaticProperty]
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
}
