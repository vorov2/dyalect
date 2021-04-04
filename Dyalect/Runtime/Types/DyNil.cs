using System.IO;

namespace Dyalect.Runtime.Types
{
    public class DyNil : DyObject
    {
        private sealed class DyTerminator : DyNil { }

        public static readonly DyNil Instance = new();
        internal static readonly DyNil Terminator = new DyTerminator();

        private DyNil() : base(DyType.Nil)
        {

        }

        public override object ToObject() => null;

        protected internal override bool GetBool() => false;

        public override string ToString() => "nil";

        public override DyObject Clone() => this;

        internal override int GetCount() => 0;

        internal protected override DyObject GetItem(DyObject index, ExecutionContext ctx) =>
            ctx.IndexOutOfRange(index);

        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
        }

        public override int GetHashCode() => 0;
    }

    internal sealed class DyNilTypeInfo : DyTypeInfo
    {
        public DyNilTypeInfo() : base(DyType.Nil)
        {

        }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Nil;

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.TypeId == right.TypeId ? DyBool.True : DyBool.False;

        protected override DyObject NotOp(DyObject arg, ExecutionContext ctx) => DyBool.True;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) => new DyString("nil");

        protected override DyFunction GetStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "Nil")
                return DyForeignFunction.Static(name, _ => DyNil.Instance);

            if (name == "default")
                return DyForeignFunction.Static(name, _ => DyNil.Instance);

            return base.GetStaticMember(name, ctx);
        }
    }
}
