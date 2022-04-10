using Dyalect.Debug;
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

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "Interop" => Func.Static(name, CreateInteropObject, -1, new Par("type")),
                "LoadAssembly" => Func.Static(name, LoadAssembly, -1, new Par("assembly")),
                "LoadAssemblyFromFile" => Func.Static(name, LoadAssemblyFromFile, -1, new Par("path")),
                _ => base.InitializeStaticMember(name, ctx)
            };

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            new DyInteropFunction(name, ((DyInteropObject)self).Type, BindingFlags.Public | BindingFlags.Instance);
    }

    internal sealed class DyInteropSpecificObjectTypeInfo : DyTypeInfo
    {
        private readonly System.Type type;

        public override string TypeName => type.Name;

        public override int ReflectedTypeId => (int)type.TypeHandle.Value;

        internal override void SetStaticMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        internal override void SetInstanceMember(ExecutionContext ctx, string name, DyFunction func) => ctx.InvalidOperation();

        public DyInteropSpecificObjectTypeInfo(System.Type type) => this.type = type;

        private DyObject CreateNew(ExecutionContext ctx, DyObject args)
        {
            return new DyInteropObject(type, System.Activator.CreateInstance(type, 
                ((DyTuple)args).UnsafeAccessValues().Select(o => o.ToObject()).ToArray())!);
        }

        protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
            name switch
            {
                "New" => Func.Static(name, CreateNew, 0, new Par("args")),
                _ => new DyInteropFunction(name, type, BindingFlags.Public | BindingFlags.Static)
            };

        protected override SupportedOperations GetSupportedOperations() => SupportedOperations.None;
    }
}
