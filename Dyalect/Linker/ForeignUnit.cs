using Dyalect.Compiler;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using System;
using Dyalect.Runtime;
using System.Reflection;

namespace Dyalect.Linker
{
    public abstract class ForeignUnit : Unit
    {
        internal List<DyObject> Values { get; } = new List<DyObject>();

        protected ForeignUnit()
        {
            Initialize();
            InitializeMethods();
        }

        internal protected void Add(string name, DyObject obj)
        {
            ExportList.Add(new PublishedName(name, new ScopeVar(0 | ExportList.Count << 8, VarFlags.Foreign)));
            Values.Add(obj);
        }

        internal void Modify(int id, DyObject obj)
        {
            Values[id] = obj;
        }

        protected virtual void Initialize()
        {

        }

        public virtual void Execute(ExecutionContext ctx)
        {

        }

        private void InitializeMethods()
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

            if (pars.Length == 2
                && pars[0].ParameterType == typeof(ExecutionContext)
                && pars[1].ParameterType == typeof(DyObject[]))
                return DyForeignFunction.Create(
                    name,
                    (Func<ExecutionContext, DyObject[], DyObject>)mi.CreateDelegate(typeof(Func<ExecutionContext, DyObject[], DyObject>), this));

            Func<ExecutionContext, DyObject[], DyObject> invoker = (ctx, args) =>
            {
                var newArgs = new object[args.Length];

                for (var i = 0; i < args.Length; i++)
                {
                    var p = pars[i];
                    var res = TypeConverter.ConvertTo(args[0], p.ParameterType, ctx);

                    if (ctx.HasErrors)
                        return DyNil.Instance;

                    newArgs[i] = res;
                }

                var retval = mi.Invoke(this, newArgs);
                return TypeConverter.ConvertFrom(retval, ctx);
            };
            return DyForeignFunction.Create(name, invoker);
        }

        protected DyObject Default() => DyNil.Instance;
    }
}
