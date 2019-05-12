using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignFunction : DyFunction
    {
        private sealed class DyCallBackFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject[], DyObject> fun;

            public DyCallBackFunction(string name, Func<ExecutionContext, DyObject[], DyObject> fun) : base(name, null)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new DyCallBackFunction(FunctionName, fun);
        }

        private sealed class MemberFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject[], DyObject> fun;

            public MemberFunction(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, Par[] pars) : base(name, pars)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction(FunctionName, fun, Parameters);
        }

        private sealed class StaticFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject[], DyObject> fun;

            public StaticFunction(string name, Func<ExecutionContext, DyObject[], DyObject> fun, Par[] pars) : base(name, pars)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new StaticFunction(FunctionName, fun, Parameters);
        }

        private readonly string name;

        protected DyForeignFunction(string name, Par[] pars) : base(StandardType.Function, pars)
        {
            this.name = name ?? DefaultName;
        }

        internal DyForeignFunction(string name, Par[] pars, int typeId) : base(typeId, pars)
        {
            this.name = name ?? DefaultName;
        }

        public static DyForeignFunction Create(string name, Func<ExecutionContext, DyObject[], DyObject> fun) => new DyCallBackFunction(name, fun);

        internal static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, params Par[] pars) => new MemberFunction(name, fun, pars);

        internal static DyFunction Static(string name, Func<ExecutionContext, DyObject[], DyObject> fun, params Par[] pars) => new StaticFunction(name, fun, pars);

        public override string FunctionName => name;

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            var clone = Clone(ctx);
            clone.Self = arg;
            return clone;
        }

        protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();
    }
}
