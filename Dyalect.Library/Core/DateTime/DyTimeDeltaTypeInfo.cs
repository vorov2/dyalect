using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeDeltaTypeInfo : AbstractTimeTypeInfo<DyTimeDelta>
    {
        private const string TimeDelta = "TimeDelta";

        public DyTimeDeltaTypeInfo() : base(TimeDelta) => AddMixin(DyType.Comparable);

        protected override SupportedOperations GetSupportedOperations() =>
            base.GetSupportedOperations() | SupportedOperations.Add | SupportedOperations.Sub | SupportedOperations.Neg;

        protected override DyObject NegOp(DyObject arg, ExecutionContext ctx) => ((DyTimeDelta)arg).Negate();

        protected override DyObject AddOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks + ((DyTimeDelta)right).TotalTicks);
        }

        protected override DyObject SubOp(DyObject left, DyObject right, ExecutionContext ctx)
        {
            if (right.TypeId != left.TypeId)
                return ctx.InvalidType(left.TypeId, right);

            try
            {
                return new DyTimeDelta(this, ((DyTimeDelta)left).TotalTicks - ((DyTimeDelta)right).TotalTicks);
            }
            catch (OverflowException)
            {
                return ctx.Overflow();
            }
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Days" => Func.Auto(name, (_, self) => new DyInteger(((DyTimeDelta)self).Days)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected override DyObject Parse(string format, string input) => DyTimeDelta.Parse(this, format, input);

        protected override DyObject Create(long ticks) => new DyTimeDelta(this, ticks);

        private DyObject New(ExecutionContext ctx, DyObject days, DyObject hours, DyObject minutes, DyObject sec, DyObject ms) =>
            CreateNew(ctx, days, hours, minutes, sec, ms);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                TimeDelta => Func.Static(name, New, -1, new Par("days", DyInteger.Zero), new Par("hours", DyInteger.Zero),
                    new Par("minutes", DyInteger.Zero), new Par("seconds", DyInteger.Zero), new Par("milliseconds", DyInteger.Zero),
                    new Par("ticks", DyInteger.Zero)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
