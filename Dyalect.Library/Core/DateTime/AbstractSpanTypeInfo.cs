using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;

namespace Dyalect.Library.Core
{
    public abstract class AbstractSpanTypeInfo<T> : DyForeignTypeInfo<CoreModule>
        where T : DyObject, ISpan, IFormattable
    {
        public override string TypeName { get; }

        protected AbstractSpanTypeInfo(string typeName)
        {
            TypeName = typeName;
            AddMixin(DyType.Comparable);
        }

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
                "TotalTicks" => Func.Auto(name, (_, self) => new DyInteger(((T)self).TotalTicks)),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        protected abstract DyObject Parse(string format, string input);

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

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
          name switch
          {
              Method.Parse => Func.Static(name, Parse, -1, new Par("input"), new Par("format")),
              _ => base.InitializeStaticMember(name, ctx)
          };
    }
}
