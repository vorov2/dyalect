using Dyalect.Compiler;
using System.Collections.Generic;

namespace Dyalect.Runtime.Types;

internal sealed class DyShowMixin : DyMixin<DyShowMixin>
{
    public DyShowMixin() : base(Dy.Show) =>
        Members.Add(Builtins.String, Unary(Builtins.String, Show));

    private static DyObject Show(ExecutionContext ctx, DyObject arg)
    {
        var cust = (DyClass)arg;

        IEnumerable<DyObject> Iterate()
        {
            var xs = cust.Fields.UnsafeAccess();
            for (var i = 0; i < cust.Fields.Count; i++)
                yield return xs[i];
        }

        try
        {
            if (arg.TypeName == cust.Constructor && cust.Fields.Count == 0)
                return new DyString($"{arg.TypeName}()");
            else if (arg.TypeName == cust.Constructor)
                return new DyString($"{arg.TypeName}({(Iterate().ToLiteral(ctx))})");
            else if (cust.Fields.Count == 0)
                return new DyString($"{arg.TypeName}.{cust.Constructor}()");
            else
                return new DyString($"{arg.TypeName}.{cust.Constructor}({(Iterate().ToLiteral(ctx))})");
        }
        catch (DyCodeException ex)
        {
            ctx.Error = ex.Error;
            return Nil;
        }
    }
}
