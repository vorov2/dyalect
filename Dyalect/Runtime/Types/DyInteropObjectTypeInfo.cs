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

        internal override void SetStaticMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

        internal override void SetInstanceMember(ExecutionContext ctx, HashString name, DyFunction func) => ctx.InvalidOperation();

        private DyObject CreateInteropObject(ExecutionContext ctx, DyObject type)
        {
            if (!type.IsString(ctx)) return Default();

            var typeInfo = Type.GetType(type.GetString(), throwOnError: false);

            if (typeInfo is null)
                return ctx.InvalidValue(type);

            return new DyInteropObject(typeInfo);
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

        private readonly System.Collections.Generic.Dictionary<string, DyInteropObject> types = new()
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

            if (!size.IsInteger(ctx)) return Default();
            var arr = Array.CreateInstance(t, (int)size.GetInteger());
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

        private DyObject GetGenericMethod(ExecutionContext ctx, DyObject type, DyObject name, DyObject pars, DyObject typeArgs)
        {
            if (type is not DyInteropObject obj || obj.Object is not Type typ)
                return ctx.InvalidType(type);

            if (!name.IsString(ctx)) return Default();
            if (!pars.IsInteger(ctx)) return Default();

            var types = ((DyTuple)typeArgs).UnsafeAccessValues();
            var (nam, p) = (name.GetString(), pars.GetInteger());

            foreach (var mi in typ.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (mi.Name == nam && mi.GetGenericArguments().Length == p)
                {
                    if (types.Length > 0)
                    {
                        var mpars = mi.GetParameters();

                        for (var i = 0; i < mpars.Length; i++)
                        {
                            var rf = types.Length > i ? types[i] : null;

                            if (rf is null)
                                continue;

                            var t = rf.ToObject();

                            if (t is not Type tt || mpars[i].ParameterType.IsAssignableFrom(tt))
                                continue;
                        }
                    }

                    return new DyInteropObject(typeof(MethodInfo), mi);
                }
            }

            return DyNil.Instance;
        }

        private DyObject Wrap(ExecutionContext ctx, DyObject obj) =>
            new DyInteropObject(obj.GetType(), obj);

        internal override DyObject GetStaticMember(HashString nameStr, ExecutionContext ctx)
        {
            var name = (string)nameStr;
            var func = name switch
            {
                "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("type")),
                "GetType" => Func.Static(name, GetSystemType, -1, new Par("type")),
                "Wrap" => Func.Static(name, Wrap, -1, new Par("value")),
                "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("assembly")),
                "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
                "ConvertTo" => Func.Static(name, ConvertTo, -1, new Par("type"), new Par("value")),
                "ConvertFrom" => Func.Static(name, ConvertFrom, -1, new Par("value")),
                "CreateArray" => Func.Static(name, CreateArray, -1, new Par("type"), new Par("size")),
                "GetGenericMethod" => Func.Static(name, GetGenericMethod, 3, new Par("type"), new Par("name"), new Par("count"), new Par("types", true)),
                _ => GetTypeInstance(ctx, name)
            };

            if (func is null)
                return ctx.StaticOperationNotSupported(name, ReflectedTypeId);

            if (func.Auto)
                return func.BindOrRun(ctx, this);

            return func;
        }

        private DyObject CreateNew(ExecutionContext ctx, DyObject self, DyObject args)
        {
            var interop = (DyInteropObject)self;
            var values = ((DyTuple)args).UnsafeAccessValues();
            var arr = values.Select(o => o.ToObject()).ToArray();
            object instance;

            try
            {
                instance = Activator.CreateInstance(interop.Object as Type ?? interop.Type, arr)!;
                return new DyInteropObject(instance.GetType(), instance);
            }
            catch (Exception)
            {
                return ctx.MethodNotFound("new", interop.Type, values);
            }
        }

        internal override DyObject GetInstanceMember(DyObject self, HashString nameStr, ExecutionContext ctx)
        {
            var interop = (DyInteropObject)self;
            DyFunction? func = null;
            var name = (string)nameStr;

            if (name == "new")
                func = Func.Member(name, CreateNew, 0, new Par("args"));
            else
                func = GetInteropFunction(interop, name, ctx);
            
            if (func is not null)
                return func.BindOrRun(ctx, self);

            return ctx.OperationNotSupported(name, self);
        }

        private DyFunction? GetInteropFunction(DyInteropObject self, string name, ExecutionContext ctx)
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
                return base.InitializeInstanceMember(self, name, ctx);

            return new DyInteropFunction(name, type, methods, auto);
        }
    }
}
