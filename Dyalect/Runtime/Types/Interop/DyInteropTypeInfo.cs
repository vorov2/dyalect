using Dyalect.Codegen;
using Dyalect.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal partial class DyInteropTypeInfo : DyTypeInfo
{
    private static readonly DyInterop TypeInt32 = new(BCL.Int32);
    private static readonly DyInterop TypeInt64 = new(BCL.Int64);
    private static readonly DyInterop TypeUInt32 = new(BCL.UInt32);
    private static readonly DyInterop TypeUInt64 = new(BCL.UInt64);
    private static readonly DyInterop TypeByte = new(BCL.Byte);
    private static readonly DyInterop TypeSByte = new(BCL.SByte);
    private static readonly DyInterop TypeChar = new(BCL.Char);
    private static readonly DyInterop TypeString = new(BCL.String);
    private static readonly DyInterop TypeBoolean = new(BCL.Boolean);
    private static readonly DyInterop TypeDouble = new(BCL.Double);
    private static readonly DyInterop TypeSingle = new(BCL.Single);
    private static readonly DyInterop TypeSystemArray = new(BCL.Array);
    private static readonly DyInterop TypeSystemType = new(BCL.Type);

    private const BindingFlags AllBindingFlags = BindingFlags.NonPublic | BindingFlags.Public 
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public override string ReflectedTypeName => nameof(Dy.Interop);

    public override int ReflectedTypeId => Dy.Interop;

    #region Operations
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
        var interop = (DyInterop)self;
        var values = ((DyTuple)args).UnsafeAccess();
        var arr = values.Select(o => o.ToObject()).ToArray();
        object instance;
        var type = interop.Object as Type ?? interop.Type;

        try
        {
            instance = Activator.CreateInstance(type, arr)!;
            return new DyInterop(instance.GetType(), instance);
        }
        catch (Exception ex)
        {
            return ctx.ConstructorFailed(arr, type, ex);
        }
    }

    internal override DyObject GetInstanceMember(DyObject self, HashString nameStr, ExecutionContext ctx)
    {
        var interop = (DyInterop)self;
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

    private DyFunction? GetInteropFunction(DyInterop self, string name, ExecutionContext _)
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

        return new DyInterop(typeInfo);
    }

    [StaticMethod(Method.GetType)]
    internal static DyObject GetSystemType(ExecutionContext ctx, DyObject typeName)
    {
        if (typeName is DyInterop obj)
            return new DyInterop(BCL.Type, obj.Type);
        else if (typeName.TypeId is not Dy.String or Dy.Char)
            throw new DyCodeException(DyError.InvalidType, typeName);

        var str = typeName.ToString();
        var key = nameof(DyInteropTypeInfo) + "_x235_" + typeName.ToString();

        if (!ctx.TryGetContextVariable(key, out var ret))
        {
            var typeInfo = Type.GetType(str, throwOnError: false);

            if (typeInfo is null)
                return ctx.InvalidValue(typeName);

            ret = new DyInterop(BCL.Type, typeInfo);
            ctx.SetContextVariable(key, ret);
        }

        return (DyObject)ret!;
    }

    [StaticMethod]
    internal static DyObject Wrap(DyObject value) => new DyInterop(value.GetType(), value);

    [StaticMethod]
    internal static void LoadAssembly(string name) => Assembly.Load(name);

    [StaticMethod]
    internal static void LoadAssemblyFromFile(string path) => Assembly.LoadFrom(path);

    [StaticMethod]
    internal static DyObject ConvertTo(ExecutionContext ctx, DyInterop type, DyObject value)
    {
        if (type.Object is not Type typ)
            throw new DyCodeException(DyError.InvalidType, type);

        var ret = TypeConverter.ConvertTo(ctx, value, typ);

        if (ret is null)
            return Nil;

        return new DyInterop(typ, ret);
    }

    [StaticMethod]
    internal static DyObject ConvertFrom(DyObject value)
    {
        if (value is not DyInterop interop)
            return value;

        return TypeConverter.ConvertFrom(interop.Object) ?? value;
    }

    [StaticMethod]
    internal static DyObject CreateArray(ExecutionContext ctx, DyInterop type, int size)
    {
        if (type.Object is not Type t)
            throw new DyCodeException(DyError.InvalidType, type);

        var arr = Array.CreateInstance(t, size);
        return new DyInterop(arr.GetType(), arr);
    }

    [StaticMethod]
    internal static DyObject GetMethod(ExecutionContext ctx, DyInterop type, string name, DyObject[]? parameterTypes = null, int typeArguments = 0)
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

                return new DyInterop(typeof(MethodInfo), mi);
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
    internal static DyObject GetField(ExecutionContext ctx, DyInterop type, string name)
    {
        if (type.Object is not Type typ)
            throw new DyCodeException(DyError.InvalidType, type);

        var ret = typ.GetField(name);
        return ret is not null ? new DyInterop(typeof(FieldInfo), ret) : Nil;
    }

    [StaticProperty]
    internal static DyObject Int32() => TypeInt32;

    [StaticProperty]
    internal static DyObject Int64() => TypeInt64;

    [StaticProperty]
    internal static DyObject UInt32() => TypeUInt32;

    [StaticProperty]
    internal static DyObject UInt64() => TypeUInt64;

    [StaticProperty]
    internal static DyObject Byte() => TypeByte;

    [StaticProperty]
    internal static DyObject SByte() => TypeSByte;

    [StaticProperty]
    internal static DyObject Char() => TypeChar;

    [StaticProperty]
    internal static DyObject String() => TypeString;

    [StaticProperty]
    internal static DyObject Boolean() => TypeBoolean;

    [StaticProperty]
    internal static DyObject Double() => TypeDouble;

    [StaticProperty]
    internal static DyObject Single() => TypeSingle;
    
    [StaticProperty(Method.Array)]
    internal static DyObject SystemArray() => TypeSystemArray;

    [StaticProperty(Method.Type)]
    internal static DyObject SystemType() => TypeSystemType;
}
