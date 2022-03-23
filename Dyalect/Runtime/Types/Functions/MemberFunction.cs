using Dyalect.Compiler;
using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    internal sealed class AutoFunction0 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject> fun;

        public AutoFunction0(string name, Func<ExecutionContext, DyObject> fun)
            : base(name, Array.Empty<Par>(), -1)
        {
            this.fun = fun;
            Attr |= FunAttr.Auto;
        }

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx);

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => fun(ctx);

        protected override DyFunction Clone(ExecutionContext ctx) => new AutoFunction0(FunctionName, fun);

        internal override bool Equals(DyFunction func) => func is AutoFunction0 m && m.fun.Equals(fun);
    }

    internal sealed class AutoFunction1 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject> fun;

        public AutoFunction1(string name, Func<ExecutionContext, DyObject, DyObject> fun)
            : base(name, Array.Empty<Par>(), -1)
        {
            this.fun = fun;
            Attr |= FunAttr.Auto;
        }

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!);

        internal override DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => fun(ctx, arg);

        protected override DyFunction Clone(ExecutionContext ctx) => new AutoFunction1(FunctionName, fun);

        internal override bool Equals(DyFunction func) => func is AutoFunction1 m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject[], DyObject> fun;

        public MemberFunction(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction(FunctionName, fun, Parameters, VarArgIndex);

        internal override bool Equals(DyFunction func) => func is MemberFunction m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction0 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject> fun;

        public MemberFunction0(string name, Func<ExecutionContext, DyObject, DyObject> fun)
            : base(name, Array.Empty<Par>(), -1) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction0(FunctionName, fun);

        internal override bool Equals(DyFunction func) => func is MemberFunction0 m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction1 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

        public MemberFunction1(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0]);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction1(FunctionName, fun, Parameters, VarArgIndex);

        internal override bool Equals(DyFunction func) => func is MemberFunction1 m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction2 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

        public MemberFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0], args[1]);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction2(FunctionName, fun, Parameters, VarArgIndex);

        internal override bool Equals(DyFunction func) => func is MemberFunction2 m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction3 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public MemberFunction3(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0], args[1], args[2]);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction3(FunctionName, fun, Parameters, VarArgIndex);

        internal override bool Equals(DyFunction func) => func is MemberFunction3 m && m.fun.Equals(fun);
    }

    internal sealed class MemberFunction4 : DyForeignFunction
    {
        private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

        public MemberFunction4(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
            : base(name, pars, varArgIndex) => this.fun = fun;

        internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args) => fun(ctx, Self!, args[0], args[1], args[2], args[3]);

        protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction4(FunctionName, fun, Parameters, VarArgIndex);

        internal override bool Equals(DyFunction func) => func is MemberFunction4 m && m.fun.Equals(fun);
    }
}
