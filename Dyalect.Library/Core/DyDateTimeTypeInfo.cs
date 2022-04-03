using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyDateTimeTypeInfo : DyForeignTypeInfo<CoreModule>
    {
        private const string FORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz";
        public override string TypeName => "DateTime";

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Sub;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyDateTime)arg).Value.ToString(FORMAT, CI.Default));

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
                    return new DyDateTime(this, ((DyDateTime)left).Value - td.Value);
                }
                catch (Exception)
                {
                    return ctx.InvalidValue(right);
                }

            return ctx.InvalidType(DeclaringUnit.DateTime.TypeId, DeclaringUnit.TimeDelta.TypeId, right);
        }

        private DyObject ToString(ExecutionContext ctx, DyObject self, DyObject format)
        {
            if (format.TypeId == DyType.Nil)
                return ToStringOp(self, ctx);

            try
            {
                var res = ((DyDateTime)self).Value.ToString(format.GetString(), CI.Default);
                return new DyString(res);
            }
            catch (Exception)
            {
                return ctx.InvalidValue(format);
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToString, -1, new Par("format", DyNil.Instance)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Parse(ExecutionContext ctx, DyObject format, DyObject value)
        {
            if (!value.IsString(ctx)) return Default();
            if (!format.IsString(ctx)) return Default();

            try
            {
                return new DyDateTime(this, DateTime.ParseExact(value.GetString(), format.GetString(), CI.Default));
            }
            catch (FormatException ex)
            {
                return ctx.ParsingFailed(ex.Message);
            }
        }

        private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day, DyObject hour, DyObject minute, DyObject second, DyObject millisecond)
        {
            if (!year.IsInteger(ctx)) return Default();
            if (!month.IsInteger(ctx)) return Default();
            if (!day.IsInteger(ctx)) return Default();
            if (!hour.IsInteger(ctx)) return Default();
            if (!minute.IsInteger(ctx)) return Default();
            if (!second.IsInteger(ctx)) return Default();
            if (!millisecond.IsInteger(ctx)) return Default();

            try
            {
                var dt = new DateTime((int)year.GetInteger(), (int)month.GetInteger(), (int)day.GetInteger(), (int)hour.GetInteger(),
                    (int)minute.GetInteger(), (int)second.GetInteger(), (int)millisecond.GetInteger(), DateTimeKind.Local);
                return new DyDateTime(this, dt);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks)
        {
            if (!ticks.IsInteger(ctx)) return Default();

            try
            {
                var dt = new DateTime((int)ticks.GetInteger(), DateTimeKind.Local);
                return new DyDateTime(this, dt);
            }
            catch (ArgumentOutOfRangeException)
            {
                return ctx.InvalidValue();
            }
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch 
            {
                "Parse" => Func.Static(name, Parse, -1, new Par("format"), new Par("value")),
                "DateTime" => Func.Static(name, New, -1, new Par("year", DyInteger.Zero), new Par("month", DyInteger.Zero), 
                    new Par("day", DyInteger.Zero), new Par("hour", DyInteger.Zero), new Par("minute", DyInteger.Zero),
                    new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero)),
                "FromTicks" => Func.Static(name, FromTicks, -1, new Par("value")),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
