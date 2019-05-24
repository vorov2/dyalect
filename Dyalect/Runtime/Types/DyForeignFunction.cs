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

            internal override bool Equals(DyFunction func) => func is MemberFunction m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction0 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject> fun;

            public MemberFunction0(string name, Func<ExecutionContext, DyObject, DyObject> fun) : base(name, Statics.EmptyParameters, -1)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction0(FunctionName, fun);

            internal override bool Equals(DyFunction func) => func is MemberFunction0 m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction1 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

            public MemberFunction1(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self, args[0]);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction1(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction1 m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction2 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

            public MemberFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self, args[0], args[1]);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction2(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction2 m && m.fun.Equals(fun);
        }

        private abstract class BaseStaticFunction : DyForeignFunction
        {
            protected BaseStaticFunction(string name, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex)
            {
            }

            protected override DyFunction Clone(ExecutionContext ctx) => this;
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
                    return ctx.ExternalFunctionFailure(FunctionName, ex.Message);
                }
            }

            internal override bool Equals(DyFunction func) => func is StaticFunction0 m && m.fun.Equals(fun);
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
                    return ctx.ExternalFunctionFailure(FunctionName, ex.Message);
                }
            }

            internal override bool Equals(DyFunction func) => func is StaticFunction1 m && m.fun.Equals(fun);
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
                    return ctx.ExternalFunctionFailure(FunctionName, ex.Message);
                }
            }

            internal override bool Equals(DyFunction func) => func is StaticFunction2 m && m.fun.Equals(fun);
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
                    return ctx.ExternalFunctionFailure(FunctionName, ex.Message);
                }
            }

            internal override bool Equals(DyFunction func) => func is StaticFunction3 m && m.fun.Equals(fun);
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
                    return ctx.ExternalFunctionFailure(FunctionName, ex.Message);
                }
            }

            internal override bool Equals(DyFunction func) => func is StaticFunction4 m && m.fun.Equals(fun);
        }

        private readonly string name;

        protected DyForeignFunction(string name, Par[] pars, int varArgIndex) : base(DyType.Function, pars, varArgIndex)
        {
            this.name = name ?? DefaultName;
        }

        internal DyForeignFunction(string name, Par[] pars, int typeId, int varArgIndex) : base(typeId, pars, varArgIndex)
        {
            this.name = name ?? DefaultName;
        }

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(name, fun);

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(name, fun, pars, varArgIndex);

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Statics.EmptyParameters);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

        public override string FunctionName => name;

        public override bool IsExternal => true;

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
