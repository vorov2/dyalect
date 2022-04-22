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

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get(((T)self).TotalTicks),
                _ => base.CastOp(self, targetType, ctx)
            };

        protected DyObject GetHours(ExecutionContext _, DyObject self) => new DyInteger(((T)self).Hours);
        protected DyObject GetMinutes(ExecutionContext _, DyObject self) => new DyInteger(((T)self).Minutes);
        protected DyObject GetSeconds(ExecutionContext _, DyObject self) => new DyInteger(((T)self).Seconds);
        protected DyObject GetMilliseconds(ExecutionContext _, DyObject self) => new DyInteger(((T)self).Milliseconds);
        protected DyObject GetTicks(ExecutionContext _, DyObject self) => new DyInteger(((T)self).Ticks);

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
