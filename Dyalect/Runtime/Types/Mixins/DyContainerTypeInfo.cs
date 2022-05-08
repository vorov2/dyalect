using Dyalect.Compiler;
namespace Dyalect.Runtime.Types;

internal class DyContainerTypeInfo : DyMixin
{
    public override string ReflectedTypeName => nameof(Dy.Container);

    public override int ReflectedTypeId => Dy.Container;

    public DyContainerTypeInfo()
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
