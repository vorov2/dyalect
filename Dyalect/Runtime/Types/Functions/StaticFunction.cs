using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal abstract class BaseStaticFunction : DyForeignFunction
    {
        protected BaseStaticFunction(string? name, Par[] pars, int varArgIndex) : 
            base(name, pars, varArgIndex) { }

        protected override DyFunction Clone(ExecutionContext ctx) => this;
    }

    internal sealed class StaticFunction0 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject> fun;

        public StaticFunction0(string name, Func<ExecutionContext, DyObject> fun, Par[] pars)
            : base(name, pars, -1) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx);

        internal override bool Equals(DyFunction func) => func is StaticFunction0 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction1 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject> fun;

        public StaticFunction1(string name, Func<ExecutionContext, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0]);

        internal override bool Equals(DyFunction func) => func is StaticFunction1 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction2 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

        public StaticFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1]);

        internal override bool Equals(DyFunction func) => func is StaticFunction2 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction3 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

        public StaticFunction3(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1], args[2]);

        internal override bool Equals(DyFunction func) => func is StaticFunction3 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction4 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public StaticFunction4(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3]);

        internal override bool Equals(DyFunction func) => func is StaticFunction4 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction5 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public StaticFunction5(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3], args[4]);

        internal override bool Equals(DyFunction func) => func is StaticFunction5 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction6 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public StaticFunction6(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3], args[4], args[5]);

        internal override bool Equals(DyFunction func) => func is StaticFunction6 m && m.fun.Equals(fun);
    }

    internal sealed class StaticFunction7 : BaseStaticFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public StaticFunction7(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);

        internal override bool Equals(DyFunction func) => func is StaticFunction7 m && m.fun.Equals(fun);
    }
}
