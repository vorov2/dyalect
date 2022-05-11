using Dyalect.Compiler;
using System.Collections.Generic;
using System.IO;
namespace Dyalect.Runtime.Types;

internal sealed class DyModuleTypeInfo : DyTypeInfo
{
    public override string ReflectedTypeName => nameof(Dy.Module);

    public override int ReflectedTypeId => Dy.Module;

    public DyModuleTypeInfo()
    {
        AddMixins(Dy.Lookup, Dy.Sequence, Dy.Container);
    }

    #region Operations
    protected override DyObject ToStringOp(ExecutionContext ctx, DyObject arg, DyObject format) =>
        new DyString("<" + GetModuleName((DyModule)arg) + ">");

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

    protected override DyObject IterateOp(ExecutionContext ctx, DyObject self) =>
        DyIterator.Create((IEnumerable<DyObject>)self);

    protected override DyObject LengthOp(ExecutionContext ctx, DyObject arg)
    {
        var count = 0;

        foreach (var g in ((DyModule)arg).Unit.ExportList)
            if ((g.Value.Data & VarFlags.Private) != VarFlags.Private)
                count++;

        return DyInteger.Get(count);
    }

    protected override DyObject EqOp(ExecutionContext ctx, DyObject left, DyObject right)
    {
        if (right is DyModule mod)
            return ((DyModule)left).Unit.Id == mod.Unit.Id ? True : False;

        return False;
    }

    protected override DyObject GetOp(ExecutionContext ctx, DyObject self, DyObject index) => ((DyModule)self).GetMember(ctx, index);

    protected override DyObject InOp(ExecutionContext ctx, DyObject self, DyObject field)
    {
        if (field.TypeId is not Dy.String and not Dy.Char)
            return Nil;

        var mod = (DyModule)self;

        if (!mod.Unit.ExportList.TryGetValue(field.ToString(), out var sv))
            return False;

        return (sv.Data & VarFlags.Private) != VarFlags.Private ? True : False;
    }
    #endregion
}
