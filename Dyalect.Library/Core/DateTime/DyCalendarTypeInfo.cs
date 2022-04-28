using Dyalect.Codegen;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

[GeneratedType]
public sealed partial class DyCalendarTypeInfo : DyForeignTypeInfo<CoreModule>
{
    public override string ReflectedTypeName => "Calendar";

    protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

    [StaticMethod]
    internal static int DaysInMonth(int year, int month) => DateTime.DaysInMonth(year, month);

    [StaticMethod]
    internal static DyObject FirstDayOfMonth(DyDateTime value) => value.FirstDayOfMonth();

    [StaticMethod]
    internal static DyObject LastDayOfMonth(DyDateTime value) => value.LastDayOfMonth();

    [StaticMethod]
    internal static int DaysInYear(int year) => DateTime.IsLeapYear(year) ? 366 : 365;

    [StaticMethod]
    internal static bool IsLeapYear(int year) => DateTime.IsLeapYear(year);

    [StaticMethod]
    internal static DyObject ParseDateTime(ExecutionContext ctx, string input, string format)
    {
        var (ticks, offset) = InputParser.Parse(FormatParser.LocalDateTimeParser, format, input);
        
        if (offset is 0)
            return new DyDateTime(ctx.Type<DyDateTimeTypeInfo>(), ticks);
        else
            return new DyLocalDateTime(ctx.Type<DyLocalDateTimeTypeInfo>(), ticks,
                new DyTimeDelta(ctx.Type<DyTimeDeltaTypeInfo>(), offset));
    }
}
