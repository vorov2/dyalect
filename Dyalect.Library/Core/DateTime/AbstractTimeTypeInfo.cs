using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public abstract class AbstractTimeTypeInfo<T> : AbstractSpanTypeInfo<T>
        where T : DyObject, ITime, IFormattable
    {
        protected AbstractTimeTypeInfo(string typeName) : base(typeName) { }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
                "Hours" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Hours)),
                "Minutes" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Minutes)),
                "Seconds" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Seconds)),
                "Milliseconds" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Milliseconds)),
                "Ticks" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Ticks)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected DyObject CreateNew(ExecutionContext ctx, DyObject days, DyObject hours, DyObject minutes, DyObject sec, DyObject ms)
        {
            if (days.NotNat(ctx)) return Default();
            if (hours.NotNat(ctx)) return Default();
            if (minutes.NotNat(ctx)) return Default();
            if (sec.NotNat(ctx)) return Default();
            if (ms.NotNat(ctx)) return Default();

            var totalTicks =
                days.GetInteger() * TimeSpan.TicksPerDay
                + hours.GetInteger() * TimeSpan.TicksPerHour
                + minutes.GetInteger() * TimeSpan.TicksPerMinute
                + sec.GetInteger() * TimeSpan.TicksPerSecond
                + ms.GetInteger() * TimeSpan.TicksPerMillisecond;

            return Create(totalTicks);
        }

        protected abstract DyObject Create(long ticks);

        private DyObject FromTicks(ExecutionContext ctx, DyObject ticks)
        {
            if (ticks.NotNat(ctx)) return Default();
            return Create(ticks.GetInteger());
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
          name switch
          {
              Method.FromTicks => Func.Static(name, FromTicks, -1, new Par("value")),
              Method.Min => Func.Static(name, _ => Create(TimeSpan.MinValue.Ticks)),
              Method.Max => Func.Static(name, _ => Create(TimeSpan.MaxValue.Ticks)),
              Method.Default => Func.Static(name, _ => Create(TimeSpan.Zero.Ticks)),
              _ => base.InitializeStaticMember(name, ctx)
          };
    }
}
