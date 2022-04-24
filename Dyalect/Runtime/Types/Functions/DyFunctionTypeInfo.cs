using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

internal sealed class DyFunctionTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not;

    public override string ReflectedTypeName => DyTypeNames.Function;

    public override int ReflectedTypeId => DyType.Function;

    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString(arg.ToString());

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx) =>
        left.TypeId == right.TypeId && ((DyFunction)left).Equals((DyFunction)right) ? DyBool.True : DyBool.False;

    private DyObject GetName(ExecutionContext ctx, DyObject self) =>
        new DyString(((DyFunction)self).FunctionName);

    private DyObject GetParameters(ExecutionContext ctx, DyObject self)
    {
        var fn = (DyFunction)self;
        var arr = new DyObject[fn.Parameters.Length];

        for (var i = 0; i < fn.Parameters.Length; i++)
        {
            var p = fn.Parameters[i];
            arr[i] = new DyTuple(
                    new[] {
                        new DyLabel("name", new DyString(p.Name)),
                        new DyLabel("hasDefault", p.Value is not null ? DyBool.True : DyBool.False),
                        new DyLabel("default", p.Value != null ? p.Value : DyNil.Instance),
                        new DyLabel("varArg", fn.VarArgIndex == i ? DyBool.True : DyBool.False)
                    }
                );
        }

        return new DyArray(arr);
    }

    private DyObject Apply(ExecutionContext ctx, DyObject self, DyObject obj)
    {
        var tup = (DyTuple)obj;
        var tv = tup.UnsafeAccessValues();
        var fn = (DyFunction)self.Clone();
        var pars = new Par[fn.Parameters.Length];

        for (var i = 0; i < fn.Parameters.Length; i++)
        {
            var p = fn.Parameters[i];

            if (p.IsVarArg)
                continue;

            var val = p.Value;

            for (var j = 0; j < tup.Count; j++)
            {
                var lab = tv[j].GetLabel();

                if (p.Name == lab)
                    val = tv[j].GetTaggedValue();
            }

            pars[i] = new Par(p.Name, val, p.IsVarArg, p.TypeAnnotation);
        }

        fn.Parameters = pars;
        return fn;
    }

    protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Apply => Func.Member(name, Apply, 0, new Par("parameters")),
            Method.Compose => Func.Member(name, Compose, -1, new Par("other")),
            Method.Name => Func.Auto(name, GetName),
            Method.Parameters => Func.Auto(name, GetParameters),
            _ => base.InitializeInstanceMember(self, name, ctx)
        };

    private DyObject Compose(ExecutionContext ctx, DyObject first, DyObject second)
    {
        var f1 = first.ToFunction(ctx);
        if (f1 is null) return Default();
        var f2 = second.ToFunction(ctx);
        if (f2 is null) return Default();
        return Func.Compose(f1, f2);
    }

    protected override DyFunction? InitializeStaticMember(string name, ExecutionContext ctx) =>
        name switch
        {
            Method.Compose => Func.Static(name, Compose, -1, new Par("first"), new Par("second")),
            _ => base.InitializeStaticMember(name, ctx)
        };
}
