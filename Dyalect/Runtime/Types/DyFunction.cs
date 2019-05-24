﻿using Dyalect.Compiler;
using Dyalect.Debug;
using System;
using System.Linq;

namespace Dyalect.Runtime.Types
{
    public abstract class DyFunction : DyObject
    {
        internal const string DefaultName = "<func>";
        internal DyObject Self;
        internal Par[] Parameters;
        internal int VarArgIndex;

        protected DyFunction(int typeId, Par[] pars, int varArgIndex) : base(typeId)
        {
            Parameters = pars;
            VarArgIndex = varArgIndex;
        }

        public override object ToObject() => (Func<ExecutionContext, DyObject[], DyObject>)Call;

        internal abstract DyFunction Clone(ExecutionContext ctx, DyObject arg);

        public abstract DyObject Call(ExecutionContext ctx, params DyObject[] args);

        internal virtual DyObject Call2(DyObject left, DyObject right, ExecutionContext ctx) =>  Call(ctx, left, right);

        internal virtual DyObject Call1(DyObject obj, ExecutionContext ctx) => Call(ctx, obj);

        internal virtual DyObject Call0(ExecutionContext ctx) => Call(ctx, Statics.EmptyDyObjects);

        internal int GetParameterIndex(string name, ExecutionContext ctx)
        {
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (Parameters[i].Name == name)
                    return i;
            }

            return -1;
        }

        public string ToString(ExecutionContext ctx) => 
            $"{FunctionName}({string.Join(",", Parameters.Select(p => p.Name))})";

        public abstract string FunctionName { get; }

        public abstract bool IsExternal { get; }

        internal virtual MemoryLayout GetLayout(ExecutionContext ctx) => null;

        internal abstract DyObject[] CreateLocals(ExecutionContext ctx);

        internal abstract bool Equals(DyFunction func);
    }

    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public DyFunctionTypeInfo() : base(DyType.Function, false)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Function;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(((DyFunction)arg).ToString());

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId && ((DyFunction)left).Equals((DyFunction)right) ? DyBool.True : DyBool.False;
    }
}
