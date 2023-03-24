using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyTimeTypeInfo : SpanTypeInfo<DyTime>
{
    private const string Time = nameof(Time);

    public DyTimeTypeInfo() : base(Time) { }

    [InstanceProperty]
    internal static int Hour(DyTime self) => self.Hours;

    [InstanceProperty]
    internal static int Minute(DyTime self) => self.Minutes;

    [InstanceProperty]
    internal static int Second(DyTime self) => self.Seconds;

    [InstanceProperty]
    internal static int Millisecond(DyTime self) => self.Milliseconds;

    [InstanceProperty]
    internal static int Tick(DyTime self) => self.Ticks;

    [InstanceProperty]
    internal static long TotalTicks(DyTime self) => self.TotalTicks;

    [StaticMethod(Time)]
    internal static DyObject CreateNew(ExecutionContext ctx, int hour = 0, int minute = 0, int second = 0, int millisecond = 0, int tick = 0)
    {
        var ticks = tick + DT.Sum(0, hour, minute, second, millisecond);
        return new DyTime(ctx.Type<DyTimeTypeInfo>(), ticks);
    }

    [StaticMethod]
    internal static DyObject FromTicks(ExecutionContext ctx, long ticks) => new DyTime(ctx.Type<DyTimeTypeInfo>(), ticks);

    [StaticMethod]
    internal static DyObject Parse(ExecutionContext ctx, string input, string format)
    {
        try
        {
            return DyTime.Parse(ctx.Type<DyTimeTypeInfo>(), format, input);
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

    [StaticProperty]
    internal static DyTime Default(ExecutionContext ctx) => Min(ctx);

    [StaticProperty]
    internal static DyTime Min(ExecutionContext ctx) => new(ctx.Type<DyTimeTypeInfo>(), DateTime.MinValue.TimeOfDay.Ticks);

    [StaticProperty]
    internal static DyTime Max(ExecutionContext ctx) => new(ctx.Type<DyTimeTypeInfo>(), DateTime.MaxValue.TimeOfDay.Ticks);
}
