using Dyalect.Debug;
using System;

namespace Dyalect.Runtime.Types
{
    public abstract class DyForeignFunction : DyFunction
    {
        #region Nested types
        private sealed class CompositionContainer : DyForeignFunction
        {
            private readonly DyFunction first;
            private readonly DyFunction second;

            public CompositionContainer(DyFunction first, DyFunction second) : base(null, first.Parameters, first.TypeId, first.VarArgIndex)
            {
                this.first = first;
                this.second = second;
            }

            internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)
            {
                var res = first.Call(ctx, args);

                if (ctx.HasErrors)
                    return DyNil.Instance;

                return second.Call(ctx, res);
            }

            internal override bool Equals(DyFunction func) => func is CompositionContainer cc
                && cc.first.Equals(first) && cc.second.Equals(second);
        }

        private sealed class MemberFunction : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject[], DyObject> fun;

            public MemberFunction(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, Par[] pars, int varArgIndex) 
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self!, args);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction0 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject> fun;

            public MemberFunction0(string name, Func<ExecutionContext, DyObject, DyObject> fun)
                : base(name, Array.Empty<Par>(), -1) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self!);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction0(FunctionName, fun);

            internal override bool Equals(DyFunction func) => func is MemberFunction0 m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction1 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

            public MemberFunction1(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex) 
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self!, args[0]);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction1(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction1 m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction2 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

            public MemberFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self!, args[0], args[1]);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction2(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction2 m && m.fun.Equals(fun);
        }

        private sealed class MemberFunction3 : DyForeignFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

            public MemberFunction3(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, Self!, args[0], args[1], args[2]);

            protected override DyFunction Clone(ExecutionContext ctx) => new MemberFunction3(FunctionName, fun, Parameters, VarArgIndex);

            internal override bool Equals(DyFunction func) => func is MemberFunction3 m && m.fun.Equals(fun);
        }

        private abstract class BaseStaticFunction : DyForeignFunction
        {
            protected BaseStaticFunction(string? name, Par[] pars, int varArgIndex) : base(name, pars, varArgIndex) { }

            protected override DyFunction Clone(ExecutionContext ctx) => this;
        }

        private sealed class StaticFunction0 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject> fun;

            public StaticFunction0(string name, Func<ExecutionContext, DyObject> fun, Par[] pars)
                : base(name, pars, -1) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx);

            internal override bool Equals(DyFunction func) => func is StaticFunction0 m && m.fun.Equals(fun);
        }

        private sealed class StaticFunction1 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject> fun;

            public StaticFunction1(string name, Func<ExecutionContext, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args[0]);

            internal override bool Equals(DyFunction func) => func is StaticFunction1 m && m.fun.Equals(fun);
        }

        private sealed class StaticFunction2 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject> fun;

            public StaticFunction2(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args[0], args[1]);

            internal override bool Equals(DyFunction func) => func is StaticFunction2 m && m.fun.Equals(fun);
        }

        private sealed class StaticFunction3 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun;

            public StaticFunction3(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args[0], args[1], args[2]);

            internal override bool Equals(DyFunction func) => func is StaticFunction3 m && m.fun.Equals(fun);
        }

        private sealed class StaticFunction4 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

            public StaticFunction4(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3]);

            internal override bool Equals(DyFunction func) => func is StaticFunction4 m && m.fun.Equals(fun);
        }

        private sealed class StaticFunction5 : BaseStaticFunction
        {
            private readonly Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;

            public StaticFunction5(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, Par[] pars, int varArgIndex)
                : base(name, pars, varArgIndex) => this.fun = fun;

            internal override DyObject InternalCall(ExecutionContext ctx, params DyObject[] args) => fun(ctx, args[0], args[1], args[2], args[3], args[4]);

            internal override bool Equals(DyFunction func) => func is StaticFunction5 m && m.fun.Equals(fun);
        }
        #endregion

        private readonly string name;

        protected DyForeignFunction(string? name, Par[] pars, int varArgIndex)
            : base(DyType.Function, pars, varArgIndex) => this.name = name ?? DefaultName;

        internal DyForeignFunction(string? name, Par[] pars, int typeId, int varArgIndex) : base(typeId, pars, varArgIndex) =>
            this.name = name ?? DefaultName;

        internal static DyFunction Compose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject[], DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject> fun) => new MemberFunction0(name, fun);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction1(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction2(name, fun, pars, varArgIndex);

        public static DyFunction Member(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new MemberFunction3(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject> fun) => new StaticFunction0(name, fun, Array.Empty<Par>());

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction1(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction2(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction3(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction4(name, fun, pars, varArgIndex);

        public static DyFunction Static(string name, Func<ExecutionContext, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun, int varArgIndex, params Par[] pars) => new StaticFunction5(name, fun, pars, varArgIndex);

        public override string FunctionName => name;

        public override bool IsExternal => true;

        internal override DyFunction BindToInstance(ExecutionContext ctx, DyObject arg)
        {
            var clone = Clone(ctx);
            clone.Self = arg;
            return clone;
        }

        protected virtual DyFunction Clone(ExecutionContext ctx) => (DyForeignFunction)MemberwiseClone();

        internal override DyObject[] CreateLocals(ExecutionContext ctx) =>
            Parameters.Length == 0 ? Array.Empty<DyObject>() : new DyObject[Parameters.Length];
    }
}
