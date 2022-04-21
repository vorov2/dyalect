using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;

namespace Dyalect.Library.Core
{
    public sealed class DyTimeTypeInfo : AbstractTimeTypeInfo<DyTime>
    {
        private const string Time = "Time";

        public DyTimeTypeInfo() : base(Time) { }

        private DyObject New(ExecutionContext ctx, DyObject hours, DyObject minutes, DyObject sec, DyObject ms) =>
            CreateNew(ctx, DyInteger.Zero, hours, minutes, sec, ms);

        protected override DyObject Parse(string format, string input) => DyTime.Parse(this, format, input);

        protected override DyObject Create(long ticks) => new DyTime(this, ticks);

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                Time => Func.Static(name, New, -1, new Par("hours", DyInteger.Zero),
                    new Par("minutes", DyInteger.Zero), new Par("seconds", DyInteger.Zero), new Par("milliseconds", DyInteger.Zero),
                    new Par("ticks", DyInteger.Zero)),
                _ => base.InitializeStaticMember(name, ctx)
            };
    }
}
