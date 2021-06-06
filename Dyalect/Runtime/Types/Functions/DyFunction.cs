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
        internal DyObject? Self;
        internal Par[] Parameters;
        internal int VarArgIndex;
        internal int Attr;

        internal bool Auto => (Attr & FunAttr.Auto) == FunAttr.Auto;

        protected DyFunction(int typeId, Par[] pars, int varArgIndex) : base(typeId) =>
            (Parameters, VarArgIndex) = (pars, varArgIndex);

        public override object ToObject() => (Func<ExecutionContext, DyObject[], DyObject>)Call;

        internal abstract DyFunction BindToInstance(ExecutionContext ctx, DyObject arg);

        internal virtual DyObject BindOrRun(ExecutionContext ctx, DyObject arg) => BindToInstance(ctx, arg);
        
        internal abstract DyObject InternalCall(ExecutionContext ctx, DyObject[] args);

        internal abstract DyObject InternalCall(ExecutionContext ctx);

        public DyObject Call(ExecutionContext ctx, params DyObject[] args)
        {
            var newArgs = PrepareArguments(ctx, args);

            if (ctx.HasErrors)
                return DyNil.Instance;

            return InternalCall(ctx, newArgs);
        }

        protected DyObject[] PrepareArguments(ExecutionContext ctx, DyObject[] args)
        {
            if (Parameters.Length == 0 && args.Length == 0)
                return args;

            if (args.Length > Parameters.Length)
            {
                ctx.TooManyArguments(FunctionName, Parameters.Length, args.Length);
                return args;
            }

            DyObject[] newLocals;

            if (args.Length == Parameters.Length)
                newLocals = args;
            else
            {
                newLocals = new DyObject[Parameters.Length];
                if (args.Length > 0)
                    Array.Copy(args, newLocals, args.Length);
            }

            if (VarArgIndex > -1)
            {
                var o = newLocals[VarArgIndex];
                if (o.TypeId == DyType.Nil)
                    newLocals[VarArgIndex] = DyTuple.Empty;
                else if (o.TypeId == DyType.Array)
                {
                    var arr = (DyArray)o;
                    arr.Compact();
                    newLocals[VarArgIndex] = new DyTuple(arr.Values);
                }
                else if (o.TypeId != DyType.Tuple)
                {
                    newLocals[VarArgIndex] = DyTuple.Create(o);
                }
            }

            DyMachine.FillDefaults(newLocals, this, ctx);
            return newLocals;
        }

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

            if (FunctionName is null)
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

        internal virtual MemoryLayout? GetLayout(ExecutionContext ctx) => null;

        internal abstract DyObject[] CreateLocals(ExecutionContext ctx);

        internal abstract bool Equals(DyFunction func);

        public override int GetHashCode() => HashCode.Combine(TypeId, FunctionName ?? DefaultName, Parameters, Self);

        internal virtual void Reset(ExecutionContext ctx) { }
    }
}
