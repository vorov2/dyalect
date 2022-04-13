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

            var typeInfo = System.Type.GetType(type.GetString(), throwOnError: false);

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

        private DyObject CreateTypedArray(ExecutionContext ctx, DyObject arr, DyObject type)
        {
            if (arr is not DyCollection coll)
                return ctx.InvalidType(DyType.Array, DyType.Tuple, arr);

            if (type is not DyInteropObject interop || interop.Object is not System.Type typ)
                return ctx.InvalidType(DyType.Interop, type);

            if (!TypeConverter.TryCreateTypedArray(coll.GetValues(), typ, out var ret))
                return ctx.InvalidCast(arr.GetTypeName(ctx), typ.FullName ?? typ.Name);

            return new DyInteropObject(ret!.GetType(), ret!);
        }

        private System.Collections.Generic.Dictionary<string, DyInteropObject> types = new()
            {
                { "Int32", new DyInteropObject(typeof(int), typeof(int).GetType()) },
                { "Int64", new DyInteropObject(typeof(long), typeof(long).GetType()) },
                { "UInt32", new DyInteropObject(typeof(uint), typeof(uint).GetType()) },
                { "UInt64", new DyInteropObject(typeof(ulong), typeof(uint).GetType()) },
                { "Byte", new DyInteropObject(typeof(byte), typeof(byte).GetType()) },
                { "SByte", new DyInteropObject(typeof(sbyte), typeof(sbyte).GetType()) },
                { "Char", new DyInteropObject(typeof(char), typeof(char).GetType()) },
                { "String", new DyInteropObject(typeof(string), typeof(string).GetType()) },
                { "Boolean", new DyInteropObject(typeof(bool), typeof(bool).GetType()) },
                { "Double", new DyInteropObject(typeof(double), typeof(double).GetType()) },
                { "Single", new DyInteropObject(typeof(float), typeof(float).GetType()) },
                { "Decimal", new DyInteropObject(typeof(decimal), typeof(decimal).GetType()) },
        };
        private DyObject GetTypeInstance(ExecutionContext ctx, DyObject name)
        {
            if (!name.IsString(ctx)) return Default();

            if (!types.TryGetValue(name.GetString(), out var typ))
                return ctx.InvalidValue(name);

            return typ;
        }

        private DyObject Convert(ExecutionContext ctx, DyObject value, DyObject type)
        {
            if (type is not DyInteropSpecificObjectTypeInfo obj)
                return ctx.InvalidType(type);

            var ret = TypeConverter.ConvertTo(ctx, value, obj.Type);

            if (ret is null)
                return Default();

            return new DyInteropObject(ret.GetType(), ret);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("type")),
                "CreateTypedArray" => Func.Static(name, CreateTypedArray, -1, new Par("arr"), new Par("type")),
                "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("assembly")),
                "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
                "GetSystemType" => Func.Static(name, GetTypeInstance, -1, new Par("name")),
                "Convert" => Func.Static(name, Convert, -1, new Par("value"), new Par("type")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            new DyInteropFunction(name, ((DyInteropObject)self).Type, BindingFlags.Public | BindingFlags.Instance);
    }

    internal sealed class DyInteropSpecificObjectTypeInfo : DyTypeInfo
    {
        internal readonly Type Type;

        public override string TypeName => Type.Name;
 
        public override int ReflectedTypeId => (int)Type.TypeHandle.Value;

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

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;
    }
}
