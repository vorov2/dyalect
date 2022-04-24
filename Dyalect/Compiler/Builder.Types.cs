using Dyalect.Parser;
using Dyalect.Parser.Model;
using System.Collections.Generic;
using static Dyalect.Compiler.Hints;
namespace Dyalect.Compiler;

//This part is responsible for the compilation logic of custom types
partial class Builder
{
    private void Build(DTypeDeclaration node, Hints hints, CompilerContext ctx)
    {
        if (!char.IsUpper(node.Name[0]))
            AddError(CompilerError.TypeNameCamel, node.Location);

        if (currentScope != globalScope)
            AddError(CompilerError.TypesOnlyGlobalScope, node.Location);

        var unitId = unit.UnitIds.Count - 1;
        var ti = new TypeInfo(node, new(unitId, unit.ExportList));
        var typeVar = 0;

        if (types.ContainsKey(node.Name))
        {
            types.Remove(node.Name);
            AddError(CompilerError.TypeAlreadyDeclared, node.Location, node.Name);
        }
        else
        {
            typeVar = AddVariable(node.Name, node.Location, VarFlags.Type | VarFlags.Const);
            cw.NewType(node.Name);
            cw.PopVar(typeVar);
        }

        types.Add(node.Name, ti);
        var nh = hints.Remove(Push);

        foreach (var c in node.Constructors)
            Build(c, nh, ctx);

        if (node.Mixins is not null)
        {
            var set = new HashSet<int>();

            foreach (var m in node.Mixins)
            {
                if (m.Parent is null && m.Local == node.Name)
                    AddError(CompilerError.MixinSameAsType, node.Location, m.ToString());

                cw.PushVar(new ScopeVar(typeVar));
                var code = PushTypeInfo(ctx, m, node.Location);
                
                if (set.Contains(code))
                    AddError(CompilerError.MixinAlreadySpecified, node.Location, m.ToString());
                else
                    set.Add(code);

                if (code < 0)
                {
                    var abs = -code;

                    if (abs is not DyType.Collection and not DyType.Number and not DyType.Comparable)
                        AddError(CompilerError.InvalidMixin, node.Location, m.Local.ToString());
                }
            
                cw.Mixin();
            }
        }

        PushIf(hints);
    }

    private void GenerateConstructor(DFunctionDeclaration func, CompilerContext ctx)
    {
        if (func.Body is not null)
            Build(func.Body, NoScope, new());

        if (func.Parameters.Count == 0)
        {
            AddLinePragma(func);
            cw.NewTuple(0);
        }
        else if (func.Parameters.Count == 1)
        {
            var p = func.Parameters[0];
            PushVariable(ctx, p.Name, p.Location);
            cw.Tag(p.Name);

            if (p.TypeAnnotation is not null)
            {
                foreach (var q in p.TypeAnnotation)
                {
                    PushTypeInfo(ctx, q, p.Location);
                    cw.Annot();
                }
            }
            
            if (p is DTypeParameter { Mutable: true })
                cw.Mut();
            
            cw.NewTuple(1);
        }
        else
        {
            for (var i = 0; i < func.Parameters.Count; i++)
            {
                var p = func.Parameters[i];
                PushVariable(ctx, p.Name, p.Location);
                cw.Tag(p.Name);

                if (p.TypeAnnotation is not null)
                {
                    foreach (var q in p.TypeAnnotation)
                    {
                        PushTypeInfo(ctx, q, p.Location);
                        cw.Annot();
                    }
                }

                if (p is DTypeParameter { Mutable: true })
                    cw.Mut();
            }

            AddLinePragma(func);
            cw.NewTuple(func.Parameters.Count);
        }

        //Just because compiler wants it, it is always not null here
        if (func.TypeName is not null)
        {
            PushTypeInfo(ctx, func.TypeName, func.Location);
            cw.NewObj(func.Name!);
        }
    }

    private int PushTypeInfo(CompilerContext ctx, Qualident qual, Location loc)
    {
        if (qual.Parent is null) //Type is local
            return PushVariable(ctx, qual.Local, loc);
        else //Type is external
        {
            //Can't find module
            if (!referencedUnits.TryGetValue(qual.Parent, out var info))
            {
                AddError(CompilerError.UndefinedModule, loc, qual.Parent);
                return default;
            }

            //Push type from found module
            return PushTypeInfo(ctx, info, qual.Local, loc);
        }
    }

    private int PushTypeInfo(CompilerContext _, UnitInfo info, string name, Location loc)
    {
        //Can't find type in the module
        if (!info.ExportList.TryGetValue(name, out var sv))
        {
            AddError(CompilerError.UndefinedType, loc, name);
            return default;
        }

        //A type is declared inside a private block
        if ((sv.Data & VarFlags.Private) == VarFlags.Private)
            AddError(CompilerError.PrivateNameAccess, loc, name);

        cw.PushVar(new(info.Handle | (sv.Address >> 8) << 8, sv.Data | VarFlags.External));
        return info.Handle | (sv.Address >> 8) << 8;
    }

    private bool TryPushTypeInfo(UnitInfo info, string name, Location loc)
    {
        //Can't find type in the module
        if (!info.ExportList.TryGetValue(name, out var sv))
            return false;

        //A type is declared inside a private block
        if ((sv.Data & VarFlags.Private) == VarFlags.Private)
            AddError(CompilerError.PrivateNameAccess, loc, name);

        AddLinePragma(loc);
        cw.PushVar(new(info.Handle | (sv.Address >> 8) << 8, sv.Data | VarFlags.External));
        return true;
    }
}
