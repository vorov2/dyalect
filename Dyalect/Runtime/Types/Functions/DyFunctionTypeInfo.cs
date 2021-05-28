using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
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

        private DyObject GetName(ExecutionContext _, DyObject self) => new DyString(((DyFunction)self).FunctionName);

        private DyObject GetParameters(ExecutionContext _, DyObject self)
        {
            var fn = (DyFunction)self;
            var arr = new DyObject[fn.Parameters.Length];

            for (var i = 0; i < fn.Parameters.Length; i++)
            {
                var p = fn.Parameters[i];
                arr[i] = new DyTuple(
                        new[] {
                            new DyLabel("name", new DyString(p.Name)),
                            new DyLabel("hasDefault", (DyBool)(p.Value is not null)),
                            new DyLabel("default", p.Value ?? DyNil.Instance),
                            new DyLabel("varArg", (DyBool)(fn.VarArgIndex == i))
                        }
                    );
            }

            return new DyArray(arr);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "compose" => Func.Member(name, Compose, -1, new Par("with")),
                "name" => Func.Member(name, GetName),
                "parameters" => Func.Member(name, GetParameters),
                _ => base.InitializeInstanceMember(self, name, ctx)
            };

        private DyObject Compose(ExecutionContext ctx, DyObject first, DyObject second)
        {
            if (first is DyFunction f1)
            {
                if (second is DyFunction f2)
                    return Func.Compose(f1, f2);
                else
                    ctx.InvalidType(second);
            }
            else
                ctx.InvalidType(first);

            return DyNil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "compose")
                return Func.Static(name, Compose, -1, new Par("first"), new Par("second"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
