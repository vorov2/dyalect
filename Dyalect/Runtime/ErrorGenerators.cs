using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static class ErrorGenerators
    {
        public static DyObject Fail(this ExecutionContext ctx, string detail)
        {
            ctx.Error = new(DyErrorCode.UnexpectedError, new DyString(detail));
            return DyNil.Instance;
        }

        public static DyObject CollectionModified(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.CollectionModified);
            return DyNil.Instance;
        }

        public static DyObject FailedReadLiteral(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.FailedReadLiteral, reason);
            return DyNil.Instance;
        }

        public static DyObject AssertFailed(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.AssertFailed, reason);
            return DyNil.Instance;
        }

        public static DyObject PrivateNameAccess(this ExecutionContext ctx, string name)
        {
            ctx.Error = new(DyErrorCode.PrivateNameAccess, name);
            return DyNil.Instance;
        }

        public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, string typeName)
        {
            ctx.Error = new(DyErrorCode.OperationNotSupported, op, typeName);
            return DyNil.Instance;
        }

        public static DyObject IndexOutOfRange(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.IndexOutOfRange);
            return DyNil.Instance;
        }

        public static DyObject KeyNotFound(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.KeyNotFound);
            return DyNil.Instance;
        }

        public static DyObject KeyAlreadyPresent(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.KeyAlreadyPresent);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, DyObject value)
        {
            ctx.Error = new(DyErrorCode.InvalidType, value.GetTypeName(ctx));
            return DyNil.Instance;
        }

        public static DyObject ExternalFunctionFailure(this ExecutionContext ctx, string functionName, string error)
        {
            ctx.Error = new(DyErrorCode.ExternalFunctionFailure, functionName, error);
            return DyNil.Instance;
        }

        public static DyObject DivideByZero(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.DivideByZero);
            return DyNil.Instance;
        }

        public static DyObject TooManyArguments(this ExecutionContext ctx, string functionName, int functionArguments, int passedArguments)
        {
            ctx.Error = new(DyErrorCode.TooManyArguments, functionName, functionArguments, passedArguments);
            return DyNil.Instance;
        }

        public static DyObject RequiredArgumentMissing(this ExecutionContext ctx, string functionName, string argumentName)
        {
            ctx.Error = new(DyErrorCode.RequiredArgumentMissing, functionName, argumentName);
            return DyNil.Instance;
        }

        public static DyObject ArgumentNotFound(this ExecutionContext ctx, string functionName, string argumentName)
        {
            ctx.Error = new(DyErrorCode.ArgumentNotFound, functionName, argumentName);
            return DyNil.Instance;
        }
    }
}
