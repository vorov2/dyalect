using Dyalect.Debug;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyError : DyObject
    {
        internal DyError(DyErrorCode code, params object[] dataItems) : base(DyType.Object)
        {
            Code = code;
            DataItems = dataItems;
        }

        internal Stack<StackPoint> Dump { get; set; }

        public DyErrorCode Code { get; }

        public object[] DataItems { get; }

        public virtual string GetDescription()
        {
            var key = Code.ToString();
            var str = RuntimeErrors.ResourceManager.GetString(key);

            if (DataItems != null)
                str = str.Format(DataItems);

            return str;
        }

        internal virtual DyObject GetDetail() => new DyString(GetDescription());

        public override object ToObject() => GetDescription();

        public override string ToString() => Code.ToString() + ": " + GetDescription();

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            if (!TryGetItem(name, ctx, out var value))
                return ctx.IndexOutOfRange();

            return value;
        }

        protected internal override bool TryGetItem(string name, ExecutionContext ctx, out DyObject value)
        {
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

        public override string GetConstructor(ExecutionContext ctx) => Code.ToString();
    }

    internal sealed class DyUserError : DyError
    {
        public DyUserError(DyObject data) : this(DyErrorCode.UserCode, data) { }

        internal DyUserError(DyErrorCode code, DyObject data) : base(code) => Data = data;

        public DyObject Data { get; }

        public override string GetDescription() => Data.ToObject()?.ToString();

        internal override DyObject GetDetail() => Data ?? DyNil.Instance;
    }

    internal sealed class DyErrorTypeInfo : DyTypeInfo
    {
        public DyErrorTypeInfo() : base(DyType.Error) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Error;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext _) => new DyString(arg.ToString());

        protected override DyFunction InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (Enum.TryParse(name, out DyErrorCode res))
                return DyForeignFunction.Static(name, (c, args) => {
                    if (args is not null && args is DyTuple t)
                    {
                        var vs = t.Values.Select(v => v.SafeToString(ctx)).ToArray();
                        return new DyError(res, vs);
                    }
                    else
                        return new DyError(res);
                }, 0, new Par("values"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
