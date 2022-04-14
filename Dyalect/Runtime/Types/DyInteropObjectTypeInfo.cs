using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Linq;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    internal class DyInteropObjectTypeInfo : DyTypeInfo
    {
        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Interop;

        public override int ReflectedTypeId => DyType.Interop;

        protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
            new DyString(arg.ToString()!);

        internal override void SetStaticMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        internal override void SetInstanceMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        private DyObject CreateInteropObject(ExecutionContext ctx, DyObject type)
        {
            if (!type.IsString(ctx)) return Default();

            var typeInfo = Type.GetType(type.GetString(), throwOnError: false);

            if (typeInfo is null)
                return ctx.InvalidValue(type);

            return new DyInteropSpecificObjectTypeInfo(typeInfo);
        }

        private DyObject LoadAssembly(ExecutionContext ctx, DyObject assembly)
        {
            if (!assembly.IsString(ctx)) return Default();
            Assembly.Load(assembly.GetString());
            return DyNil.Instance;
        }

        private DyObject LoadAssemblyFromFile(ExecutionContext ctx, DyObject path)
        {
            if (!path.IsString(ctx)) return Default();
            Assembly.LoadFrom(path.GetString());
            return DyNil.Instance;
        }

        private System.Collections.Generic.Dictionary<string, DyInteropSpecificObjectTypeInfo> types = new()
            {
                { "Int32", new DyInteropSpecificObjectTypeInfo(BCL.Int32) },
                { "Int64", new DyInteropSpecificObjectTypeInfo(BCL.Int64) },
                { "UInt32", new DyInteropSpecificObjectTypeInfo(BCL.UInt32) },
                { "UInt64", new DyInteropSpecificObjectTypeInfo(BCL.UInt64) },
                { "Byte", new DyInteropSpecificObjectTypeInfo(BCL.Byte) },
                { "SByte", new DyInteropSpecificObjectTypeInfo(BCL.SByte) },
                { "Char", new DyInteropSpecificObjectTypeInfo(BCL.Char) },
                { "String", new DyInteropSpecificObjectTypeInfo(BCL.String) },
                { "Boolean", new DyInteropSpecificObjectTypeInfo(BCL.Boolean) },
                { "Double", new DyInteropSpecificObjectTypeInfo(BCL.Double) },
                { "Single", new DyInteropSpecificObjectTypeInfo(BCL.Single) },
                { "Decimal", new DyInteropSpecificObjectTypeInfo(BCL.Decimal) },
                { "Type", new DyInteropSpecificObjectTypeInfo(BCL.Type) },
                { "Array", new DyInteropSpecificObjectTypeInfo(BCL.Array) },
        };
        private DyFunction? GetTypeInstance(ExecutionContext ctx, string name)
        {
            if (!types.TryGetValue(name, out var typ))
                return base.InitializeStaticMember(name, ctx);

            return Func.Auto(name, _ => typ);
        }

        private DyObject CreateArray(ExecutionContext ctx, DyObject type, DyObject size)
        {
            if (type is not DyInteropSpecificObjectTypeInfo obj)
                return ctx.InvalidType(type);

            if (!size.IsInteger(ctx)) return Default();
            var arr = Array.CreateInstance(obj.Type, (int)size.GetInteger());
            return new DyInteropObject(arr.GetType(), arr);
        }

        private DyObject ConvertTo(ExecutionContext ctx, DyObject type, DyObject value)
        {
            if (type is not DyInteropObject obj || obj.Object is not Type typ)
                return ctx.InvalidType(type);

            var ret = TypeConverter.ConvertTo(ctx, value, typ);

            if (ret is null)
                return Default();

            return new DyInteropObject(typ, ret);
        }

        private DyObject ConvertFrom(ExecutionContext ctx, DyObject obj)
        {
            if (obj is not DyInteropObject interop)
                return obj;

            return TypeConverter.ConvertFrom(interop.Object) ?? obj;
        }

        internal override DyObject GetStaticMember(string name, ExecutionContext ctx)
        {
            var func = name switch
            {
                "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("type")),
                "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("assembly")),
                "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
                "ConvertTo" => Func.Static(name, ConvertTo, -1, new Par("type"), new Par("value")),
                "ConvertFrom" => Func.Static(name, ConvertFrom, -1, new Par("value")),
                "CreateArray" => Func.Static(name, CreateArray, -1, new Par("type"), new Par("size")),
                _ => GetTypeInstance(ctx, name)
            };

            if (func is null)
                return ctx.StaticOperationNotSupported(name, ReflectedTypeId);

            if (func.Auto)
                return func.BindOrRun(ctx, this);

            return func;
        }

        internal override DyObject GetInstanceMember(DyObject self, string name, ExecutionContext ctx)
        {
            var func = GetInteropFunction(self, name, ctx);

            if (func is not null)
                return func.BindOrRun(ctx, self);

            return ctx.OperationNotSupported(name, self);
        }

        private DyFunction? GetInteropFunction(DyObject self, string name, ExecutionContext ctx)
        {
            var type = ((DyInteropObject)self).Type;
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var methods = type.GetOverloadedMethod(name, flags);
            var auto = false;

            if (methods is null)
            {
                name = Builtins.Getter(name);
                (methods, auto) = (type.GetOverloadedMethod(name, flags), true);
            }

            if (methods is null)
                return base.InitializeInstanceMember(self, name, ctx);

            return new DyInteropFunction(name, type, methods, auto);
        }
    }

    internal sealed class DyInteropSpecificObjectTypeInfo : DyTypeInfo
    {
        internal readonly Type Type;

        public override string TypeName => Type.Name;
 
        public override int ReflectedTypeId => (int)Type.TypeHandle.Value;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        internal override void SetStaticMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        internal override void SetInstanceMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        public DyInteropSpecificObjectTypeInfo(Type type) => Type = type;

        public override object ToObject() => Type;

        private DyObject CreateNew(ExecutionContext ctx, DyObject args)
        {
            var values = ((DyTuple)args).UnsafeAccessValues();
            var arr = values.Select(o => o.ToObject()).ToArray();
            object instance;

            try
            {
                instance = Activator.CreateInstance(Type, arr)!;
                return new DyInteropObject(Type, instance);
            }
            catch (Exception)
            {
                return ctx.MethodNotFound("new", Type, values);
            }
        }

        private DyFunction? GetInteropFunction(string name, ExecutionContext ctx)
        {
            var flags = BindingFlags.Public | BindingFlags.Static;
            var methods = Type.GetOverloadedMethod(name, flags);
            var auto = false;

            if (methods is null)
            {
                name = Builtins.Getter(name);
                (methods, auto) = (Type.GetOverloadedMethod(name, flags), true);
            }

            if (methods is null)
                return null;

            return new DyInteropFunction(name, Type, methods, auto);
        }

        internal override DyObject GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "new")
                return Func.Static(name, CreateNew, 0, new Par("args"));

            var func = GetInteropFunction(name, ctx);

            if (func is null)
                return ctx.StaticOperationNotSupported(name, ReflectedTypeId);

            if (func.Auto)
                return func.BindOrRun(ctx, this);

            return func;
        }
    }
}
