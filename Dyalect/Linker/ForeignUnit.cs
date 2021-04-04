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
        internal List<DyObject> Values { get; } = new List<DyObject>();

        protected ForeignUnit()
        {
            InitializeMembers();
        }

        internal protected void Add(string name, DyObject obj)
        {
            ExportList.Add(name, new ScopeVar(0 | ExportList.Count << 8, VarFlags.Foreign));
            Values.Add(obj);
        }

        internal protected void AddType(string name, Func<int, DyTypeInfo> typeActivator)
        {
            var typeId = Types.Count;
            var td = new TypeDescriptor(name, typeId, true, typeActivator);
            Types.Add(td);
            TypeMap[name] = td;
        }

        internal void Modify(int id, DyObject obj)
        {
            Values[id] = obj;
        }

        public virtual void Execute(ExecutionContext ctx)
        {

        }

        protected virtual void InitializeMembers()
        {
            var methods = GetType().GetMethods();

            foreach (var mi in methods)
            {
                var attr = Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute));

                if (attr != null)
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
            Par[] parsMeta = null;

            if (hasContext && pars.Length > 1)
                parsMeta = new Par[pars.Length - 1];
            else if (!hasContext && pars.Length > 0)
                parsMeta = new Par[pars.Length];
            else if (pars.Length == 0)
                parsMeta = Statics.EmptyParameters;
            
            var varArgIndex = -1;
            var simpleSignature = true;

            if (pars.Length == 0 || pars[0].ParameterType != typeof(ExecutionContext))
                simpleSignature = false;

            for (var i = 0; i < pars.Length; i++)
            {
                if (i == 0 && hasContext)
                    continue;

                var p = pars[i];

                if (p.ParameterType != Dyalect.Types.DyObject)
                    simpleSignature = false;

                var va = false;

                if (Attribute.IsDefined(p, typeof(VarArgAttribute)))
                {
                    if (varArgIndex != -1)
                        throw new DyException(LinkerErrors.DuplicateVarArgs.Format(mi.Name));

                    va = true;
                    varArgIndex = i - 1;
                }

                DyObject def = null;

                if (Attribute.GetCustomAttribute(p, typeof(DefaultAttribute)) is DefaultAttribute defAttr)
                    def = defAttr.Value;
                else if (p.HasDefaultValue)
                    def = TypeConverter.ConvertFrom(p.DefaultValue, p.ParameterType);

                parsMeta[i - (hasContext ? 1 : 0)] = new Par(p.Name, def, va);
            }

            if (simpleSignature)
            {
                if (parsMeta == null)
                    return DyForeignFunction.Static(name, (Func<ExecutionContext, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject>), this));

                if (parsMeta.Length == 1)
                    return DyForeignFunction.Static(name, (Func<ExecutionContext, DyObject, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 2)
                    return DyForeignFunction.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 3)
                    return DyForeignFunction.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                if (parsMeta.Length == 4)
                    return DyForeignFunction.Static(name, (Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject>), this), varArgIndex, parsMeta);

                throw new DyException(LinkerErrors.TooManyParameters.Format(mi.Name));
            }
            else
            {
                var (fun, types) = CreateDelegate(mi, pars, this);
                return new ForeignFunction(name, new FunctionDescriptor { Func = fun, Types = types }, parsMeta, varArgIndex, hasContext);
            }
        }

        private (Delegate,Type[]) CreateDelegate(MethodInfo self, ParameterInfo[] pars, object instance)
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
