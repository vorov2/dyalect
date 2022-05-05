using Dyalect.Codegen;
using Dyalect.Debug;
namespace Dyalect.Runtime.Types;

[GeneratedType]
internal sealed partial class DyFunctionTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Function);

    public override int ReflectedTypeId => Dy.Function;

    public DyFunctionTypeInfo() => AddMixins(Dy.Functor);

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString(arg.ToString());

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right) =>
        left.TypeId == right.TypeId && ((DyFunction)left).Equals((DyFunction)right) ? True : False;

    protected override DyObject AddOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right.TypeId is Dy.String)
            return base.AddOp(ctx, left, right);

        var f1 = left.ToFunction(ctx);
        if (ctx.HasErrors) return Nil;
        var f2 = right.ToFunction(ctx);
        if (ctx.HasErrors) return Nil;
        return new CompositionContainer(f1!, f2!);
    }

    internal override DyObject GetInstanceMember(DyObject self, HashString name, ExecutionContext ctx) =>
        name == "Call" ? self : base.GetInstanceMember(self, name, ctx);
    #endregion

    [InstanceMethod]
    internal static DyObject Apply(DyFunction self, [VarArg]DyTuple parameters)
    {
        var tv = parameters.UnsafeAccess();
        var fn = (DyFunction)self.Clone();
        var pars = new Par[fn.Parameters.Length];

        for (var i = 0; i < fn.Parameters.Length; i++)
        {
            var p = fn.Parameters[i];

            if (p.IsVarArg)
                continue;

            var val = p.Value;

            for (var j = 0; j < parameters.Count; j++)
            {
                if (tv[j] is DyLabel la && la.Label == p.Name)
                    val = la.Value;
            }

            pars[i] = new Par(p.Name, val, p.IsVarArg, p.TypeAnnotation);
        }

        fn.Parameters = pars;
        return fn;
    }

    [InstanceMethod]
    internal static DyObject Compose(DyFunction self, DyFunction other) => new CompositionContainer(self, other);

    [InstanceProperty("Object")]
    internal static DyObject GetObject(DyFunction self) => self.Self ?? Nil;

    [InstanceProperty("Name")]
    internal static string GetName(DyFunction self) => self.FunctionName;

    [InstanceProperty("Parameters")]
    internal static DyObject GetParameters(DyFunction self)
    {
        var arr = new DyObject[self.Parameters.Length];

        for (var i = 0; i < self.Parameters.Length; i++)
        {
            var p = self.Parameters[i];
            arr[i] = new DyTuple(
                    new DyLabel[] {
                        new("name", new DyString(p.Name)),
                        new("hasDefault", p.Value is not null ? True : False),
                        new("default", p.Value ?? Nil),
                        new("varArg", self.VarArgIndex == i ? True : False)
                    }
                );
        }

        return new DyArray(arr);
    }

    [StaticMethod("Compose")]
    internal static DyObject StaticCompose(DyFunction first, DyFunction second) => new CompositionContainer(first, second);
}
