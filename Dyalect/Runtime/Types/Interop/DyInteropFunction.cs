using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dyalect.Runtime.Types
{
    public static class Foobar
    {
        public static void Execute(int i) {
            throw new Exception("What the fuck!");
        }
    }

    internal sealed class DyInteropFunction : DyForeignFunction
    {
        private readonly static Par[] pars = new Par[] { new Par("args", isVarArg: true) };
        private readonly string name;
        private readonly Type type;
        private readonly List<MethodInfo> methods;
        private readonly ParameterInfo[][] parameters;

        public override string FunctionName => name;

        public DyInteropFunction(string name, Type type, List<MethodInfo> methods, bool auto) : base(name, pars, 0) =>
            (this.name, this.type, this.methods, Attr, parameters) = (name, type, methods, auto ? FunAttr.Auto : FunAttr.None, new ParameterInfo[methods.Count][]);

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
        {
            if (Auto)
                return CallInteropMethod(ctx, arg, Array.Empty<DyObject>());

            return base.BindOrRun(ctx, arg);
        }

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => CallInteropMethod(ctx, Self!, args);

        private DyObject CallInteropMethod(ExecutionContext ctx, DyObject self, DyObject[] args)
        {
            var tupleArgs = args.Length > 0 ? ((DyTuple)args[0]).UnsafeAccessValues() : null;
            var arguments = tupleArgs is null ? Array.Empty<object>() : tupleArgs.Select(a => a.ToObject()).ToArray();
            var argumentTypes = tupleArgs is null ? Array.Empty<Type>() : arguments.Select(a => a is null ? DyNil.Instance.GetType() : a.GetType()).ToArray();

            if (!ResolveMethod(self, arguments, argumentTypes, false, out var result))
                if (!ResolveMethod(self, arguments, argumentTypes, true, out result))
                    return ctx.MethodNotFound(name, type, tupleArgs);

            return result;
        }

        private bool ResolveMethod(DyObject self, object[] arguments, Type[] argumentTypes, bool generalize, out DyObject result)
        {
            result = DyNil.Instance;

            for (var i = 0; i < methods.Count; i++)
            {
                var m = methods[i];
                var pars = parameters[i] is null
                    ? parameters[i] = m.GetParameters() : parameters[i];

                if (pars.Length != arguments.Length || !CheckArguments(arguments, argumentTypes, generalize, pars))
                    continue;

                var ret = m.Invoke(m.IsStatic ? null : self.ToObject(), arguments);
                    
                if (ret is null)
                    result = DyNil.Instance;
                else if (ret is int i4)
                    result = DyInteger.Get(i4);
                else if (ret is long i8)
                    result = DyInteger.Get(i8);
                else if (ret is char c)
                    result = new DyChar(c);
                else if (ret is string s)
                    result = new DyString(s);
                else if (ret is bool i1)
                    result = i1 ? DyBool.True : DyBool.False;
                else if (ret is double r8)
                    result = new DyFloat(r8);
                else if (ret is float r4)
                    result = new DyFloat(r4);
                else
                    result = new DyInteropObject(ret.GetType(), ret);

                return true;
            }

            return false;
        }

        private bool CheckArguments(object[] arguments, Type[] argumentTypes, bool generalize, ParameterInfo[] pars)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var (pt, at) = (pars[i].ParameterType, argumentTypes[i]);

                if (!at.Equals(pt) && (!generalize || !pt.IsAssignableFrom(at)) && (arguments[i] is not null || !pt.IsClass))
                    return false;
            }

            return true;
        }

        protected override DyFunction Clone(ExecutionContext ctx) => new DyInteropFunction(name, type, methods, Attr == FunAttr.Auto);

        internal override bool Equals(DyFunction func) => func is DyInteropFunction f
            && f.name == name && f.type.Equals(type);
    }    
}
