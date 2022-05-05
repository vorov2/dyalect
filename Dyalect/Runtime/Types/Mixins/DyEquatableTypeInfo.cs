using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyEquatableTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Equatable);

    public override int ReflectedTypeId => Dy.Equatable;

    public DyEquatableTypeInfo()
    {
        Members.Add(Builtins.Eq, Binary(Builtins.Eq, Equatable));
    }

    private static DyObject Equatable(ExecutionContext ctx, DyObject left, DyObject right)
    {
        var self = (DyClass)left;

        if (self.TypeId == right.TypeId && right is DyClass t && t.Constructor == self.Constructor)
        {
            try
            {
                return DyTuple.Equals(ctx, self.Fields, t.Fields);
            }
            catch (DyCodeException ex)
            {
                ctx.Error = ex.Error;
                return Nil;
            }
        }

        return False;
    }
}
