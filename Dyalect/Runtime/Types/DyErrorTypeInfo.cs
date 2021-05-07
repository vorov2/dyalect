using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyErrorTypeInfo : DyTypeInfo
    {
        public DyErrorTypeInfo() : base(DyType.Error) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
            | SupportedOperations.Len | SupportedOperations.Get;

        protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx) =>
            DyInteger.Get(((DyError)arg).DataItems?.Length ?? 0);

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            var err = (DyError)self;

            if (index.TypeId == DyType.Integer)
            {
                var idx = index.GetInteger();

                if (idx < 0 || idx >= err.DataItems.Length)
                    return ctx.IndexOutOfRange();

                return TypeConverter.ConvertFrom(err.DataItems[idx]);
            }
            else if (index.TypeId == DyType.String)
                return err.GetItem(index, ctx);
            else
                return ctx.InvalidType(index);
        }

        public override string TypeName => DyTypeNames.Error;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext _) => new DyString(arg.ToString());

        protected override DyFunction InitializeStaticMember(string name, ExecutionContext ctx)
        {
            return Func.Static(name, (c, args) =>
            {
                if (!Enum.TryParse(name, out DyErrorCode code))
                    code = DyErrorCode.UnexpectedError;

                if (args is not null && args is DyTuple t)
                    return new DyError(name, code, t.Values);
                else
                    return new DyError(name, code);
            }, 0, new Par("values"));
        }
    }
}
