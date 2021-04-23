using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static class ErrorGenerators
    {
        public static DyObject Fail(this ExecutionContext ctx, string detail)
        {
            ctx.Error = new DyUserError(new DyString(detail));
            return DyNil.Instance;
        }

        public static DyObject CollectionModified(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.CollectionModified);
            return DyNil.Instance;
        }

        public static DyObject OpenRangeNotSupported(this ExecutionContext ctx, string typeName)
        {
            ctx.Error = new(DyErrorCode.OpenRangeNotSupported,
                ("Type", typeName));
            return DyNil.Instance;
        }

        public static DyObject FailedReadLiteral(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.FailedReadLiteral,
                ("Reason", reason));
            return DyNil.Instance;
        }

        public static DyObject AssertFailed(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.AssertFailed,
                ("Reason", reason));
            return DyNil.Instance;
        }

        public static DyObject StaticOperationNotSupported(this ExecutionContext ctx, string op, string typeName)
        {
            ctx.Error = new(DyErrorCode.StaticOperationNotSupported,
                ("Operation", op),
                ("TypeName", typeName));
            return DyNil.Instance;
        }

        public static DyObject PrivateNameAccess(this ExecutionContext ctx, DyObject obj)
        {
            ctx.Error = new(DyErrorCode.PrivateNameAccess,
                ("Name", obj.ToString()));
            return DyNil.Instance;
        }

        public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, DyObject obj)
        {
            ctx.Error = new(DyErrorCode.OperationNotSupported,
                ("Operation", op),
                ("TypeName", obj.GetTypeName(ctx)));
            return DyNil.Instance;
        }

        public static DyObject IndexOutOfRange(this ExecutionContext ctx, object index)
        {
            ctx.Error = new(DyErrorCode.IndexOutOfRange,
                ("Index", index));
            return DyNil.Instance;
        }

        public static DyObject ValueOutOfRange(this ExecutionContext ctx, object value)
        {
            ctx.Error = new(DyErrorCode.ValueOutOfRange,
                ("Value", value));
            return DyNil.Instance;
        }

        public static DyObject IndexInvalidType(this ExecutionContext ctx, DyObject index)
        {
            ctx.Error = new(DyErrorCode.IndexInvalidType,
                ("Index", index),
                ("IndexTypeName", index.GetTypeName(ctx)));
            return DyNil.Instance;
        }

        public static DyObject KeyNotFound(this ExecutionContext ctx, DyObject key)
        {
            ctx.Error = new(DyErrorCode.KeyNotFound, ("Key", key));
            return DyNil.Instance;
        }

        public static DyObject KeyAlreadyPresent(this ExecutionContext ctx, DyObject key)
        {
            ctx.Error = new(DyErrorCode.KeyAlreadyPresent, ("Key", key));
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, DyObject value)
        {
            ctx.Error = new(DyErrorCode.InvalidType,
                ("TypeName", value.GetTypeName(ctx)));
            return DyNil.Instance;
        }

        public static DyObject InvalidValue(this ExecutionContext ctx, DyObject value)
        {
            ctx.Error = new(DyErrorCode.InvalidValue,
                ("Value", value.ToString(ctx)));
            return DyNil.Instance;
        }

        public static DyObject InvalidRange(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidRange);
            return DyNil.Instance;
        }

        public static DyObject ExternalFunctionFailure(this ExecutionContext ctx, string functionName, string error)
        {
            ctx.Error = new(DyErrorCode.ExternalFunctionFailure,
                ("FunctionName", functionName),
                ("Error", error));
            return DyNil.Instance;
        }

        public static DyObject DivideByZero(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.DivideByZero);
            return DyNil.Instance;
        }

        public static DyObject TooManyArguments(this ExecutionContext ctx, string functionName, int functionArguments, int passedArguments)
        {
            ctx.Error = new(DyErrorCode.TooManyArguments,
                ("FunctionName", functionName),
                ("FunctionArguments", functionArguments),
                ("PassedArguments", passedArguments));
            return DyNil.Instance;
        }

        public static DyObject RequiredArgumentMissing(this ExecutionContext ctx, string functionName, string argumentName)
        {
            ctx.Error = new(DyErrorCode.RequiredArgumentMissing,
                ("FunctionName", functionName),
                ("ArgumentName", argumentName));
            return DyNil.Instance;
        }

        public static DyObject ArgumentNotFound(this ExecutionContext ctx, string functionName, string argumentName)
        {
            ctx.Error = new(DyErrorCode.ArgumentNotFound,
                ("FunctionName", functionName),
                ("ArgumentName", argumentName));
            return DyNil.Instance;
        }
    }
}
