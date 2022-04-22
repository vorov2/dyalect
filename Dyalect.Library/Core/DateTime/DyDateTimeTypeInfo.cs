using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core;

public sealed class DyDateTimeTypeInfo : AbstractDateTimeTypeInfo<DyDateTime>
{
    private const string DateTimeType = "DateTime";

    protected override SupportedOperations GetSupportedOperations() =>
        base.GetSupportedOperations() | SupportedOperations.Sub | SupportedOperations.Add;

    public DyDateTimeTypeInfo() : base(DateTimeType) { }

    protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right is DyDateTime dt)
            try
            {
                return new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)left).Ticks - dt.Ticks);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(right);
            }
        else if (right is DyTimeDelta td)
            try
            {
                return new DyDateTime(this, ((DyDateTime)left).Ticks - td.TotalTicks);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(right);
            }

        return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.TimeDelta.TypeId, right);
    }

    protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right is DyTimeDelta td)
        {
            try
            {
                return new DyDateTime(this, ((DyDateTime)left).Ticks + td.TotalTicks);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue(right);
            }
        }

        return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
    }

    protected override DyObject Parse(string format, string input) => DyDateTime.Parse(this, format, input);

    protected override DyObject Create(long ticks, DyTimeDelta? offset) => new DyDateTime(this, ticks);

    private DyObject CreateNew(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond) =>
        New(ctx, year, month, day, hour, minute, second, millisecond, null);

    private DyObject FromTicks(ExecutionContext ctx, DyObject ticks)
    {
        if (ticks.NotNat(ctx)) return Default();
        return new DyDateTime(this, ticks.GetInteger());
    }

    protected override DyDateTime GetMax(ExecutionContext ctx) => new(this, DateTime.MaxValue.Ticks);

    protected override DyDateTime GetMin(ExecutionContext ctx) => new(this, DateTime.MinValue.Ticks);

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch 
        {
            DateTimeType => Func.Static(name, CreateNew, -1, new Par("year"), new Par("month"), new Par("day"), new Par("hour", 0),
                new Par("minute", 0), new Par("second", 0), new Par("millisecond", 0)),
            "Now" => Func.Static(name, _ => new DyDateTime(this, DateTime.UtcNow.Ticks)),
            "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value")),
            _ => base.InitializeStaticMember(name, ctx)
        };
}
