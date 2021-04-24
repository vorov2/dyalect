using Dyalect.Debug;
using Dyalect.Strings;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public class DyError : DyObject
    {
        internal DyError(DyErrorCode code, params (string, object)[] dataItems) : base(DyType.Object)
        {
            Code = code;
            DataItems = dataItems;
        }

        internal Stack<StackPoint> Dump { get; set; }

        public DyErrorCode Code { get; }

        public (string Key, object Value)[] DataItems { get; }

        public virtual string GetDescription()
        {
            var key = Code.ToString();
            var sb = new StringBuilder(RuntimeErrors.ResourceManager.GetString(key));

            if (DataItems != null)
                foreach (var (Key, Value) in DataItems)
                    sb.Replace("%" + Key + "%", (Value ?? "N/A").ToString());

            return sb.ToString();
        }

        internal virtual DyObject GetDetail() => new DyString(GetDescription());

        public override object ToObject() => GetDescription();

        public override string ToString() => Code.ToString() + ": " + GetDescription();

        protected internal override DyObject GetItem(string name, ExecutionContext ctx)
        {
            if (!TryGetItem(name, ctx, out var value))
                return ctx.IndexOutOfRange(name);

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
        public DyUserError(DyObject data) : base(DyErrorCode.UserCode)
        {
            Data = data;
        }

        public DyObject Data { get; }

        public override string GetDescription() => Data.ToObject()?.ToString();

        internal override DyObject GetDetail() => Data ?? DyNil.Instance;
    }
}
