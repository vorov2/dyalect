using Dyalect.Compiler;
using System;
using System.IO;
namespace Dyalect.Runtime.Types;

internal sealed class DyModuleTypeInfo : DyTypeInfo
{
    protected override SupportedOperations GetSupportedOperations() =>
        SupportedOperations.Eq | SupportedOperations.Neq | SupportedOperations.Not
        | SupportedOperations.Get | SupportedOperations.Len
        | SupportedOperations.Iter;

    public override string ReflectedTypeName => nameof(Dy.Module);

    public override int ReflectedTypeId => Dy.Module;

    public DyModuleTypeInfo() => AddMixin(Dy.Collection);

    #region Operations
    protected override DyObject ToStringOp(DyObject arg, DyObject format, ExecutionContext ctx) =>
        new DyString("{" + GetModuleName((DyModule)arg) + "}");

    private string GetModuleName(DyModule arg)
    {
        if (arg.Unit is Linker.Lang)
            return arg.Unit.FileName!;
        else if (arg.Unit is Linker.ForeignUnit)
        {
            var type = arg.Unit.GetType();
            var nam = Attribute.GetCustomAttribute(type,
                typeof(Linker.DyUnitAttribute)) is not Linker.DyUnitAttribute attr ? type.Name : attr.Name;
            return "foreign." + nam + "," + Path.GetFileNameWithoutExtension(arg.Unit.FileName);
        }
        else
            return "dyalect." + (arg.Unit.FileName is null ? "#memory#"
                : Path.GetFileNameWithoutExtension(arg.Unit.FileName));
    }

    protected override DyObject LengthOp(DyObject arg, ExecutionContext ctx)
    {
        var count = 0;

        foreach (var g in ((DyModule)arg).Unit.ExportList)
            if ((g.Value.Data & VarFlags.Private) != VarFlags.Private)
                count++;

        return DyInteger.Get(count);
    }

    protected override DyObject EqOp(DyObject left, DyObject right, ExecutionContext ctx)
    {
        if (right is DyModule mod)
            return ((DyModule)left).Unit.Id == mod.Unit.Id ? True : False;

        return False;
    }

    protected override DyObject GetOp(DyObject self, DyObject index, ExecutionContext ctx) => self.GetItem(index, ctx);

    protected override DyObject ContainsOp(DyObject self, DyObject field, ExecutionContext ctx)
    {
        if (!field.Is(ctx, Dy.String))
            return Nil;

        var mod = (DyModule)self;

        if (!mod.Unit.ExportList.TryGetValue(field.GetString(), out var sv))
            return False;

        return (sv.Data & VarFlags.Private) != VarFlags.Private ? True : False;
    }
    #endregion
}
