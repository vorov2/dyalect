using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
namespace Dyalect.Library.Core;

public abstract class AbstractDateTypeInfo<T> : AbstractSpanTypeInfo<T>
    where T : DyObject, IDate, IFormattable
{
    protected AbstractDateTypeInfo(string typeName) : base(typeName) { }

    protected virtual long ToInteger(DyObject self) => ((T)self).TotalTicks / DT.TicksPerDay;

    protected override DyObject CastOp(DyObject self, DyTypeInfo targetType, ExecutionContext ctx) =>
        targetType.ReflectedTypeId switch
        {
            DyType.Integer => DyInteger.Get(ToInteger(self)),
            _ => base.CastOp(self, targetType, ctx)
        };

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            "ToString" => Func.Member(name, ToStringWithFormat, -1, new Par("format", DyNil.Instance)),
            "Year" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Year)),
            "Month" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Month)),
            "Day" => Func.Auto(name, (_, self) => new DyInteger(((T)self).Day)),
            "DayOfWeek" => Func.Auto(name, (_, self) => new DyString(((T)self).DayOfWeek)),
            "DayOfYear" => Func.Auto(name, (_, self) => new DyInteger(((T)self).DayOfYear)),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    protected abstract T GetMin(ExecutionContext ctx);

    protected abstract T GetMax(ExecutionContext ctx);

    protected virtual T GetDefault(ExecutionContext ctx) => GetMin(ctx);

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
      name switch
      {
          Method.Min => Func.Static(name, GetMin),
          Method.Max => Func.Static(name, GetMax),
          Method.Default => Func.Static(name, GetDefault),
          _ => base.InitializeStaticMember(name, ctx)
      };
}
