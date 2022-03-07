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

        internal DyError(string error, DyErrorCode code, params object[] dataItems) : base(DyType.Error) =>
            (errorCode, Code, DataItems) = (error, code, dataItems);

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

        internal DyObject GetDetail(ExecutionContext ctx) => new DyString(GetDescription());

        public override object ToObject() => GetDescription();

        public override string ToString() => errorCode + ": " + GetDescription();

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.String)
                return ctx.InvalidType(index);

            var name = index.GetString();

            if (name is "code")
                return new DyInteger((int)Code);
            
            if (name is "detail")
                return GetDetail(ctx);
            
            return ctx.IndexOutOfRange();
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) => name is "code" or "detail";

        public override void GetConstructor(ExecutionContext ctx, out string ctor, out bool priv) => (ctor, priv) = (errorCode, false);

        public override int GetHashCode() => HashCode.Combine(Code, DataItems);
    }
}
