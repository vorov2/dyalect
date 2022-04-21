using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyDateTimeTypeInfo : DyBaseDateTimeTypeInfo
    {
        private const string FORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffff";
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Sub | SupportedOperations.Add
            | SupportedOperations.Gt | SupportedOperations.Gte
            | SupportedOperations.Lt | SupportedOperations.Lte;

        public DyDateTimeTypeInfo() : base("DateTime") { }

        protected override DyDateTime CreateDateTime(DateTime dateTime, TimeSpan? offset) => new DyDateTime(this, dateTime);

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.TypeId == DyType.Nil)
                return new DyString(((DyDateTime)arg).Value.ToString(FORMAT, CI.Default));

            try
            {
                var res = ((DyDateTime)arg).Value.ToString(format.GetString(), CI.Default);
                return new DyString(res);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(format);
            }
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right is DyDateTime dt)
                try
                {
                    return new DyTimeDelta(DeclaringUnit.TimeDelta, ((DyDateTime)left).Value - dt.Value);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }
            else if (right is DyTimeDelta td)
                try
                {
                    return new DyDateTime(this, new DateTime(((DyDateTime)left).Value.Ticks - td.TotalTicks, DateTimeKind.Utc));
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
                    return new DyDateTime(this, new DateTime(((DyDateTime)left).Value.Ticks + td.TotalTicks, DateTimeKind.Utc));
                }
                catch (ArgumentOutOfRangeException)
                {
                    return ctx.InvalidValue(right);
                }
            }

            return ctx.InvalidType(DeclaringUnit.TimeDelta.TypeId, right);
        }

        private DyObject Parse(ExecutionContext ctx, DyObject value, DyObject format)
        {
            if (!value.IsString(ctx)) return Default();
            if (!format.IsString(ctx)) return Default();

            try
            {
                var dt = DateTime.ParseExact(value.GetString(), format.GetString(), CI.Default);
                return new DyDateTime(this, DateTime.SpecifyKind(dt, DateTimeKind.Utc));
            }
            catch (FormatException ex)
            {
                return ctx.ParsingFailed(ex.Message);
            }
        }

        private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond) =>
            New(ctx, year, month, day, hour, minute, second, millisecond, null);

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks) => FromTicks(ctx, ticks, null);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch 
            {
                "Now" => Func.Static(name, _ => new DyDateTime(this, DateTime.UtcNow)),
                "Today" => Func.Static(name, _ => new DyDateTime(this, DateTime.UtcNow.Date)),
                "Min" => Func.Static(name, _ => new DyDateTime(this, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc))),
                "Max" => Func.Static(name, _ => new DyDateTime(this, DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc))),
                "Default" => Func.Static(name, _ => new DyDateTime(this, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc))),
                "Parse" => Func.Static(name, Parse, -1, new Par("value"), new Par("format")),
                "DateTime" => Func.Static(name, New, -1, new Par("year"), new Par("month"), new Par("day"), new Par("hour", DyInteger.Zero), new Par("minute", DyInteger.Zero),
                    new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero)),
                "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
