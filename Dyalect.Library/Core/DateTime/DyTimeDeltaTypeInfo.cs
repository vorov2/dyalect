using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Text;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDeltaTypeInfo : DyForeignTypeInfo
    {
        public override string TypeName => "TimeDelta";

        #region Standard operations
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Add | SupportedOperations.Sub | SupportedOperations.Get
            | SupportedOperations.Gt | SupportedOperations.Gte | SupportedOperations.Lt
            | SupportedOperations.Lte | SupportedOperations.Eq | SupportedOperations.Neq;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.TypeId == DyType.Nil)
                return new DyString(arg.ToString());

            if (!format.IsString(ctx)) return Default();

            try
            {
                var res = FormatTimeSpan(((DyTimeDelta)arg).Value, format.GetString());
                return new DyString(res);
            }
            catch (FormatException)
            {
                return ctx.ParsingFailed();
            }
        }

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return new DyTimeDelta(this, ((DyTimeDelta)left).Value + ((DyTimeDelta)right).Value);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            try
            {
                return new DyTimeDelta(this, ((DyTimeDelta)left).Value - ((DyTimeDelta)right).Value);
            }
            catch (OverflowException)
            {
                return ctx.Overflow();
            }
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.False;

            return ((DyTimeDelta)left).Value == ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.True ;

            return ((DyTimeDelta)left).Value != ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).Value > ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).Value < ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).Value >= ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((DyTimeDelta)left).Value <= ((DyTimeDelta)right).Value ? DyBool.True : DyBool.False;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get(((DyTimeDelta)self).Value.Ticks),
                _ => base.CastOp(self, targetType, ctx)
            };
        #endregion

        private string GetFormatString(string format)
        {
            var sb = new StringBuilder();
            const string FORMATCHARS = "dhmsfDHMSF";

            foreach (var c in format)
                if (FORMATCHARS.Contains(c))
                    sb.Append(c);
                else
                {
                    sb.Append('\\');
                    sb.Append(c);
                }

            return sb.ToString();
        }

        private string FormatTimeSpan(TimeSpan timeSpan, string format) => timeSpan.ToString(GetFormatString(format), CI.Default);

        private DyObject InternalParse(ExecutionContext ctx, Func<TimeSpan> parser)
        {
            try
            {
                return new DyTimeDelta(this, parser());
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

        private DyObject Parse(ExecutionContext ctx, DyObject input, DyObject format)
        {
            if (!input.IsString(ctx)) return Default();

            if (format.TypeId == DyType.Nil)
                return InternalParse(ctx, () => TimeSpan.Parse(input.GetString()));

            if (!format.IsString(ctx)) return Default();

            return InternalParse(ctx, () => TimeSpan.ParseExact(input.GetString(), GetFormatString(format.GetString()), CI.Default));
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
                "Days" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Days)),
                "Hours" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Hours)),
                "Minutes" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Minutes)),
                "Seconds" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Seconds)),
                "Milliseconds" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Milliseconds)),
                "Ticks" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Value.Ticks)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject days, DyObject hours, DyObject minutes, DyObject sec, DyObject ms, DyObject ticks)
        {
            if (!days.IsInteger(ctx)) return Default();
            if (!hours.IsInteger(ctx)) return Default();
            if (!minutes.IsInteger(ctx)) return Default();
            if (!sec.IsInteger(ctx)) return Default();
            if (!ms.IsInteger(ctx)) return Default();
            if (!ticks.IsInteger(ctx)) return Default();

            var totalTicks =
                days.GetInteger() * TimeSpan.TicksPerDay
                + hours.GetInteger() * TimeSpan.TicksPerHour
                + minutes.GetInteger() * TimeSpan.TicksPerMinute
                + sec.GetInteger() * TimeSpan.TicksPerSecond
                + ms.GetInteger() * TimeSpan.TicksPerMillisecond
                + ticks.GetInteger();

            return new DyTimeDelta(this, TimeSpan.FromTicks(totalTicks));
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "TimeDelta" => Func.Static(name, New, -1, new Par("days", DyInteger.Zero), new Par("hours", DyInteger.Zero),
                    new Par("minutes", DyInteger.Zero), new Par("seconds", DyInteger.Zero), new Par("milliseconds", DyInteger.Zero),
                    new Par("ticks", DyInteger.Zero)),
                Method.Min => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.MinValue)),
                Method.Max => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.MaxValue)),
                Method.Default => Func.Static(name, _ => new DyTimeDelta(this, TimeSpan.Zero)),
                Method.Parse => Func.Static(name, Parse, -1, new Par("input"), new Par("format", DyNil.Instance)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
