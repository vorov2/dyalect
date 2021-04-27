using Dyalect.Debug;
using Dyalect.Strings;
using System;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types
{
    public sealed class DyError : DyObject
    {
        private readonly string errorCode;

        internal DyError(DyErrorCode code, params object[] dataItems) : this(code.ToString(), code, dataItems) { }

        internal DyError(string error, DyErrorCode code, params object[] dataItems) : base(DyType.Error)
        {
            Code = code;
            errorCode = error;
            DataItems = dataItems;
        }

        internal Stack<StackPoint> Dump { get; set; }

        public DyErrorCode Code { get; }

        public object[] DataItems { get; }

        public string GetDescription()
        {
            var str = RuntimeErrors.ResourceManager.GetString(errorCode);

            if (str is not null)
            {
                if (DataItems is not null)
                    str = str.Format(DataItems);

                return str;
            }

            if (DataItems is not null)
                return string.Join(",", DataItems);

            return errorCode;
        }

        internal DyObject GetDetail() => new DyString(GetDescription());

        public override object ToObject() => GetDescription();

        public override string ToString() => errorCode + ": " + GetDescription();

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (!TryGetItem(index, ctx, out var value))
                return ctx.IndexOutOfRange();

            return value;
        }

        protected internal override bool TryGetItem(DyObject index, ExecutionContext ctx, out DyObject value)
        {
            if (index.TypeId != DyType.String)
            {
                value = null;
                ctx.InvalidType(index);
                return false;
            }

            var name = index.GetString();

            if (name == "code")
            {
                value = new DyInteger((int)Code);
                return true;
            }
            else if (name == "detail")
            {
                value = GetDetail();
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            name == "code" || name == "detail";

        public override string GetConstructor(ExecutionContext ctx) => errorCode;
    }

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
            {
                if (!err.TryGetItem(index, ctx, out var value))
                    return ctx.IndexOutOfRange();

                return value;
            }
            else
                return ctx.InvalidType(index);
        }

        public override string TypeName => DyTypeNames.Error;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext _) => new DyString(arg.ToString());

        protected override DyFunction InitializeStaticMember(string name, ExecutionContext ctx)
        {
            return DyForeignFunction.Static(name, (c, args) => 
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
