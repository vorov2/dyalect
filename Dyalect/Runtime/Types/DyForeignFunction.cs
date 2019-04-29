using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignFunction : DyFunction
    {
        private sealed class DyCallBackFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject[], DyObject> fun;

            public DyCallBackFunction(string name, Func<ExecutionContext, DyObject[], DyObject> fun) : base(name, 0)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new DyCallBackFunction(GetFunctionName(ctx), fun);
        }

        private sealed class DyCallBackMemberFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject[], DyObject> fun;

            public DyCallBackMemberFunction(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun) : base(name, 0)
            {
                this.fun = fun;
            }

            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new DyCallBackMemberFunction(GetFunctionName(ctx), fun);
        }

        private readonly string name;

        protected DyForeignFunction(string name, int pars) : base(StandardType.Function, pars)
        {
            this.name = name ?? DefaultName;
        }

        internal DyForeignFunction(string name, int pars, int typeId) : base(typeId, pars)
        {
            this.name = name ?? DefaultName;
        }

        public static DyForeignFunction Create(string name, Func<ExecutionContext, DyObject[], DyObject> fun) => new DyCallBackFunction(name, fun);

        public static DyForeignFunction Create(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun) => new DyCallBackMemberFunction(name, fun);

        protected override string GetCustomFunctionName(ExecutionContext ctx) => name;

        internal override DyFunction Clone(ExecutionContext ctx, DyObject arg)
        {
            var clone = Clone(ctx);
            clone.Self = arg;
            return clone;
        }

        protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();
    }
}
