using Dyalect.Runtime.Types;

namespace Dyalect.Runtime
{
    public static class ErrorGenerators
    {
        public static DyObject Fail(this ExecutionContext ctx, string detail)
        {
            ctx.Error = new(DyErrorCode.UnexpectedError, detail);
            return DyNil.Instance;
        }

        public static DyObject InvalidOverload(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidOverload);
            return DyNil.Instance;
        }

        public static DyObject InvalidOverload(this ExecutionContext ctx, object func)
        {
            ctx.Error = new(DyErrorCode.InvalidOverload, func);
            return DyNil.Instance;
        }

        public static DyObject InvalidValue(this ExecutionContext ctx, object val1)
        {
            ctx.Error = new(DyErrorCode.InvalidValue, val1);
            return DyNil.Instance;
        }

        public static DyObject InvalidValue(this ExecutionContext ctx, object val1, object val2)
        {
            ctx.Error = new(DyErrorCode.InvalidValue, val1, val2);
            return DyNil.Instance;
        }

        public static DyObject InvalidValue(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidValue);
            return DyNil.Instance;
        }

        public static DyObject PrivateAccess(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.PrivateAccess);
            return DyNil.Instance;
        }

        public static DyObject IndexReadOnly(this ExecutionContext ctx, object obj)
        {
            ctx.Error = new(DyErrorCode.IndexReadOnly, obj);
            return DyNil.Instance;
        }

        public static DyObject IndexReadOnly(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.IndexReadOnly);
            return DyNil.Instance;
        }

        public static DyObject MultipleValuesForArgument(this ExecutionContext ctx, string funName, string argName)
        {
            ctx.Error = new(DyErrorCode.MultipleValuesForArgument, funName, argName);
            return DyNil.Instance;
        }

        public static DyObject CollectionModified(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.CollectionModified);
            return DyNil.Instance;
        }

        public static DyObject FormatException(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.FormatException, reason);
            return DyNil.Instance;
        }

        public static DyObject AssertionFailed(this ExecutionContext ctx, string reason)
        {
            ctx.Error = new(DyErrorCode.AssertionFailed, reason);
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

        public static DyObject InvalidCast(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidCast);
            return DyNil.Instance;
        }

        public static DyObject InvalidCast(this ExecutionContext ctx, string type1, string type2)
        {
            ctx.Error = new(DyErrorCode.InvalidCast, type1, type2);
            return DyNil.Instance;
        }

        public static DyObject IndexOutOfRange(this ExecutionContext ctx, object obj)
        {
            ctx.Error = new(DyErrorCode.IndexOutOfRange, obj);
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

        public static DyObject KeyNotFound(this ExecutionContext ctx, object key)
        {
            ctx.Error = new(DyErrorCode.KeyNotFound, key);
            return DyNil.Instance;
        }

        public static DyObject KeyAlreadyPresent(this ExecutionContext ctx, object key)
        {
            ctx.Error = new(DyErrorCode.KeyAlreadyPresent, key);
            return DyNil.Instance;
        }

        public static DyObject KeyAlreadyPresent(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.KeyAlreadyPresent);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidType);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, DyObject value)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[value.TypeId].TypeName);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected].TypeName, ctx.RuntimeContext.Types[got.TypeId].TypeName);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected1].TypeName, ctx.RuntimeContext.Types[exptected2].TypeName, ctx.RuntimeContext.Types[got.TypeId].TypeName);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, int expected3, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected1].TypeName, ctx.RuntimeContext.Types[exptected2].TypeName,
                ctx.RuntimeContext.Types[expected3].TypeName, ctx.RuntimeContext.Types[got.TypeId].TypeName);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, int expected3, int expected4, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected1].TypeName, ctx.RuntimeContext.Types[exptected2].TypeName,
                ctx.RuntimeContext.Types[expected3].TypeName, ctx.RuntimeContext.Types[expected4].TypeName, ctx.RuntimeContext.Types[got.TypeId].TypeName);
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, string typeName)
        {
            ctx.Error = new(DyErrorCode.InvalidType, typeName);
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
