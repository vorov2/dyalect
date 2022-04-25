using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyCalendarTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        public override string ReflectedTypeName => "Calendar";

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        private DyObject DaysInMonth(ExecutionContext ctx, DyObject year, DyObject month)
        {
            if (!year.IsInteger(ctx)) return Nil;
            if (!month.IsInteger(ctx)) return Nil;
            return new DyInteger(DateTime.DaysInMonth((int)year.GetInteger(), (int)month.GetInteger()));
        }

        private DyObject FirstDayOfMonth(ExecutionContext ctx, DyObject value)
        {
            if (value is DyDateTime dt)
                return dt.FirstDayOfMonth();
            else
                return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.LocalDateTime.TypeId, value);
        }

        private DyObject LastDayOfMonth(ExecutionContext ctx, DyObject value)
        {
            if (value is DyDateTime dt)
                return dt.LastDayOfMonth();
            else
                return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.LocalDateTime.TypeId, value);
        }

        private DyObject DaysInYear(ExecutionContext ctx, DyObject year)
        {
            if (!year.IsInteger(ctx)) return Nil;
            return new DyInteger(DateTime.IsLeapYear((int)year.GetInteger()) ? 366 : 365);
        }

        private DyObject IsLeapYear(ExecutionContext ctx, DyObject year)
        {
            if (!year.IsInteger(ctx)) return Nil;
            return DateTime.IsLeapYear((int)year.GetInteger()) ? DyBool.True : DyBool.False;
        }

        private DyObject ParseDateTime(ExecutionContext ctx, DyObject input, DyObject format)
        {
            if (input.NotString(ctx)) return Nil;
            if (format.NotString(ctx)) return Nil;
            var (ticks, offset) = InputParser.Parse(FormatParser.LocalDateTimeParser, format.GetString(), input.GetString());
            
            if (offset is 0)
                return new DyDateTime(DeclaringUnit.DateTime, ticks);
            else
                return new DyLocalDateTime(DeclaringUnit.LocalDateTime, ticks,
                        new DyTimeDelta(DeclaringUnit.TimeDelta, offset));
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "DaysInMonth" => Func.Static(name, DaysInMonth, -1, new Par("year"), new Par("month")),
                "FirstDayOfMonth" => Func.Static(name, FirstDayOfMonth, -1, new Par("value")),
                "LastDayOfMonth" => Func.Static(name, LastDayOfMonth, -1, new Par("value")),
                "DaysInYear" => Func.Static(name, DaysInYear, -1, new Par("year")),
                "IsLeapYear" => Func.Static(name, IsLeapYear, -1, new Par("year")),
                "ParseDateTime" => Func.Static(name, ParseDateTime, -1, new Par("input"), new Par("format")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
