using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyInteropFunction : DyForeignFunction
    {
        private readonly static Type objectType = typeof(object);
        private readonly static Type intType = typeof(int);
        private readonly static Type longType = typeof(long);
        private readonly static Par[] pars = new Par[] { new Par("args", isVarArg: true) };
        private readonly string name;
        private readonly Type type;
        private readonly BindingFlags flags;
        private MethodInfo[]? methods;

        public DyInteropFunction(string name, Type type, BindingFlags flags) : base(name, pars, 0) =>
            (this.name, this.type, this.flags) = (name, type, flags);

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
        {
            if (methods is null)
                methods = type.GetMethods(flags);

            var tupleArgs = ((DyTuple)args[0]).UnsafeAccessValues();
            var arguments = tupleArgs.Select(a => a.ToObject()).ToArray();
            var argumentTypes = arguments.Select(a => a.GetType()).ToList();

            if (!ResolveMethod(arguments, argumentTypes, false, out var result))
                if (!ResolveMethod(arguments, argumentTypes, true, out result))
                    return ctx.MethodNotFound(name, type, tupleArgs);

            return result;
        }

        private bool ResolveMethod(object[] arguments, List<Type> argumentTypes, bool generalize, out DyObject result)
        {
            result = DyNil.Instance;

            foreach (var m in methods!)
            {
                if (m.Name == name)
                {
                    var pars = m.GetParameters();

                    if (pars.Length != arguments.Length || !CheckArguments(arguments, argumentTypes, generalize, pars))
                        continue;

                    var ret = m.Invoke(Self is null ? null : ((DyInteropObject)Self!).Object, arguments);

                    if (ret is null)
                        result = DyNil.Instance;

                    result = TypeConverter.ConvertFrom(ret);

                    if (result is DyNil && ret is not null)
                        result = new DyInteropObject(ret.GetType(), ret);

                    return true;
                }
            }

            return false;
        }

        private bool CheckArguments(object[] arguments, List<Type> argumentTypes, bool generalize, ParameterInfo[] pars)
        {
            Span<int> checks = stackalloc int[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                var (pt, at) = (pars[i].ParameterType, argumentTypes[i]);

                if (at == longType && pt == intType)
                    checks[i] = 1;
                else if (!at.Equals(pt) && (!generalize || !pt.IsAssignableFrom(at)))
                    return false;
            }

            for (var i = 0; i < checks.Length; i++)
                if (checks[i] == 1)
                    arguments[i] = (int)(long)arguments[i];

            return true;
        }

        protected override DyFunction Clone(ExecutionContext ctx) => new DyInteropFunction(name, type, flags);

        internal override bool Equals(DyFunction func) => func is DyInteropFunction f
            && f.name == name && f.type.Equals(type);
    }    
}
