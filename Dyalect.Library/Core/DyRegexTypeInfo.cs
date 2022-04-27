using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyRegexTypeInfo : DyForeignTypeInfo
{
    public override string ReflectedTypeName => "Regex";

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    #region Operations
    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
    {
        return new DyString(((DyRegex)arg).Regex.ToString());
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        return left is DyRegex a && right is DyRegex b && a.Regex.ToString() == b.Regex.ToString()
            ? DyBool.True : DyBool.False;
    }
    #endregion

    [InstanceMethod]
    internal static string? Replace(ExecutionContext ctx, DyRegex self, string input, string replacement)
    {
        try
        {
            return self.Regex.Replace(input, replacement);
        }
        catch (RegexMatchTimeoutException)
        {
            ctx.Timeout();
            return default;
        }
    }

    [InstanceMethod]
    internal static DyObject Split(ExecutionContext ctx, DyRegex self, string input, int? count = null, int index = 0)
    {
        if (count is null)
            count = int.MaxValue;

        if (index < 0 || index >= input.Length)
            return ctx.IndexOutOfRange(index);

        try
        {
            var arr = self.Regex.Split(input, count.Value, index);
            var objs = new List<DyObject>();

            for (var i = 0; i < arr.Length; i++)
                if (!self.RemoveEmptyEntries || !string.IsNullOrEmpty(arr[i]))
                    objs.Add(new DyString(arr[i]));

            return new DyTuple(objs.ToArray());
        }
        catch (RegexMatchTimeoutException)
        {
            return ctx.Timeout();
        }
    }

    [InstanceMethod]
    internal static DyObject Match(ExecutionContext ctx, DyRegex self, string input, int index = 0, int? count = null)
    {
        if (count is null)
            count = input.Length;

        if (index + count > input.Length)
            return ctx.IndexOutOfRange();

        try
        {
            var m = self.Regex.Match(input, index, count.Value);
            return new DyRegexMatch(m);
        }
        catch (RegexMatchTimeoutException)
        {
            return ctx.Timeout();
        }
    }

    [InstanceMethod]
    internal static DyObject Matches(ExecutionContext ctx, DyRegex self, string input, int index = 0)
    {
        if (index > input.Length)
            return ctx.IndexOutOfRange();

        var ms = self.Regex.Matches(input, index);
        var xs = new FastList<DyRegexMatch>();

        for (var i = 0; i < ms.Count; i++)
            xs.Add(new DyRegexMatch(ms[i]));

        return new DyTuple(xs.ToArray());
    }

    [InstanceMethod] 
    internal static bool IsMatch(ExecutionContext ctx, DyRegex self, string input, int index = 0)
    {
        if (index < 0 || index >= input.Length)
        {
            ctx.IndexOutOfRange(index);
            return default;
        }

        try
        {
            return self.Regex.IsMatch(input, index);
        }
        catch (RegexMatchTimeoutException) 
        {
            ctx.Timeout();
            return default;
        }
    }

    [StaticMethod("Regex")]
    internal static DyObject New(ExecutionContext ctx, string pattern, bool ignoreCase = false, bool singleline = false, bool multiline = false, bool removeEmptyEntries = false)
    {
        return new DyRegex(ctx.Type<DyRegexTypeInfo>(), pattern, ignoreCase, singleline, multiline, removeEmptyEntries);
    }
}
