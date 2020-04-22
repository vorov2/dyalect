using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System;
using Dyalect.Runtime;
using System.Reflection;
using Dyalect.Debug;
using Dyalect.Strings;

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
            var parsMeta = pars.Length > 1 ? new Par[pars.Length - 1] : null;
            var varArgIndex = -1;
            var simpleSignature = true;
            var expectContext = true;

            if (pars.Length == 0 || pars[0].ParameterType != typeof(ExecutionContext))
                simpleSignature = false;

            for (var i = 0; i < pars.Length; i++)
            {
                var p = pars[i];

                if (p.ParameterType != Dyalect.Types.DyObject)
                    expectContext = simpleSignature = false;
                else
                    continue; 

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
                    def = TypeConverter.ConvertFrom(p.DefaultValue, p.ParameterType, ExecutionContext.Default);

                parsMeta[i - (expectContext ? 1 : 0)] = new Par(p.Name, def, va);
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
                return new ForeignFunction(name, new FunctionDescriptor { Func = fun, Types = types }, parsMeta, varArgIndex, expectContext);
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
