using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    public static class Func
    {
        public static DyFunction Compose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);

        public static DyFunction Member(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Member(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(ctx.RuntimeContext.Function, name, fun);

        public static DyFunction Member(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Member(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Member(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction3(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(ctx.RuntimeContext.Function, name, fun, Array.Empty<Par>());

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Static(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction5(ctx.RuntimeContext.Function, name, fun, pars, varArgIndex);

        public static DyFunction Auto(ExecutionContext ctx, string name, Func<ExecutionContext, DyObject, DyObject> fun) => new AutoFunction(ctx.RuntimeContext.Function, name, fun);
    }
}
