using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    public static class Func
    {
        public static DyFunction Compose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(name, fun);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction3(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction4(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Array.Empty<Par>());

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction5(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction6(name, fun, pars, varArgIndex);

        public static DyFunction Auto(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new AutoFunction(name, fun);
    }
}
