using Dyalect.Compiler;
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
        private List<MethodInfo> methods;

        public override string FunctionName => name;

        public DyInteropFunction(string name, Type type, List<MethodInfo> methods, bool auto) : base(name, pars, 0) =>
            (this.name, this.type, this.methods, Attr) = (name, type, methods, auto ? FunAttr.Auto : FunAttr.None);

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg)
        {
            if (Auto)
                return CallInteropMethod(ctx, arg, Array.Empty<DyObject>());

            return base.BindOrRun(ctx, arg);
        }

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => CallInteropMethod(ctx, Self, args);

        private DyObject CallInteropMethod(ExecutionContext ctx, DyObject? self, DyObject[] args)
        {
            var tupleArgs = args.Length > 0 ? ((DyTuple)args[0]).UnsafeAccessValues() : null;
            var arguments = tupleArgs is null ? Array.Empty<object>() : tupleArgs.Select(a => a.ToObject()).ToArray();
            var argumentTypes = tupleArgs is null ? Array.Empty<Type>() : arguments.Select(a => a.GetType()).ToArray();

            if (!ResolveMethod(self, arguments, argumentTypes, false, out var result))
                if (!ResolveMethod(self, arguments, argumentTypes, true, out result))
                    return ctx.MethodNotFound(name, type, tupleArgs);

            return result;
        }

        private bool ResolveMethod(DyObject? self, object[] arguments, Type[] argumentTypes, bool generalize, out DyObject result)
        {
            result = DyNil.Instance;

            foreach (var m in methods)
            {
                if (m.Name == name)
                {
                    var pars = m.GetParameters();

                    if (pars.Length != arguments.Length || !CheckArguments(arguments, argumentTypes, generalize, pars))
                        continue;

                    object? @this = self;

                    if (@this is not null)
                    {
                        if (self is DyInteropObject obj)
                            @this = obj.Object;
                        else if (self is DyInteropSpecificObjectTypeInfo spec)
                            @this = spec.Type;
                    }

                    var ret = m.Invoke(@this, arguments);
                    
                    if (ret is null)
                        result = DyNil.Instance;
                    else if (ret is int i)
                        result = DyInteger.Get(i);
                    else if (ret is long l)
                        result = DyInteger.Get(l);
                    else if (ret is char c)
                        result = new DyChar(c);
                    else if (ret is string s)
                        result = new DyString(s);
                    else if (ret is bool b)
                        result = b ? DyBool.True : DyBool.False;
                    else if (ret is double d)
                        result = new DyFloat(d);
                    else if (ret is float f)
                        result = new DyFloat(f);
                    else
                        result = new DyInteropObject(ret.GetType(), ret);

                    return true;
                }
            }

            return false;
        }

        private bool CheckArguments(object[] arguments, Type[] argumentTypes, bool generalize, ParameterInfo[] pars)
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

        protected override DyFunction Clone(ExecutionContext ctx) => new DyInteropFunction(name, type, methods, Attr == FunAttr.Auto);

        internal override bool Equals(DyFunction func) => func is DyInteropFunction f
            && f.name == name && f.type.Equals(type);
    }    
}
