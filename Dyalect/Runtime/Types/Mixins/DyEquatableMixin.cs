using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyEquatableMixin : DyMixin<DyEquatableMixin>
{
    public DyEquatableMixin() : base(Dy.Equatable)
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
