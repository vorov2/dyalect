using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeTypeInfo : AbstractTimeTypeInfo<DyTime>
    {
        private const string Time = "Time";

        public DyTimeTypeInfo() : base(Time) { }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "Hour" => Func.Auto(name, GetHours),
                "Minute" => Func.Auto(name, GetMinutes),
                "Second" => Func.Auto(name, GetSeconds),
                "Millisecond" => Func.Auto(name, GetMilliseconds),
                "Tick" => Func.Auto(name, GetTicks),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject New(ExecutionContext ctx, DyObject hours, DyObject minutes, DyObject sec, DyObject ms) =>
            CreateNew(ctx, DyInteger.Zero, hours, minutes, sec, ms);

        protected override DyObject Parse(string format, string input) => DyTime.Parse(this, format, input);

        protected override DyObject Create(long ticks) => new DyTime(this, ticks);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Time => Func.Static(name, New, -1, new Par("hour", DyInteger.Zero),
                    new Par("minute", DyInteger.Zero), new Par("second", DyInteger.Zero), new Par("millisecond", DyInteger.Zero),
                    new Par("tick", DyInteger.Zero)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
