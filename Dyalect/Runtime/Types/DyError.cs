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

        internal Stack<StackPoint>? Dump { get; set; }

        public DyErrorCode Code { get; }

        public object[] DataItems { get; }

        public string GetDescription()
        {
            var str = RuntimeErrors.ResourceManager.GetString(errorCode);

            if (str is not null)
            {
                if (DataItems is not null && DataItems.Length > 0)
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
            if (index.TypeId != DyType.String)
                return ctx.InvalidType(index);

            var name = index.GetString();

            if (name == "code")
                return new DyInteger((int)Code);
            else if (name == "detail")
                return GetDetail();
            else
                return ctx.IndexOutOfRange();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            name == "code" || name == "detail";

        public override string GetConstructor(ExecutionContext ctx) => errorCode;

        public override int GetHashCode() => HashCode.Combine(Code, DataItems);
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
                return err.GetItem(index, ctx);
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
