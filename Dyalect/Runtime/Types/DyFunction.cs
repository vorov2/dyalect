using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Parser;
using System;
using System.Text;

namespace Dyalect.Runtime.Types
{
    public abstract class DyFunction : DyObject
    {
        internal const string DefaultName = "<func>";
        internal DyObject Self;
        internal Par[] Parameters;
        internal int VarArgIndex;

        protected DyFunction(int typeId, Par[] pars, int varArgIndex) : base(typeId) =>
            (Parameters, VarArgIndex) = (pars, varArgIndex);

        public override object ToObject() => (Func<ExecutionContext, DyObject[], DyObject>)Call;

        internal abstract DyFunction BindToInstance(ExecutionContext ctx, DyObject arg);

        public abstract DyObject Call(ExecutionContext ctx, params DyObject[] args);

        internal virtual DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx) =>  Call(ctx, left, right);

        internal virtual DyObject Call1(DyObject obj, ExecutionContext ctx) => Call(ctx, obj);

        internal virtual DyObject Call0(ExecutionContext ctx) => Call(ctx, Array.Empty<DyObject>());

        internal int GetParameterIndex(string name)
        {
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (Parameters[i].Name == name)
                    return i;
            }

            return -1;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (FunctionName == null)
                sb.Append(DefaultName);
            else
                sb.Append(FunctionName);

            sb.Append('(');
            var c = 0;

            foreach (var p in Parameters)
            {
                if (c != 0)
                    sb.Append(", ");

                sb.Append(p.Name);

                if (p.IsVarArg)
                    sb.Append("...");

                if (p.Value != null)
                {
                    sb.Append(" = ");
                    if (p.Value.TypeId == DyType.String)
                        sb.Append(StringUtil.Escape(p.Value.ToString()));
                    else if (p.Value.TypeId == DyType.Char)
                        sb.Append(StringUtil.Escape(p.Value.ToString(), "'"));
                    else
                        sb.Append(p.Value.ToString());
                }

                c++;
            }

            sb.Append(')');
            var ret = sb.ToString();
            return ret;
        }

        public abstract string FunctionName { get; }

        public abstract bool IsExternal { get; }

        internal virtual MemoryLayout GetLayout(ExecutionContext ctx) => null;

        internal abstract DyObject[] CreateLocals(ExecutionContext ctx);

        internal abstract bool Equals(DyFunction func);

        public override int GetHashCode() => HashCode.Combine(TypeId, FunctionName ?? DefaultName, Parameters, Self);

        internal virtual void Reset(ExecutionContext ctx) { }
    }

    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public DyFunctionTypeInfo() : base(DyType.Function) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Function;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(arg.ToString());

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId && ((DyFunction)left).Equals((DyFunction)right) ? DyBool.True : DyBool.False;

        protected override DyFunction InitializeInstanceMember(string name, ExecutionContext ctx)
        {
            if (name == "compose")
                return DyForeignFunction.Member(name, Compose, -1, new Par("with"));

            return base.InitializeInstanceMember(name, ctx);
        }

        protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx)
        {
            if (index.TypeId == DyType.String && index.GetString() == "name")
                return new DyString(((DyFunction)self).FunctionName);
            return ctx.IndexOutOfRange();
        }

        private DyObject Compose(ExecutionContext ctx, DyObject first, DyObject second)
        {
            if (first is DyFunction f1)
            {
                if (second is DyFunction f2)
                    return DyForeignFunction.Compose(f1, f2);
                else
                    ctx.InvalidType(second);
            }
            else
                ctx.InvalidType(first);

            return DyNil.Instance;
        }

        protected override DyFunction InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "compose")
                return DyForeignFunction.Static(name, Compose, -1, new Par("first"), new Par("second"));

            if (name == "Function")
                return DyForeignFunction.Static(name, (c, obj) => DyForeignFunction.Static("id", _ => obj), -1, new Par("value"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
