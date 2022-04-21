using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public sealed class DyDateTypeInfo : AbstractSpanTypeInfo<DyDate>
    {
        private const string Date = "Date";

        public DyDateTypeInfo() : base(Date) { }

        protected override SupportedOperations GetSupportedOperations() => base.GetSupportedOperations();

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
                "Year" => Func.Auto(name, (_, self) => new DyInteger(((DyDate)self).Year)),
                "Month" => Func.Auto(name, (_, self) => new DyInteger(((DyDate)self).Month)),
                "Day" => Func.Auto(name, (_, self) => new DyInteger(((DyDate)self).Day)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected override DyObject Parse(string format, string input) => DyDate.Parse(this, format, input);

        private DyObject New(ExecutionContext ctx, DyObject year, DyObject month, DyObject day)
        {
            if (year.NotNat(ctx)) return Default();
            if (month.NotNat(ctx)) return Default();
            if (day.NotNat(ctx)) return Default();

            if (year.GetInteger() == 0) return ctx.InvalidValue(year);
            if (month.GetInteger() == 0) return ctx.InvalidValue(year);
            if (day.GetInteger() == 0) return ctx.InvalidValue(year);

            DateTime dt;

            try
            {
                dt = new DateTime((int)year.GetInteger(), (int)month.GetInteger(), (int)day.GetInteger()).Date;
            }
            catch (Exception)
            {
                return ctx.Overflow();
            }

            return new DyDate(this, (int)(dt.Ticks / DT.TicksPerDay));
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
          name switch
          {
              Date => Func.Static(name, New, -1, new Par("year"), new Par("month"), new Par("day")),
              Method.Min => Func.Static(name, _ => new DyDate(this, (int)(DateTime.MinValue.Date.Ticks / DT.TicksPerDay))),
              Method.Max => Func.Static(name, _ => new DyDate(this, (int)(DateTime.MaxValue.Date.Ticks / DT.TicksPerDay))),
              Method.Default => Func.Static(name, _ => new DyDate(this, (int)(DateTime.MinValue.Date.Ticks / DT.TicksPerDay))),
              _ => base.InitializeStaticMember(name, ctx)
          };
    }
}
