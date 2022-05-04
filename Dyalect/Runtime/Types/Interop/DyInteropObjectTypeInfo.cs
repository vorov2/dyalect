using Dyalect.Codegen;
using Dyalect.Compiler;
using System.Linq;
using System.Reflection;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal partial class DyInteropObjectTypeInfo : DyTypeInfo
{
    private const BindingFlags AllBindingFlags = BindingFlags.NonPublic | BindingFlags.Public 
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public override string ReflectedTypeName => nameof(Dy.Interop);

    public override int ReflectedTypeId => Dy.Interop;

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(arg.ToString()!);

    internal override void SetStaticMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

    internal override void SetInstanceMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

    internal override DyObject GetStaticMember(HashString nameStr, ExecutionContext ctx)
    {
        var name = (string)nameStr;
        if (!StaticMembers.TryGetValue(name, out var func))
        {
            func = InitializeStaticMember(name, ctx);

            if (func is not null)
                StaticMembers.Add(name, func);
        }

        if (func is null)
            return ctx.StaticOperationNotSupported(name, ReflectedTypeId);

        if (func.Auto)
            return func.TryInvokeProperty(ctx, this);

        return func;
    }

    private DyObject CreateNew(ExecutionContext ctx, DyObject self, DyObject args)
    {
        var interop = (DyInteropObject)self;
        var values = ((DyTuple)args).UnsafeAccess();
        var arr = values.Select(o => o.ToObject()).ToArray();
        object instance;
        var type = interop.Object as Type ?? interop.Type;

        try
        {
            instance = Activator.CreateInstance(type, arr)!;
            return new DyInteropObject(instance.GetType(), instance);
        }
        catch (Exception ex)
        {
            return ctx.ConstructorFailed(arr, type, ex);
        }
    }

    internal override DyObject GetInstanceMember(DyObject self, HashString nameStr, ExecutionContext ctx)
    {
        var interop = (DyInteropObject)self;
        var name = (string)nameStr;

        if (!Members.TryGetValue(name, out var func))
        {
            func = GetInteropFunction(interop, name, ctx);

            if (func is not null)
                Members.Add(name, func);
        }

        if (func is not null)
            return func.TryInvokeProperty(ctx, self);

        return ctx.OperationNotSupported(name, self);
    }

    private DyFunction? GetInteropFunction(DyInteropObject self, string name, ExecutionContext _)
    {
        if (name == "new")
            return new DyForeignConstructor(CreateNew);

        var type = self.Type;
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        var methods = type.GetOverloadedMethod(name, flags);
        var auto = false;

        if (methods is null)
        {
            name = Builtins.Getter(name);
            (methods, auto) = (type.GetOverloadedMethod(name, flags), true);
        }

        if (methods is null)
            return null;

        return new DyInteropFunction(name, type, methods, auto);
    }
    #endregion

    [StaticMethod("Interop")]
    internal static DyObject CreateInteropObject(ExecutionContext ctx, string typeName)
    {
        var typeInfo = Type.GetType(typeName, throwOnError: false);

        if (typeInfo is null)
            return ctx.InvalidValue(typeName);

        return new DyInteropObject(typeInfo);
    }

    [StaticMethod("GetType")]
    internal static DyObject GetSystemType(ExecutionContext ctx, DyObject typeName)
    {
        if (typeName is DyInteropObject obj)
            return new DyInteropObject(BCL.Type, obj.Type);
        else if (typeName.TypeId is not Dy.String or Dy.Char)
            throw new DyCodeException(DyError.InvalidType, typeName);

        var typeInfo = Type.GetType(typeName.ToString(), throwOnError: false);

        if (typeInfo is null)
            return ctx.InvalidValue(typeName);

        return new DyInteropObject(BCL.Type, typeInfo);
    }

    [StaticMethod]
    internal static DyObject Wrap(DyObject value) => new DyInteropObject(value.GetType(), value);

    [StaticMethod]
    internal static void LoadAssembly(string name) => Assembly.Load(name);

    [StaticMethod]
    internal static void LoadAssemblyFromFile(string path) => Assembly.LoadFrom(path);

    [StaticMethod]
    internal static DyObject ConvertTo(ExecutionContext ctx, DyInteropObject type, DyObject value)
    {
        if (type.Object is not Type typ)
            throw new DyCodeException(DyError.InvalidType, type);

        var ret = TypeConverter.ConvertTo(ctx, value, typ);

        if (ret is null)
            return Nil;

        return new DyInteropObject(typ, ret);
    }

    [StaticMethod]
    internal static DyObject ConvertFrom(DyObject value)
    {
        if (value is not DyInteropObject interop)
            return value;

        return TypeConverter.ConvertFrom(interop.Object) ?? value;
    }

    [StaticMethod]
    internal static DyObject CreateArray(ExecutionContext ctx, DyInteropObject type, int size)
    {
        if (type.Object is not Type t)
            throw new DyCodeException(DyError.InvalidType, type);

        var arr = Array.CreateInstance(t, size);
        return new DyInteropObject(arr.GetType(), arr);
    }

    [StaticMethod]
    internal static DyObject GetMethod(ExecutionContext ctx, DyInteropObject type, string name, DyObject[]? parameterTypes = null, int typeArguments = 0)
    {
        if (type.Object is not Type typ)
            throw new DyCodeException(DyError.InvalidType, type);

        foreach (var mi in typ.GetMethods(AllBindingFlags))
        {
            if (mi.Name == name && mi.GetGenericArguments().Length == typeArguments)
            {
                if (parameterTypes is not null)
                {
                    var mpars = mi.GetParameters();

                    if (parameterTypes.Length != mpars.Length || MatchParameters(mpars, parameterTypes))
                        continue;
                }

                return new DyInteropObject(typeof(MethodInfo), mi);
            }
        }

        return ctx.MethodNotFound(name, typ, parameterTypes);

        static bool MatchParameters(ParameterInfo[] mpars, DyObject[] types)
        {
            for (var i = 0; i < mpars.Length; i++)
            {
                var t = types[i].ToObject();

                if (t is not Type tt || mpars[i].ParameterType.IsAssignableFrom(tt))
                    return false;
            }

            return true;
        }
    }

    [StaticMethod]
    internal static DyObject GetField(ExecutionContext ctx, DyInteropObject type, string name)
    {
        if (type.Object is not Type typ)
            throw new DyCodeException(DyError.InvalidType, type);

        var ret = typ.GetField(name);
        return ret is not null ? new DyInteropObject(typeof(FieldInfo), ret) : Nil;
    }

    [StaticProperty]
    internal static DyObject Int32() => new DyInteropObject(BCL.Int32);

    [StaticProperty]
    internal static DyObject Int64() => new DyInteropObject(BCL.Int64);

    [StaticProperty]
    internal static DyObject UInt32() => new DyInteropObject(BCL.UInt32);

    [StaticProperty]
    internal static DyObject UInt64() => new DyInteropObject(BCL.UInt64);

    [StaticProperty]
    internal static DyObject Byte() => new DyInteropObject(BCL.Byte);

    [StaticProperty]
    internal static DyObject SByte() => new DyInteropObject(BCL.SByte);

    [StaticProperty]
    internal static DyObject Char() => new DyInteropObject(BCL.Char);

    [StaticProperty]
    internal static DyObject String() => new DyInteropObject(BCL.String);

    [StaticProperty]
    internal static DyObject Boolean() => new DyInteropObject(BCL.Boolean);

    [StaticProperty]
    internal static DyObject Double() => new DyInteropObject(BCL.Double);

    [StaticProperty]
    internal static DyObject Single() => new DyInteropObject(BCL.Single);
    
    [StaticProperty("Array")]
    internal static DyObject SystemArray() => new DyInteropObject(BCL.Array);

    [StaticProperty("Type")]
    internal static DyObject SystemType() => new DyInteropObject(BCL.Type);
}
