using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyInteropFunction : DyForeignFunction
    {
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
            var realName = name.StartsWith("Get_") ? "get_" + name.Substring(4) : name;

            foreach (var m in methods)
            {
                if (m.Name == realName)
                {
                    var pars = m.GetParameters();

                    if (pars.Length != arguments.Length || !CheckArguments(arguments, argumentTypes, pars))
                        continue;

                    var retval = m.Invoke(Self is null ? null : ((DyInteropObject)Self!).Object, arguments);

                    if (retval is null)
                        return DyNil.Instance;

                    var conv = TypeConverter.ConvertFrom(retval);

                    if (conv is DyNil)
                        return new DyInteropObject(type, retval);

                    return conv;
                }
            }

            return ctx.MethodNotFound(realName, type, tupleArgs);
        }

        private bool CheckArguments(object[] arguments, List<Type> argumentTypes, ParameterInfo[] pars)
        {
            for (var i = 0; i < arguments.Length; i++)
                if (!argumentTypes[0].Equals(pars[0].ParameterType))
                    return false;

            return true;
        }

        protected override DyFunction Clone(ExecutionContext ctx) => new DyInteropFunction(name, type, flags);

        internal override bool Equals(DyFunction func) => func is DyInteropFunction f
            && f.name == name && f.type.Equals(type);
    }

    
}
