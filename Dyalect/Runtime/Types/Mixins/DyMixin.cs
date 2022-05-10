namespace Dyalect.Runtime.Types;

public abstract class DyMixin<T> : DyTypeInfo
    where T : DyMixin<T>, new()
{
    public static T Instance { get; } = new T();

    public override string ReflectedTypeName { get; }

    public override int ReflectedTypeId { get; }

    protected DyMixin(int typeId)
    {
        ReflectedTypeId = typeId;
        ReflectedTypeName = Dy.GetTypeNameByCode(typeId);
        Closed = true;
    }
}
