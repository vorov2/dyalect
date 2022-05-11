using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyContainerMixin : DyMixin<DyContainerMixin>
{
    public DyContainerMixin() : base(Dy.Container)
    {
        Members.Add(Builtins.In, Binary(Builtins.In, IsIn, "value"));
    }

    private static DyObject IsIn(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return False;

        return ((DyClass)self).Fields.GetOrdinal(field.ToString()) is not -1 ? True : False;
    }
}
