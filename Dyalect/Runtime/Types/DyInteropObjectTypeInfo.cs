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

        private System.Collections.Generic.Dictionary<string, DyInteropObject> types = new()
            {
                { "Int32", new DyInteropObject(typeof(int).GetType(), typeof(int)) },
                { "Int64", new DyInteropObject(typeof(long).GetType(), typeof(long)) },
                { "UInt32", new DyInteropObject(typeof(uint).GetType(), typeof(uint)) },
                { "UInt64", new DyInteropObject(typeof(uint).GetType(), typeof(ulong)) },
                { "Byte", new DyInteropObject(typeof(byte).GetType(), typeof(byte)) },
                { "SByte", new DyInteropObject(typeof(sbyte).GetType(), typeof(sbyte)) },
                { "Char", new DyInteropObject(typeof(char).GetType(), typeof(char)) },
                { "String", new DyInteropObject(typeof(string).GetType(), typeof(string)) },
                { "Boolean", new DyInteropObject(typeof(bool).GetType(), typeof(bool)) },
                { "Double", new DyInteropObject(typeof(double).GetType(), typeof(double)) },
                { "Single", new DyInteropObject(typeof(float).GetType(), typeof(float)) },
                { "Decimal", new DyInteropObject(typeof(decimal).GetType(), typeof(decimal)) },
                { "Type", new DyInteropObject(typeof(Type).GetType(), typeof(Type)) },
                { "Array", new DyInteropObject(typeof(Array).GetType(), typeof(Array)) },
        };
        private DyFunction? GetTypeInstance(ExecutionContext ctx, string name)
        {
            if (!types.TryGetValue(name, out var typ))
                return base.InitializeStaticMember(name, ctx);

            return Func.Auto(name, _ => typ);
        }

        private DyObject CreateArray(ExecutionContext ctx, DyObject type, DyObject size)
        {
            if (type is not DyInteropObject obj || obj.Object is not Type typ)
                return ctx.InvalidType(type);

            if (!size.IsInteger(ctx)) return Default();
            var arr = Array.CreateInstance(typ, (int)size.GetInteger());
            return new DyInteropObject(arr.GetType(), arr);
        }

        private DyObject ConvertTo(ExecutionContext ctx, DyObject value, DyObject type)
        {
            if (type is not DyInteropObject obj || obj.Object is not Type typ)
                return ctx.InvalidType(type);

            var ret = TypeConverter.ConvertTo(ctx, value, typ);

            if (ret is null)
                return Default();

            return new DyInteropObject(ret.GetType(), ret);
        }

        private DyObject ConvertFrom(ExecutionContext ctx, DyObject obj)
        {
            if (obj is not DyInteropObject interop)
                return ctx.InvalidType(obj);

            return TypeConverter.ConvertFrom(interop.Object) ?? obj;
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("type")),
                "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("assembly")),
                "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
                "ConvertTo" => Func.Static(name, ConvertTo, -1, new Par("type"), new Par("value")),
                "ConvertFrom" => Func.Static(name, ConvertFrom, -1, new Par("value")),
                "CreateArray" => Func.Static(name, CreateArray, -1, new Par("type"), new Par("size")),
                _ => GetTypeInstance(ctx, name)
            };

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            new DyInteropFunction(name, ((DyInteropObject)self).Type, BindingFlags.Public | BindingFlags.Instance);
    }

    internal sealed class DyInteropSpecificObjectTypeInfo : DyTypeInfo
    {
        internal readonly Type Type;

        public override string TypeName => Type.Name;
 
        public override int ReflectedTypeId => (int)Type.TypeHandle.Value;

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;

        internal override void SetStaticMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        internal override void SetInstanceMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        public DyInteropSpecificObjectTypeInfo(Type type) => this.Type = type;

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

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "new" => Func.Static(name, CreateNew, 0, new Par("args")),
                _ => new DyInteropFunction(name, Type, BindingFlags.Public | BindingFlags.Static)
            };
    }
}
