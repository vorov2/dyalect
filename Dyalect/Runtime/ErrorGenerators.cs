using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System.Linq;
using System.Text;
namespace Dyalect.Runtime;

public static class ErrorGenerators
{
    public static DyObject CustomError(this ExecutionContext ctx, string constructor)
    {
        ctx.Error = new(constructor);
        return DyNil.Instance;
    }

    public static DyObject Failure(this ExecutionContext ctx, string detail)
    {
        ctx.Error = new(DyError.Failure, detail);
        return DyNil.Instance;
    }

    public static DyObject IOFailed(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.IOFailed);
        return DyNil.Instance;
    }

    public static DyObject TypeClosed(this ExecutionContext ctx, DyTypeInfo typeInfo)
    {
        ctx.Error = new(DyError.TypeClosed, typeInfo.ReflectedTypeName);
        return DyNil.Instance;
    }

    public static DyObject Overflow(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.Overflow);
        return DyNil.Instance;
    }

    public static DyObject InvalidOperation(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.InvalidOperation);
        return DyNil.Instance;
    }

    public static DyObject NotImplemented(this ExecutionContext ctx, string op)
    {
        ctx.Error = new(DyError.NotImplemented, Builtins.Translate(op));
        return DyNil.Instance;
    }

    public static DyObject ParsingFailed(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.ParsingFailed);
        return DyNil.Instance;
    }

    public static DyObject ParsingFailed(this ExecutionContext ctx, string detail)
    {
        ctx.Error = new(DyError.ParsingFailed, detail);
        return DyNil.Instance;
    }

    public static DyObject Timeout(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.Timeout);
        return DyNil.Instance;
    }

    public static DyObject ValueMissing(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.ValueMissing);
        return DyNil.Instance;
    }

    public static DyObject InvalidOverload(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.InvalidOverload);
        return DyNil.Instance;
    }

    public static DyObject InvalidOverload(this ExecutionContext ctx, object func)
    {
        ctx.Error = new(DyError.InvalidOverload, func);
        return DyNil.Instance;
    }

    public static DyObject ConstructorFailed(this ExecutionContext ctx, object[]? args, Type type, Exception ex)
    {
        var sb = new StringBuilder();
        sb.Append("new(");
        ProcessArguments(sb, args);
        sb.Append(')');
        ctx.Error = new(DyError.ConstructorFailed, sb.ToString(), type.FullName ?? type.Name, ex.Message);
        return DyNil.Instance;
    }

    public static DyObject InvalidValue(this ExecutionContext ctx, object val1)
    {
        ctx.Error = new(DyError.InvalidValue, val1);
        return DyNil.Instance;
    }

    public static DyObject InvalidValue(this ExecutionContext ctx, object val1, object val2)
    {
        ctx.Error = new(DyError.InvalidValue, val1, val2);
        return DyNil.Instance;
    }

    public static DyObject InvalidValue(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.InvalidValue);
        return DyNil.Instance;
    }

    public static DyObject PrivateAccess(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.PrivateAccess);
        return DyNil.Instance;
    }

    public static DyObject IndexReadOnly(this ExecutionContext ctx, object obj)
    {
        ctx.Error = new(DyError.IndexReadOnly, obj);
        return DyNil.Instance;
    }

    public static DyObject IndexReadOnly(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.IndexReadOnly);
        return DyNil.Instance;
    }

    public static DyObject MultipleValuesForArgument(this ExecutionContext ctx, string funName, string argName)
    {
        ctx.Error = new(DyError.MultipleValuesForArgument, funName, argName);
        return DyNil.Instance;
    }

    public static DyObject CollectionModified(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.CollectionModified);
        return DyNil.Instance;
    }

    public static DyObject AssertionFailed(this ExecutionContext ctx, string reason)
    {
        ctx.Error = new(DyError.AssertionFailed, reason);
        return DyNil.Instance;
    }

    public static DyObject PrivateNameAccess(this ExecutionContext ctx, string name)
    {
        ctx.Error = new(DyError.PrivateNameAccess, name);
        return DyNil.Instance;
    }

    public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, DyObject obj)
    {
        ctx.Error = new(DyError.OperationNotSupported, Builtins.Translate(op), obj.TypeName);
        return DyNil.Instance;
    }

    public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, int typeId)
    {
        var typeName = ctx.RuntimeContext.Types[typeId].ReflectedTypeName;
        ctx.Error = new(DyError.OperationNotSupported, Builtins.Translate(op), typeName, 0, 0);
        return DyNil.Instance;
    }

    public static DyObject StaticOperationNotSupported(this ExecutionContext ctx, string op, int typeId)
    {
        var typeName = ctx.RuntimeContext.Types[typeId].ReflectedTypeName;
        //small hack to get OperationNotSupported.4
        ctx.Error = new(DyError.OperationNotSupported, Builtins.Translate(op), typeName, 0, 0);
        return DyNil.Instance;
    }

    public static DyObject OperationNotSupported(this ExecutionContext ctx, string op, DyObject obj1, DyObject obj2)
    {
        ctx.Error = new(DyError.OperationNotSupported, Builtins.Translate(op), obj1.TypeName, obj2.TypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidCast(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.InvalidCast);
        return DyNil.Instance;
    }

    public static DyObject InvalidCast(this ExecutionContext ctx, string type1, string type2)
    {
        ctx.Error = new(DyError.InvalidCast, type1, type2);
        return DyNil.Instance;
    }

    public static DyObject IndexOutOfRange(this ExecutionContext ctx, object obj)
    {
        ctx.Error = new(DyError.IndexOutOfRange, obj);
        return DyNil.Instance;
    }

    public static DyObject IndexOutOfRange(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.IndexOutOfRange);
        return DyNil.Instance;
    }

    public static DyObject KeyNotFound(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.KeyNotFound);
        return DyNil.Instance;
    }

    public static DyObject KeyNotFound(this ExecutionContext ctx, object key)
    {
        ctx.Error = new(DyError.KeyNotFound, key);
        return DyNil.Instance;
    }

    public static DyObject KeyAlreadyPresent(this ExecutionContext ctx, object key)
    {
        ctx.Error = new(DyError.KeyAlreadyPresent, key);
        return DyNil.Instance;
    }

    public static DyObject KeyAlreadyPresent(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.KeyAlreadyPresent);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.InvalidType);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, DyObject value)
    {
        ctx.Error = new(DyError.InvalidType, value.TypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, int expected, DyObject got)
    {
        ctx.Error = new(DyError.InvalidType, ctx.RuntimeContext.Types[expected].ReflectedTypeName, got.TypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, DyObject got)
    {
        ctx.Error = new(DyError.InvalidType, ctx.RuntimeContext.Types[expected1].ReflectedTypeName, ctx.RuntimeContext.Types[exptected2].ReflectedTypeName, ctx.RuntimeContext.Types[got.TypeId].ReflectedTypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, int expected3, DyObject got)
    {
        ctx.Error = new(DyError.InvalidType, ctx.RuntimeContext.Types[expected1].ReflectedTypeName, ctx.RuntimeContext.Types[exptected2].ReflectedTypeName,
            ctx.RuntimeContext.Types[expected3].ReflectedTypeName, got.TypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, int expected1, int exptected2, int expected3, int expected4, DyObject got)
    {
        ctx.Error = new(DyError.InvalidType, ctx.RuntimeContext.Types[expected1].ReflectedTypeName, ctx.RuntimeContext.Types[exptected2].ReflectedTypeName,
            ctx.RuntimeContext.Types[expected3].ReflectedTypeName, ctx.RuntimeContext.Types[expected4].ReflectedTypeName, got.TypeName);
        return DyNil.Instance;
    }

    public static DyObject InvalidType(this ExecutionContext ctx, string typeName)
    {
        ctx.Error = new(DyError.InvalidType, typeName);
        return DyNil.Instance;
    }

    public static DyObject ExternalFunctionFailure(this ExecutionContext ctx, DyFunction func, string error)
    {
        var functionName = func.Self is null ? func.FunctionName
            : $"{func.Self.TypeName}.{func.FunctionName}";
        ctx.Error = new(DyError.ExternalFunctionFailure, functionName, error);
        return DyNil.Instance;
    }

    public static DyObject DivideByZero(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.DivideByZero);
        return DyNil.Instance;
    }

    public static DyObject TooManyArguments(this ExecutionContext ctx)
    {
        ctx.Error = new(DyError.TooManyArguments);
        return DyNil.Instance;
    }

    public static DyObject TooManyArguments(this ExecutionContext ctx, string functionName, int functionArguments, int passedArguments)
    {
        ctx.Error = new(DyError.TooManyArguments, functionName, functionArguments, passedArguments);
        return DyNil.Instance;
    }

    public static DyObject RequiredArgumentMissing(this ExecutionContext ctx, string functionName, string argumentName)
    {
        ctx.Error = new(DyError.RequiredArgumentMissing, functionName, argumentName);
        return DyNil.Instance;
    }

    public static DyObject ArgumentNotFound(this ExecutionContext ctx, string functionName, string argumentName)
    {
        ctx.Error = new(DyError.ArgumentNotFound, functionName, argumentName);
        return DyNil.Instance;
    }

    public static DyObject MethodNotFound(this ExecutionContext ctx, string name, Type type, DyObject[]? args)
    {
        var sb = new StringBuilder();
        sb.Append(type.FullName ?? type.Name);
        sb.Append('.');
        sb.Append(name);
        sb.Append('(');
        ProcessArguments(sb, args);
        sb.Append(')');
        ctx.Error = new(DyError.MethodNotFound, sb.ToString());
        return DyNil.Instance;
    }

    public static DyError GetErrorCode(DyVariant err)
    {
        if (!Enum.TryParse<DyError>(err.Constructor, true, out var res))
            return DyError.UnexpectedError;

        return res;
    }

    public static string GetErrorDescription(DyVariant err)
    {
        if (!Enum.TryParse<DyError>(err.Constructor, true, out var res))
        {
            if (err.Tuple.Count > 0)
                return err.Tuple[0].ToString() ?? err.Constructor;

            return err.Constructor;
        }

        var idx = err.Tuple.Count;
        var str = RuntimeErrors.ResourceManager.GetString(err.Constructor + "." + idx);

        if (str is not null && err.Tuple.Count > 0)
        {
            var vals = err.Tuple.ToArray()
                .Select(v => v is DyTypeInfo t ? t.ReflectedTypeName : (v.ToString() ?? ""))
                .ToArray();
            str = string.Format(str, vals);
        }
        else if (str is null)
            str = RuntimeErrors.ResourceManager.GetString(err.Constructor + ".0");

        return str ?? err.Constructor;
    }

    private static void ProcessArguments(StringBuilder sb, object[]? args)
    {
        if (args is not null)
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                    sb.Append(',');

                var tt = (args[i] is DyObject obj ? obj.ToObject() : args[i])?.GetType();

                if (tt is null)
                    sb.Append("<null>");
                else
                    sb.Append(tt.FullName ?? tt.Name);
            }
    }
}
