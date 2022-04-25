using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Dyalect.Runtime.Types;

internal class DyInteropObjectTypeInfo : DyTypeInfo
{
    private const BindingFlags BINDING_FLAGS = BindingFlags.NonPublic | BindingFlags.Public 
        | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string ReflectedTypeName => nameof(DyType.Interop);

    public override int ReflectedTypeId => DyType.Interop;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(arg.ToString()!);

    internal override void SetStaticMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

    internal override void SetInstanceMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

    private DyObject CreateInteropObject(ExecutionContext ctx, DyObject type)
    {
        if (!type.IsString(ctx)) return Nil;

        var typeInfo = Type.GetType(type.GetString(), throwOnError: false);

        if (typeInfo is null)
            return ctx.InvalidValue(type);

        return new DyInteropObject(typeInfo);
    }

    private DyObject LoadAssembly(ExecutionContext ctx, DyObject assembly)
    {
        if (!assembly.IsString(ctx)) return Nil;
        Assembly.Load(assembly.GetString());
        return DyNil.Instance;
    }

    private DyObject LoadAssemblyFromFile(ExecutionContext ctx, DyObject path)
    {
        if (!path.IsString(ctx)) return Nil;
        Assembly.LoadFrom(path.GetString());
        return DyNil.Instance;
    }

    private readonly Dictionary<string, DyInteropObject> types = new()
        {
            { "Int32", new DyInteropObject(BCL.Int32) },
            { "Int64", new DyInteropObject(BCL.Int64) },
            { "UInt32", new DyInteropObject(BCL.UInt32) },
            { "UInt64", new DyInteropObject(BCL.UInt64) },
            { "Byte", new DyInteropObject(BCL.Byte) },
            { "SByte", new DyInteropObject(BCL.SByte) },
            { "Char", new DyInteropObject(BCL.Char) },
            { "String", new DyInteropObject(BCL.String) },
            { "Boolean", new DyInteropObject(BCL.Boolean) },
            { "Double", new DyInteropObject(BCL.Double) },
            { "Single", new DyInteropObject(BCL.Single) },
            { "Decimal", new DyInteropObject(BCL.Decimal) },
            { "Type", new DyInteropObject(BCL.Type) },
            { "Array", new DyInteropObject(BCL.Array) },
    };
    private DyFunction? GetTypeInstance(ExecutionContext ctx, string name)
    {
        if (!types.TryGetValue(name, out var typ))
            return base.InitializeStaticMember(name, ctx);

        return Func.Auto(name, _ => typ);
    }

    private DyObject CreateArray(ExecutionContext ctx, DyObject type, DyObject size)
    {
        if (type is not DyInteropObject obj || obj.Object is not Type t)
            return ctx.InvalidType(type);

        if (!size.IsInteger(ctx)) return Nil;
        var arr = Array.CreateInstance(t, (int)size.GetInteger());
        return new DyInteropObject(arr.GetType(), arr);
    }

    private DyObject ConvertTo(ExecutionContext ctx, DyObject type, DyObject value)
    {
        if (type is not DyInteropObject obj || obj.Object is not Type typ)
            return ctx.InvalidType(type);

        var ret = TypeConverter.ConvertTo(ctx, value, typ);

        if (ret is null)
            return Nil;

        return new DyInteropObject(typ, ret);
    }

    private DyObject ConvertFrom(ExecutionContext ctx, DyObject obj)
    {
        if (obj is not DyInteropObject interop)
            return obj;

        return TypeConverter.ConvertFrom(interop.Object) ?? obj;
    }
    private DyObject GetSystemType(ExecutionContext ctx, DyObject type)
    {
        if (type is DyInteropObject obj)
            return new DyInteropObject(BCL.Type, obj.Type);
        else if (type.TypeId != DyType.String)
            return ctx.InvalidType(DyType.Interop, DyType.String, type);

        var typeInfo = Type.GetType(type.GetString(), throwOnError: false);

        if (typeInfo is null)
            return ctx.InvalidValue(type);

        return new DyInteropObject(BCL.Type, typeInfo);
    }

    private DyObject GetMethod(ExecutionContext ctx, DyObject type, DyObject name, DyObject parTypes, DyObject genericParsCount)
    {
        if (type is not DyInteropObject obj || obj.Object is not Type typ)
            return ctx.InvalidType(type);

        if (parTypes is not DyCollection coll)
            coll = null!;

        if (parTypes.NotNil() && coll is null)
            return ctx.InvalidType(parTypes);

        if (!name.IsString(ctx)) return Nil;
        if (!genericParsCount.IsInteger(ctx)) return Nil;

        var types = coll?.GetValues();
        var (nam, p) = (name.GetString(), genericParsCount.GetInteger());

        foreach (var mi in typ.GetMethods(BINDING_FLAGS))
        {
            if (mi.Name == nam && mi.GetGenericArguments().Length == p)
            {
                if (types is not null)
                {
                    var mpars = mi.GetParameters();

                    if (types.Length != mpars.Length || MatchParameters(mpars, types))
                        continue;
                }

                return new DyInteropObject(typeof(MethodInfo), mi);
            }
        }

        return ctx.MethodNotFound(nam, typ, types);
    }

    private DyObject GetField(ExecutionContext ctx, DyObject type, DyObject name)
    {
        if (type is not DyInteropObject obj || obj.Object is not Type typ)
            return ctx.InvalidType(type);

        if (!name.IsString(ctx)) return Nil;
        var ret = typ.GetField(name.GetString());
        return ret is not null ? new DyInteropObject(typeof(FieldInfo), ret) : DyNil.Instance;
    }

    private bool MatchParameters(ParameterInfo[] mpars, DyObject[] types)
    {
        for (var i = 0; i < mpars.Length; i++)
        {
            var t = types[i].ToObject();

            if (t is not Type tt || mpars[i].ParameterType.IsAssignableFrom(tt))
                return false;
        }

        return true;
    }

    private DyObject Wrap(ExecutionContext ctx, DyObject obj) => new DyInteropObject(obj.GetType(), obj);

    internal override DyObject GetStaticMember(HashString nameStr, ExecutionContext ctx)
    {
        var name = (string)nameStr;
        if (!StaticMembers.TryGetValue(name, out var func))
        {
            func = LookupStatic(name, ctx);

            if (func is not null)
                StaticMembers.Add(name, func);
        }

        if (func is null)
            return ctx.StaticOperationNotSupported(name, ReflectedTypeId);

        if (func.Auto)
            return func.BindOrRun(ctx, this);
        
        return func;
    }

    private DyFunction? LookupStatic(string name, ExecutionContext ctx) =>
        name switch
        {
            "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("typeName")),
            "GetType" => Func.Static(name, GetSystemType, -1, new Par("typeName")),
            "Wrap" => Func.Static(name, Wrap, -1, new Par("value")),
            "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("name")),
            "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
            "ConvertTo" => Func.Static(name, ConvertTo, -1, new Par("type"), new Par("value")),
            "ConvertFrom" => Func.Static(name, ConvertFrom, -1, new Par("value")),
            "CreateArray" => Func.Static(name, CreateArray, -1, new Par("type"), new Par("size")),
            "GetMethod" => Func.Static(name, GetMethod, -1, new Par("type"), new Par("name"), new Par("parameterTypes", DyNil.Instance), new Par("typeArguments", DyInteger.Zero)),
            "GetField" => Func.Static(name, GetField, -1, new Par("type"), new Par("name")),
            _ => GetTypeInstance(ctx, name)
        };

    private DyObject CreateNew(ExecutionContext ctx, DyObject self, DyObject args)
    {
        var interop = (DyInteropObject)self;
        var values = ((DyTuple)args).UnsafeAccessValues();
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
            func = LookupInstance(name, interop, ctx);

            if (func is not null)
                Members.Add(name, func);
        }
        
        if (func is not null)
            return func.BindOrRun(ctx, self);

        return ctx.OperationNotSupported(name, self);
    }

    private DyFunction? LookupInstance(string name, DyInteropObject self, ExecutionContext ctx) =>
        name switch
        {
            "new" => Func.Member(name, CreateNew, 0, new Par("args")),
            _ => GetInteropFunction(self, name, ctx)
        };

    private DyFunction? GetInteropFunction(DyInteropObject self, string name, ExecutionContext _)
    {
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
}
