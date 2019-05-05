using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Dyalect.Runtime
{
    public enum DyErrorCode
    {
        None,

        UserCode = 601,

        NotFunction = 602,

        ExternalFunctionFailure = 603,

        OperationNotSupported = 604,

        IndexOutOfRange = 605,

        IndexInvalidType = 606,

        DivideByZero = 607,

        WrongNumberOfArguments = 608,

        InvalidType = 609
    }

    public sealed class DyError
    {
        internal DyError(DyErrorCode code, params (string, object)[] dataItems)
        {
            Code = code;
            DataItems = new ReadOnlyCollection<(string, object)>(dataItems);
        }

        public DyErrorCode Code { get; }

        public IReadOnlyList<(string Key, object Value)> DataItems { get; }

        public string GetDescription()
        {
            var sb = new StringBuilder(RuntimeErrors.ResourceManager.GetString(Code.ToString()));

            foreach (var dt in DataItems)
                sb.Replace("%" + dt.Key + "%", dt.Value.ToString());

            return sb.ToString();
        }
    }

    internal static class Err
    {
        public static DyError OperationNotSupported(string op, string typeName)
        {
            return new DyError(DyErrorCode.OperationNotSupported,
                ("Operation", op),
                ("TypeName", typeName));
        }

        public static DyError OperationNotSupported(string op, string typeName1, string typeName2)
        {
            return new DyError(DyErrorCode.OperationNotSupported,
                ("Operation", op),
                ("TypeName", "(" + typeName1 + "," + typeName2 + ")"));
        }

        public static DyError IndexOutOfRange(string typeName, object index)
        {
            return new DyError(DyErrorCode.IndexOutOfRange,
                ("TypeName", typeName),
                ("Index", index));
        }

        public static DyError IndexInvalidType(string typeName, string indexTypeName)
        {
            return new DyError(DyErrorCode.IndexInvalidType,
                ("TypeName", typeName),
                ("IndexTypeName", indexTypeName));
        }

        public static DyError InvalidType(string expectedTypeName, string gotTypeName)
        {
            return new DyError(DyErrorCode.InvalidType,
                ("Expected", expectedTypeName),
                ("Got", gotTypeName));
        }

        public static DyError ExternalFunctionFailure(string functionName, string error)
        {
            return new DyError(DyErrorCode.ExternalFunctionFailure,
                ("FunctionName", functionName),
                ("Error", error));
        }

        public static DyError UserCode(string error)
        {
            return new DyError(DyErrorCode.UserCode, ("Error", error));
        }

        public static DyError NotFunction(string typeName)
        {
            return new DyError(DyErrorCode.NotFunction, ("TypeName", typeName));
        }

        public static DyError DivideByZero()
        {
            return new DyError(DyErrorCode.DivideByZero);
        }

        public static DyError WrongNumberOfArguments(string functionName, int functionArguments, int passedArguments)
        {
            return new DyError(DyErrorCode.WrongNumberOfArguments,
                ("FunctionName", functionName),
                ("FunctionArguments", functionArguments),
                ("PassedArguments", passedArguments));
        }
    }

    internal static class DyErrorExtensions
    {
        public static DyObject Set(this DyError err, ExecutionContext ctx)
        {
            ctx.Error = err;
            return DyNil.Instance;
        }
    }
}
