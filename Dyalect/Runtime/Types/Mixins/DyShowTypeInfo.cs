using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyShowTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Show);

    public override int ReflectedTypeId => Dy.Show;

    public DyShowTypeInfo()
    {
        Members.Add(Builtins.String, Unary(Builtins.String, Show));
    }

    private static DyObject Show(ExecutionContext ctx, DyObject arg)
    {
        var cust = (DyClass)arg;
        var priv = cust.Fields;

        if (arg.TypeName == cust.Constructor && priv.Count == 0)
            return new DyString($"{arg.TypeName}()");
        else if (arg.TypeName == cust.Constructor)
            return new DyString($"{arg.TypeName}{(priv.ToString(ctx))}");
        else if (priv.Count == 0)
            return new DyString($"{arg.TypeName}.{cust.Constructor}()");
        else
            return new DyString($"{arg.TypeName}.{cust.Constructor}{(priv.ToString(ctx))}");
    }
}
