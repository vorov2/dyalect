namespace Dyalect.Runtime.Types;

internal sealed class DyClassInfo : DyTypeInfo
{
    public override string ReflectedTypeName { get; }

    public override int ReflectedTypeId { get; }

    public DyClassInfo(string typeName, int typeCode)
    {
        ReflectedTypeName = typeName;
        ReflectedTypeId = typeCode;
    }

    protected override DyObject CastOp(ExecutionContext ctx, DyObject self, DyTypeInfo targetType) =>
        targetType.ReflectedTypeId switch
        {
            _ when targetType.ReflectedTypeId == self.TypeId => self,
            Dy.Dictionary => new DyDictionary(((DyClass)self).Fields.ConvertToDictionary()),
            Dy.Tuple => ((DyClass)self).Fields,
            Dy.Array => new DyArray(((DyClass)self).Fields.ToArray()),
            _ => base.CastOp(ctx, self, targetType)
        };
}
