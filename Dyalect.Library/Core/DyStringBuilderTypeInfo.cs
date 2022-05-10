using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Linq;
using System.Text;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyStringBuilderTypeInfo : DyForeignTypeInfo
{
    private const string StringBuilder = nameof(StringBuilder);

    public override string ReflectedTypeName => StringBuilder;

    public DyStringBuilderTypeInfo()
    {
        SetSupportedOperations(Ops.Len);
    }

    #region Operations
    public DyStringBuilder Create(StringBuilder sb) => new(this, sb);

    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(((DyStringBuilder)arg).ToString());

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg) =>
        DyInteger.Get(((DyStringBuilder)arg).Builder.Length);

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var a = ((DyStringBuilder)left).Builder;
        var b = ((DyStringBuilder)right).Builder;
        return a.ToString() == b.ToString() ? True : False;
    }
    #endregion

    [InstanceMethod]
    internal static DyObject Append(ExecutionContext ctx, DyStringBuilder self, DyObject value)
    {
        var str = value.ToString(ctx).Value;

        if (ctx.HasErrors)
            return Nil;

        self.Builder.Append(str);
        return self;
    }

    [InstanceMethod]
    internal static DyObject AppendLine(ExecutionContext ctx, DyStringBuilder self, [Default("")]DyObject value)
    {
        var str = value.ToString(ctx).Value;

        if (ctx.HasErrors)
            return Nil;

        self.Builder.AppendLine(str);
        return self;
    }

    [InstanceMethod]
    internal static DyObject Replace(ExecutionContext ctx, DyStringBuilder self, DyObject value, DyObject other)
    {
        var a = value.ToString(ctx).Value;
        var b = other.ToString(ctx).Value;

        if (ctx.HasErrors)
            return Nil;

        self.Builder.Replace(a, b);
        return self;
    }

    [InstanceMethod]
    internal static DyObject Remove(ExecutionContext ctx, DyStringBuilder self, int index, int count)
    {
        if (index + count >= self.Builder.Length)
            return ctx.IndexOutOfRange();

        self.Builder.Remove(index, count);
        return self;
    }

    [InstanceMethod]
    internal static DyObject Insert(ExecutionContext ctx, DyStringBuilder self, int index, DyObject value)
    {
        var str = value.ToString(ctx).Value;

        if (ctx.HasErrors)
            return Nil;

        if (index < 0 || index >= self.Builder.Length)
            return ctx.IndexOutOfRange();

        self.Builder.Insert(index, str);
        return self;
    }

    [StaticMethod(StringBuilder)]
    internal static DyObject New(ExecutionContext ctx, [VarArg]DyTuple values)
    {
        if (values.Count > 0)
        {
            var vals = DyIterator.ToEnumerable(ctx, values);
            var arr = vals.Select(o => o.ToString(ctx).Value).ToArray();
            var sb = new StringBuilder(string.Join("", arr));
            return new DyStringBuilder(ctx.Type<DyStringBuilderTypeInfo>(), sb);
        }
        else
            return new DyStringBuilder(ctx.Type<DyStringBuilderTypeInfo>(), new StringBuilder());
    }
}
