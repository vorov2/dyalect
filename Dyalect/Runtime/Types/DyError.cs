using Dyalect.Debug;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dyalect.Runtime.Types
{
    public sealed class DyError : DyObject
    {
        private readonly string errorName;
        private readonly string? description;

        internal DyError(DyErrorCode code, params object[] dataItems) : base(DyType.Error) =>
            (Code, errorName, DataItems) = (code, code.ToString(), dataItems);

        internal DyError(string errorName, string? description, params object[] dataItems) : 
            this(errorName, DyErrorCode.UnexpectedError, description, dataItems) { }

        internal DyError(string errorName, DyErrorCode code, string? description, params object[] dataItems) : base(DyType.Error) =>
            (this.errorName, this.description, Code, DataItems) = (errorName, description, code, dataItems);
        
        internal Stack<StackPoint>? Dump { get; set; }

        public DyErrorCode Code { get; }

        public object[] DataItems { get; }

        public string GetDescription()
        {
            if (description is not null)
                return description;

            var idx = DataItems.Length;
            var str = RuntimeErrors.ResourceManager.GetString(errorName + "." + idx);

            if (str is not null)
            {
                if (DataItems is not null && DataItems.Length > 0)
                {
                    var arr = new string[DataItems.Length];

                    for (var i = 0; i < arr.Length; i++)
                    {
                        var it = DataItems[i];

                        if (it is DyTypeInfo dti)
                            arr[i] = dti.TypeName;
                        else
                            arr[i] = it.ToString() ?? "";
                    }

                    str = str.Format(arr);
                }

                return str;
            }
            else if (str is null)
            {
                str = RuntimeErrors.ResourceManager.GetString(errorName + ".0");

                if (str is not null)
                    return str;
            }

            return errorName;
        }

        internal DyObject GetDetail() => new DyString(GetDescription());

        public override object ToObject() => GetDescription();

        public override string ToString() => errorName + ": " + GetDescription();

        protected internal override DyObject GetItem(DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId != DyType.String)
                return ctx.IndexOutOfRange(index);

            var name = index.GetString();

            if (name is "code")
                return new DyInteger((int)Code);
            
            if (name is "detail")
                return GetDetail();

            if (name is "items")
                return new DyTuple(DataItems.Select(i => TypeConverter.ConvertFrom(i)).ToArray());
            
            return ctx.IndexOutOfRange(index);
        }

        protected internal override bool HasItem(string name, ExecutionContext ctx) => name is "code" or "detail" or "items";

        public override string GetConstructor(ExecutionContext ctx) => errorName;

        public override int GetHashCode() => HashCode.Combine(Code, DataItems);
    }
}
