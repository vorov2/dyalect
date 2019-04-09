using System;

namespace Dyalect.Runtime.Types
{
    internal abstract class CallAdapter
    {
        private CallAdapter()
        {

        }

        public abstract DyObject Call(ExecutionContext ctx, params DyObject[] args);

        protected abstract Delegate GetDelegate();

        internal sealed class Arg0 : CallAdapter
        {
            private readonly Func<DyObject> fun;
            internal Arg0(Func<DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun();
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg1 : CallAdapter
        {
            private readonly Func<DyObject, DyObject> fun;
            internal Arg1(Func<DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg2 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject> fun;
            internal Arg2(Func<DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg3 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg3(Func<DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg4 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg4(Func<DyObject, DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2], args[3]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg5 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg5(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2], args[3], args[4]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg6 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg6(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2], args[3], args[4], args[5]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg7 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg7(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg8 : CallAdapter
        {
            private readonly Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun;
            internal Arg8(Func<DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject, DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class ArgAny : CallAdapter
        {
            private readonly Func<DyObject[], DyObject> fun;
            internal ArgAny(Func<DyObject[], DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(args);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg0<T1> : CallAdapter
        {
            private readonly Func<T1> fun;
            internal Arg0(Func<T1> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => TypeConverter.ConvertFrom<T1>(this.fun(), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg1<T1, T2> : CallAdapter
        {
            private readonly Func<T1, T2> fun;
            internal Arg1(Func<T1, T2> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T2>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg2<T1, T2, T3> : CallAdapter
        {
            private readonly Func<T1, T2, T3> fun;
            internal Arg2(Func<T1, T2, T3> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T3>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg3<T1, T2, T3, T4> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4> fun;
            internal Arg3(Func<T1, T2, T3, T4> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T4>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg4<T1, T2, T3, T4, T5> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4, T5> fun;
            internal Arg4(Func<T1, T2, T3, T4, T5> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T5>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx), TypeConverter.ConvertTo<T4>(args[3], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg5<T1, T2, T3, T4, T5, T6> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4, T5, T6> fun;
            internal Arg5(Func<T1, T2, T3, T4, T5, T6> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T6>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx), TypeConverter.ConvertTo<T4>(args[3], ctx), TypeConverter.ConvertTo<T5>(args[4], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg6<T1, T2, T3, T4, T5, T6, T7> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4, T5, T6, T7> fun;
            internal Arg6(Func<T1, T2, T3, T4, T5, T6, T7> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T7>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx), TypeConverter.ConvertTo<T4>(args[3], ctx), TypeConverter.ConvertTo<T5>(args[4], ctx),
                    TypeConverter.ConvertTo<T6>(args[5], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg7<T1, T2, T3, T4, T5, T6, T7, T8> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8> fun;
            internal Arg7(Func<T1, T2, T3, T4, T5, T6, T7, T8> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T8>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx), TypeConverter.ConvertTo<T4>(args[3], ctx), TypeConverter.ConvertTo<T5>(args[4], ctx),
                    TypeConverter.ConvertTo<T6>(args[5], ctx), TypeConverter.ConvertTo<T7>(args[6], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class Arg8<T1, T2, T3, T4, T5, T6, T7, T8, T9> : CallAdapter
        {
            private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, T9> fun;
            internal Arg8(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) =>
                TypeConverter.ConvertFrom<T9>(this.fun(TypeConverter.ConvertTo<T1>(args[0], ctx), TypeConverter.ConvertTo<T2>(args[1], ctx),
                    TypeConverter.ConvertTo<T3>(args[2], ctx), TypeConverter.ConvertTo<T4>(args[3], ctx), TypeConverter.ConvertTo<T5>(args[4], ctx),
                    TypeConverter.ConvertTo<T6>(args[5], ctx), TypeConverter.ConvertTo<T7>(args[6], ctx), TypeConverter.ConvertTo<T8>(args[7], ctx)), ctx);
            protected override Delegate GetDelegate() => this.fun;
        }

        internal sealed class ArgAny<T1, T2> : CallAdapter
        {
            private readonly Func<T1[], T2> fun;
            internal ArgAny(Func<T1[], T2> fun) => this.fun = fun;
            protected override Delegate GetDelegate() => this.fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args)
            {
                var arr = new T1[args.Length];

                for (var i = 0; i < args.Length; i++)
                    arr[i] = TypeConverter.ConvertTo<T1>(args[i], ctx);

                return TypeConverter.ConvertFrom<T2>(this.fun(arr), ctx);
            }
        }

        internal sealed class ArgCtx : CallAdapter
        {
            private readonly Func<ExecutionContext, DyObject[], DyObject> fun;
            internal ArgCtx(Func<ExecutionContext, DyObject[], DyObject> fun) => this.fun = fun;
            public override DyObject Call(ExecutionContext ctx, params DyObject[] args) => this.fun(ctx, args);
            protected override Delegate GetDelegate() => this.fun;
        }
    }
}
