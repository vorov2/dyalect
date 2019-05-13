using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignFunction : DyFunction
    {
        private sealed class MemberFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject[], DyObject> fun;

            public MemberFunction(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction(FunctionName, fun, Parameters, VarArgIndex);
        }

        private abstract class BaseStaticFunction : DyForeignFunction
        {
            protected BaseStaticFunction(string name, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
            }

            protected override DyFunction Clone(ExecutionContext ctx) => this;
        }

        private sealed class StaticFunction : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject[], DyObject> fun;

            public StaticFunction(string name, Func<ExecutionContext, DyObject[], DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args);
        }

        private sealed class StaticFunction0 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject> fun;

            public StaticFunction0(string name, Func<ExecutionContext, DyObject> fun, Par[] pars) : base(name, pars, -1)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                try
                {
                    return fun(ctx);
                }
                catch (Exception ex)
                {
                    ctx.Error = Err.ExternalFunctionFailure(FunctionName, ex.Message);
                    return DyNil.Instance;
                }
            }
        }

        private sealed class StaticFunction1 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject> fun;

            public StaticFunction1(string name, Func<ExecutionContext, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                try
                {
                    return fun(ctx, args[0]);
                }
                catch (Exception ex)
                {
                    ctx.Error = Err.ExternalFunctionFailure(FunctionName, ex.Message);
                    return DyNil.Instance;
                }
            }
        }

        private sealed class StaticFunction2 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

            public StaticFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                try
                {
                    return fun(ctx, args[0], args[1]);
                }
                catch (Exception ex)
                {
                    ctx.Error = Err.ExternalFunctionFailure(FunctionName, ex.Message);
                    return DyNil.Instance;
                }
            }
        }

        private sealed class StaticFunction3 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

            public StaticFunction3(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                try
                {
                    return fun(ctx, args[0], args[1], args[2]);
                }
                catch (Exception ex)
                {
                    ctx.Error = Err.ExternalFunctionFailure(FunctionName, ex.Message);
                    return DyNil.Instance;
                }
            }
        }

        private sealed class StaticFunction4 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

            public StaticFunction4(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                try
                {
                    return fun(ctx, args[0], args[1], args[2], args[3]);
                }
                catch (Exception ex)
                {
                    ctx.Error = Err.ExternalFunctionFailure(FunctionName, ex.Message);
                    return DyNil.Instance;
                }
            }
        }

        private readonly string name;

        protected DyForeignFunction(string name, Par[] pars, int varArgIndex) : base(StandardType.Function, pars, varArgIndex)
        {
            this.name = name ?? DefaultName;
        }

        internal DyForeignFunction(string name, Par[] pars, int typeId, int varArgIndex) : base(typeId, pars, varArgIndex)
        {
            this.name = name ?? DefaultName;
        }

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Statics.EmptyParameters);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

        public override string FunctionName => name;

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            var clone = Clone(ctx);
            clone.Self = arg;
            return clone;
        }

        protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();

        internal override DyObject[] CreateLocals(ExecutionContext ctx) => Parameters.Length == 0 ? Statics.EmptyDyObjects : new DyObject[Parameters.Length];
    }
}
