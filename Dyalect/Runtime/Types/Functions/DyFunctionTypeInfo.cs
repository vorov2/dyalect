using Dyalect.Debug;

namespace Dyalect.Runtime.Types
{
    internal sealed class DyFunctionTypeInfo : DyTypeInfo
    {
        public DyFunctionTypeInfo(DyTypeInfo typeInfo) : base(typeInfo, DyTypeCode.Function) { }

        protected override SupportedOperations GetSupportedOperations() =>
            SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

        public override string TypeName => DyTypeNames.Function;

        protected override DyObject ToStringOp(DyObject arg, ExecutionContext ctx) =>
            new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, arg.ToString());

        protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
            left.DecType.TypeCode == right.DecType.TypeCode && ((DyFunction)left).Equals((DyFunction)right) ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False;

        private DyObject GetName(ExecutionContext ctx, DyObject self) =>
            new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, ((DyFunction)self).FunctionName);

        private DyObject GetParameters(ExecutionContext ctx, DyObject self)
        {
            var fn = (DyFunction)self;
            var arr = new DyObject[fn.Parameters.Length];

            for (var i = 0; i < fn.Parameters.Length; i++)
            {
                var p = fn.Parameters[i];
                arr[i] = new DyTuple(ctx.RuntimeContext.Tuple,
                        new[] {
                            new DyLabel("name", new DyString(ctx.RuntimeContext.String, ctx.RuntimeContext.Char, p.Name)),
                            new DyLabel("hasDefault", p.Value is not null ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False),
                            new DyLabel("default", p.Value != null ? p.Value.ToRuntimeType(ctx.RuntimeContext) : ctx.RuntimeContext.Nil.Instance),
                            new DyLabel("varArg", fn.VarArgIndex == i ? ctx.RuntimeContext.Bool.True : ctx.RuntimeContext.Bool.False)
                        }
                    );
            }

            return new DyArray(ctx.RuntimeContext.Array, arr);
        }

        protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
            name switch
            {
                "compose" => Func.Member(name, Compose, -1, new Par("with")),
                "name" => Func.Auto(name, GetName),
                "parameters" => Func.Auto(name, GetParameters),
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

            return ctx.RuntimeContext.Nil.Instance;
        }

        protected override DyObject? InitializeStaticMember(string name, ExecutionContext ctx)
        {
            if (name == "compose")
                return Func.Static(name, Compose, -1, new Par("first"), new Par("second"));

            return base.InitializeStaticMember(name, ctx);
        }
    }
}
