using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyTimeDeltaTypeInfo : SpanTypeInfo<DyTimeDelta>
{
    private const string TimeDelta = nameof(TimeDelta);

    public DyTimeDeltaTypeInfo() : base(TimeDelta) { }

    protected override SupportedOperations GetSupportedOperations() =>
        base.GetSupportedOperations() | SupportedOperations.Add | SupportedOperations.Sub | SupportedOperations.Neg;

    #region Operations
    protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => ((DyTimeDelta)arg).Negate();

    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks + ((DyTimeDelta)right).TotalTicks);
    }

    protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right.TypeId != left.TypeId)
            return ctx.InvalidType(left.TypeId, right);

        try
        {
            return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks - ((DyTimeDelta)right).TotalTicks);
        }
        catch (OverflowException)
        {
            return ctx.Overflow();
        }
    }
    #endregion

    [InstanceProperty]
    internal static int Days(DyTimeDelta self) => self.Days;

    [InstanceProperty]
    internal static int Hours(DyTimeDelta self) => self.Hours;

    [InstanceProperty]
    internal static int Minutes(DyTimeDelta self) => self.Minutes;

    [InstanceProperty]
    internal static int Seconds(DyTimeDelta self) => self.Seconds;

    [InstanceProperty]
    internal static int Milliseconds(DyTimeDelta self) => self.Milliseconds;

    [InstanceProperty]
    internal static int Ticks(DyTimeDelta self) => self.Ticks;

    [InstanceProperty]
    internal static long TotalTicks(DyTimeDelta self) => self.TotalTicks;

    [InstanceMethod]
    internal static DyObject Negate(DyTimeDelta self) => self.Negate();

    [StaticMethod]
    internal static DyObject FromTicks(ExecutionContext ctx, long ticks) =>
        new DyTimeDelta(ctx.Type<DyTimeDeltaTypeInfo>(), ticks);

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string input, string format)
    {
        try
        {
            return DyTimeDelta.Parse(ctx.Type<DyTimeDeltaTypeInfo>(), format, input);
        }
        catch (FormatException)
        {
            return ctx.ParsingFailed();
        }
        catch (OverflowException)
        {
            return ctx.Overflow();
        }
    }

    [StaticMethod(TimeDelta)]
    internal static DyObject New(ExecutionContext ctx, int days = 0, int hours = 0, int minutes = 0,
        int seconds = 0, int milliseconds = 0, long ticks = 0)
    {
        ticks += DT.Sum(days, hours, minutes, seconds, milliseconds);
        return new DyTimeDelta(ctx.Type<DyTimeDeltaTypeInfo>(), ticks);
    }

    [StaticMethod]
    internal static DyTimeDelta Default(ExecutionContext ctx) => new(ctx.Type<DyTimeDeltaTypeInfo>(), TimeSpan.Zero.Ticks);

    [StaticMethod]
    internal static DyTimeDelta Min(ExecutionContext ctx) => new(ctx.Type<DyTimeDeltaTypeInfo>(), TimeSpan.MinValue.Ticks);

    [StaticMethod]
    internal static DyTimeDelta Max(ExecutionContext ctx) => new(ctx.Type<DyTimeDeltaTypeInfo>(), TimeSpan.MaxValue.Ticks);
}
