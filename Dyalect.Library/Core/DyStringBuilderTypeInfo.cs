using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Linq;
using System.Text;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyStringBuilderTypeInfo : DyForeignTypeInfo
{
    public override string ReflectedTypeName => "StringBuilder";

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neg | SupportedOperations.Not | SupportedOperations.Len;

    #region Operations
    public DyStringBuilder Create(StringBuilder sb) => new(this, sb);

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(((DyStringBuilder)arg).ToString());

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
        DyInteger.Get(((DyStringBuilder)arg).Builder.Length);

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        var a = ((DyStringBuilder)left).Builder;
        var b = ((DyStringBuilder)right).Builder;
        return a.ToString() == b.ToString() ? DyBool.True : DyBool.False;
    }
    #endregion

    [InstanceMethod]
    internal static DyObject Append(ExecutionContext ctx, DyStringBuilder self, DyObject value)
    {
        var str = DyString.ToString(value, ctx);

        if (ctx.HasErrors)
            return Nil;

        self.Builder.Append(str);
        return self;
    }

    [InstanceMethod]
    internal static DyObject AppendLine(ExecutionContext ctx, DyStringBuilder self, [Default("")]DyObject value)
    {
        var str = DyString.ToString(value, ctx);

        if (ctx.HasErrors)
            return Nil;

        self.Builder.AppendLine(str);
        return self;
    }

    [InstanceMethod]
    internal static DyObject Replace(ExecutionContext ctx, DyStringBuilder self, DyObject value, DyObject other)
    {
        var a = DyString.ToString(value, ctx);
        var b = DyString.ToString(other, ctx);

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
        var str = DyString.ToString(value, ctx);

        if (ctx.HasErrors)
            return Nil;

        if (index < 0 || index >= self.Builder.Length)
            return ctx.IndexOutOfRange();

        self.Builder.Insert(index, str);
        return self;
    }

    [StaticMethod("StringBuilder")]
    internal static DyObject New(ExecutionContext ctx, [VarArg]DyTuple values)
    {
        if (values.Count > 0)
        {
            var vals = DyIterator.ToEnumerable(ctx, values);
            var arr = vals.Select(o => DyString.ToString(o, ctx)).ToArray();
            var sb = new StringBuilder(string.Join("", arr));
            return new DyStringBuilder(ctx.Type<DyStringBuilderTypeInfo>(), sb);
        }
        else
            return new DyStringBuilder(ctx.Type<DyStringBuilderTypeInfo>(), new StringBuilder());
    }
}
