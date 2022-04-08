using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Linq;

namespace Dyalect.Runtime
{
    public static class ErrorGenerators
    {
        public static DyObject CustomError(this ExecutionContext ctx, string constructor)
        {
            ctx.Error = new(constructor);
            return DyNil.Instance;
        }

        public static DyObject Failure(this ExecutionContext ctx, string detail)
        {
            ctx.Error = new(DyErrorCode.Failure, detail);
            return DyNil.Instance;
        }

        public static DyObject Overflow(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.Overflow);
            return DyNil.Instance;
        }

        public static DyObject InvalidOperation(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.InvalidOperation);
            return DyNil.Instance;
        }

        public static DyObject ParsingFailed(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.ParsingFailed);
            return DyNil.Instance;
        }

        public static DyObject ParsingFailed(this ExecutionContext ctx, string detail)
        {
            ctx.Error = new(DyErrorCode.ParsingFailed, detail);
            return DyNil.Instance;
        }

        public static DyObject Timeout(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.Timeout);
            return DyNil.Instance;
        }

        public static DyObject ValueMissing(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.ValueMissing);
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

        public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, DyObject obj)
        {
            ctx.Error = new(DyErrorCode.OperationNotSupported, Builtins.Translate(op), obj.GetTypeName(ctx));
            return DyNil.Instance;
        }

        public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, int typeId)
        {
            var typeName = ctx.RuntimeContext.Types[typeId].TypeName;
            ctx.Error = new(DyErrorCode.OperationNotSupported, Builtins.Translate(op), typeName, 0, 0);
            return DyNil.Instance;
        }

        public static DyObject StaticOperationNotSupported(this ExecutionContext ctx, string op, int typeId)
        {
            var typeName = ctx.RuntimeContext.Types[typeId].TypeName;
            //small hack to get OperationNotSupported.4
            ctx.Error = new(DyErrorCode.OperationNotSupported, Builtins.Translate(op), typeName, 0, 0);
            return DyNil.Instance;
        }

        public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, DyObject obj1, DyObject obj2)
        {
            ctx.Error = new(DyErrorCode.OperationNotSupported, Builtins.Translate(op),
                obj1.GetTypeName(ctx), obj2.GetTypeName(ctx));
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
            ctx.Error = new(DyErrorCode.InvalidType, value.GetTypeName(ctx));
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected].TypeName, got.GetTypeName(ctx));
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
                ctx.RuntimeContext.Types[expected3].TypeName, got.GetTypeName(ctx));
            return DyNil.Instance;
        }

        public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, int expected3, int expected4, DyObject got)
        {
            ctx.Error = new(DyErrorCode.InvalidType, ctx.RuntimeContext.Types[expected1].TypeName, ctx.RuntimeContext.Types[exptected2].TypeName,
                ctx.RuntimeContext.Types[expected3].TypeName, ctx.RuntimeContext.Types[expected4].TypeName, got.GetTypeName(ctx));
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

        public static DyObject TooManyArguments(this ExecutionContext ctx)
        {
            ctx.Error = new(DyErrorCode.TooManyArguments);
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

        public static DyErrorCode GetErrorCode(DyVariant err)
        {
            if (!Enum.TryParse<DyErrorCode>(err.Constructor, true, out var res))
                return DyErrorCode.UnexpectedError;

            return res;
        }

        public static string GetErrorDescription(DyVariant err)
        {
            if (!Enum.TryParse<DyErrorCode>(err.Constructor, true, out var res))
                return err.Constructor;

            var idx = err.Tuple.Count;
            var str = RuntimeErrors.ResourceManager.GetString(err.Constructor + "." + idx);

            if (str is not null && err.Tuple.Count > 0)
                {
                var vals = err.Tuple.GetValues()
                    .Select(v => v is DyTypeInfo t ? t.TypeName : (v.ToString() ?? ""))
                    .ToArray();
                str = str.Format(vals);
            }
            else if (str is null)
            {
                str = RuntimeErrors.ResourceManager.GetString(err.Constructor + ".0");
                str ??= err.Constructor;
            }

            return str;
        }
    }
}
