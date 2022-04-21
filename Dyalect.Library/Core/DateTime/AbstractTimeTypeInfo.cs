using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public abstract class AbstractTimeTypeInfo<T> : DyForeignTypeInfo<CoreModule>
        where T : DyObject, ITime, IFormattable
    {
        public override string TypeName { get; }

        protected AbstractTimeTypeInfo(string typeName) => TypeName = typeName;

        protected override SupportedOperations GetSupportedOperations() =>
           SupportedOperations.Gt | SupportedOperations.Gte | SupportedOperations.Lt | SupportedOperations.Lte;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx)
        {
            if (format.IsNil())
                return new DyString(arg.ToString());

            if (format.NotString(ctx)) return Default();

            try
            {
                return new DyString(((T)arg).ToString(format.GetString(), null));
            }
            catch (FormatException)
            {
                return ctx.ParsingFailed();
            }
        }

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.False;

            return ((T)left).TotalTicks == ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject NeqOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return DyBool.True;

            return ((T)left).TotalTicks != ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject GtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((T)left).TotalTicks > ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject LtOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((T)left).TotalTicks < ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject GteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((T)left).TotalTicks >= ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject LteOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return ((T)left).TotalTicks <= ((T)right).TotalTicks ? DyBool.True : DyBool.False;
        }

        protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
            targetType.ReflectedTypeId switch
            {
                DyType.Integer => DyInteger.Get(((T)self).TotalTicks),
                _ => base.CastOp(self, targetType, ctx)
            };

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
                "Hours" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Hours)),
                "Minutes" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Minutes)),
                "Seconds" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Seconds)),
                "Milliseconds" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Milliseconds)),
                "Ticks" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Ticks)),
                "TotalTicks" => Func.Auto(name, (_, self) => new DyInteger(((T)self).TotalTicks)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected abstract DyObject Parse(string format, string input);

        protected abstract DyObject Create(long ticks);

        private DyObject Parse(ExecutionContext ctx, DyObject input, DyObject format)
        {
            if (input.NotString(ctx)) return Default();
            if (format.NotString(ctx)) return Default();

            try
            {
                return Parse(format.GetString(), input.GetString());
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
              Method.Parse => Func.Static(name, Parse, -1, new Par("input"), new Par("format")),
              _ => base.InitializeStaticMember(name, ctx)
          };
    }
}
