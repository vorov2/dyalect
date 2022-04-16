using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using Dyalect.Strings;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Linker
{
    public abstract class ForeignUnit : Unit
    {
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
            Add(t.TypeName, t);
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
            Execute(ctx);
        }

        protected virtual void Execute(ExecutionContext ctx) { }

        protected virtual void InitializeTypes() { }

        protected virtual void InitializeMembers()
        {
            var methods = GetType().GetMethods();

            foreach (var mi in methods)
            {
                var attr = Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute));

                if (attr is not null)
                {
                    var name = attr.ToString() ?? mi.Name;
                    var obj = ProcessMethod(name, mi);
                    Add(name, obj);
                }
            }
        }

        private DyObject ProcessMethod(string name, MethodInfo mi)
        {
            var pars = mi.GetParameters();
            var hasContext = pars.Length > 0 && pars[0].ParameterType == typeof(ExecutionContext);
            Par[] parsMeta = null!;

            if (hasContext && pars.Length > 1)
                parsMeta = new Par[pars.Length - 1];
            else if (!hasContext && pars.Length > 0)
                parsMeta = new Par[pars.Length];
            else if (pars.Length == 0)
                parsMeta = Array.Empty<Par>();
            
            var varArgIndex = -1;
            var simpleSignature = pars.Length > 0 && pars[0].ParameterType == typeof(ExecutionContext);

            for (var i = 0; i < pars.Length; i++)
            {
                if (i == 0 && hasContext)
                    continue;

                var p = pars[i];

                if (p.ParameterType != BCL.DyObject)
                    simpleSignature = false;

                var va = false;

                if (Attribute.IsDefined(p, typeof(VarArgAttribute)))
                {
                    if (varArgIndex != -1)
                        throw new DyException(LinkerErrors.DuplicateVarArgs.Format(mi.Name));

                    va = true;
                    varArgIndex = i - 1;
                }

                DyObject? def = null;

                if (Attribute.GetCustomAttribute(p, typeof(DefaultAttribute)) is DefaultAttribute defAttr)
                    def = defAttr.Value;
                else if (p.HasDefaultValue)
                    def = TypeConverter.ConvertFrom(p.DefaultValue!, p.ParameterType);

                parsMeta[i - (hasContext ? 1 : 0)] = new Par(p.Name!, def, va);
            }

            if (simpleSignature)
            {
                if (parsMeta == null)
                    return Func.Static(name, (Func<ExecutionContext, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject>), this));

                if (parsMeta.Length == 1)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 2)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 3)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 4)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 5)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 6)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 7)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 8)
                    return Func.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>) mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                throw new DyException(LinkerErrors.TooManyParameters.Format(mi.Name));
            }
            else
            {
                var (fun, types) = CreateDelegate(name, mi, pars, this);
                return new ForeignFunction(name, new() { Func = fun, Types = types }, parsMeta, varArgIndex, hasContext);
            }
        }

        private (Delegate,Type[]) CreateDelegate(string name, MethodInfo self, ParameterInfo[] pars, object instance)
        {
            var types = new Type[pars.Length + 1];

            for (var i = 0; i < pars.Length; i++)
                types[i] = pars[i].ParameterType;

            types[^1] = self.ReturnType;

            var func = types.Length switch
            {
                1  => typeof(Func<>),
                2  => typeof(Func<,>),
                3  => typeof(Func<,,>),
                4  => typeof(Func<,,,>),
                5  => typeof(Func<,,,,>),
                6  => typeof(Func<,,,,,>),
                7  => typeof(Func<,,,,,,>),
                8  => typeof(Func<,,,,,,,>),
                9  => typeof(Func<,,,,,,,,>),
                10 => typeof(Func<,,,,,,,,,>),
                11 => typeof(Func<,,,,,,,,,,>),
                12 => typeof(Func<,,,,,,,,,,,>),
                13 => typeof(Func<,,,,,,,,,,,,>),
                14 => typeof(Func<,,,,,,,,,,,,,>),
                15 => typeof(Func<,,,,,,,,,,,,,,>),
                16 => typeof(Func<,,,,,,,,,,,,,,,>),
                17 => typeof(Func<,,,,,,,,,,,,,,,,>),
                _  => throw new Exception("Method not supported. Too many arguments.")
            };

            var dt = func.MakeGenericType(types);
            return (self.CreateDelegate(dt, instance), types);
        }

        protected DyObject Default() => DyNil.Instance;
    }
}
