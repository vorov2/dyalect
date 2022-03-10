using System;
using System.IO;

namespace Dyalect.Runtime.Types
{
    public class DyLazy : DyObject
    {
        private DyFunction? func;
        private DyObject? value;

        internal DyLazy(DyFunction func) : base(DyType.Lazy) => this.func = func;

        public override object ToObject() => (value is not null ? value : func)!;

        internal override DyObject? Force(ExecutionContext ctx)
        {
            if (func is not null)
            {
                value = func.InternalCall(ctx);

                if (ctx.HasErrors)
                    return null;
                else
                    func = null;
            }

            return value;
        }

        protected internal override bool GetBool(ExecutionContext ctx) =>
            Force(ctx) is not null && value!.GetBool(ctx);

        protected internal override bool HasItem(string name, ExecutionContext ctx) =>
            Force(ctx) is not null && value!.HasItem(name, ctx);

        public override string ToString() => "nil";

        public override DyObject Clone() => this;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            ctx.IndexOutOfRange();

        internal override void Serialize(BinaryWriter writer) => writer.Write(TypeId);

        public override int GetHashCode() => HashCode.Combine(DyTypeNames.Nil, TypeId);
    }
}
