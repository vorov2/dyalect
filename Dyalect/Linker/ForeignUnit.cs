using Dyalect.Compiler;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;

namespace Dyalect.Linker
{
    public abstract class ForeignUnit : Unit
    {
        private bool initialized;
        private readonly Dictionary<Type, DyForeignTypeInfo> typeInfos = new();

        internal List<DyForeignTypeInfo> Types { get; }

        internal List<DyObject> Values { get; } = new();

        protected RuntimeContext RuntimeContext { get; private set; }

        protected ForeignUnit()
        {
            RuntimeContext = null!;
            Types = new();
            InitializeMembers();
            InitializeTypes();
            UnitIds.Add(0); //Self reference, to mimic the behavior of regular units
        }

        internal T GetTypeInfo<T>() where T : DyForeignTypeInfo => (T)typeInfos[typeof(T)];

        protected void Add(string name, DyObject obj)
        {
            ExportList.Add(name, new ScopeVar(0 | ExportList.Count << 8, VarFlags.Foreign));
            Values.Add(obj);
        }

        protected T AddType<T>() where T : DyForeignTypeInfo, new()
        {
            var t = new T();
            typeInfos.Add(typeof(T), t);
            Types.Add(t);
            Add(t.ReflectedTypeName, t);
            t.DeclaringUnit = this;
            return t;
        }

        protected Reference<T> AddReference<T>() where T : ForeignUnit
        {
            var ti = typeof(T);

            if (Attribute.GetCustomAttribute(ti, typeof(DyUnitAttribute)) is not DyUnitAttribute attr)
                throw new DyException("Invalid reference.");

            var asmName = ti.Assembly.GetName().Name + ".dll";
            var rf = new Reference(Guid.NewGuid(), attr.Name, null, asmName, default, null);
            UnitIds.Add(-1); //Real handles are added by a linker
            References.Add(rf);
            return new Reference<T>(rf);
        }

        public void Initialize(ExecutionContext ctx)
        {
            foreach (var t in Types)
            {
                t.SetReflectedTypeCode(ctx.RuntimeContext.Types.Count);
                ctx.RuntimeContext.Types.Add(t);
            }

            RuntimeContext = ctx.RuntimeContext;

            if (!initialized)
            {
                Execute(ctx);
                initialized = true;
            }
        }

        protected virtual void Execute(ExecutionContext ctx) { }

        protected virtual void InitializeTypes() { }

        protected virtual void InitializeMembers() { }

        protected static DyObject Nil = DyNil.Instance;
    }
}
