using Dyalect.Parser;
using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dyalect.Compiler
{
    //This part is responsible for the compilation logic of custom types
    partial class Builder
    {
        private void Build(DTypeDeclaration node, Hints hints, CompilerContext ctx)
        {
            var typeId = unit.TypeIds.Count;
            var unitId = unit.UnitIds.Count - 1;
            var ti = new TypeInfo(typeId, new UnitInfo(unitId, unit));

            if (localTypes.ContainsKey(node.Name))
            {
                localTypes.Remove(node.Name);
                AddError(CompilerError.TypeAlreadyDeclared, node.Location, node.Name);
            }

            localTypes.Add(node.Name, ti);
            types.Add(node.Name, ti);
            unit.TypeIds.Add(typeId);
            unit.TypeNames.Add(node.Name);
        }

        private TypeHandle GetTypeHandle(Qualident name, Location loc) => GetTypeHandle(name.Parent, name.Local, loc);

        private TypeHandle GetTypeHandle(string parent, string local, Location loc)
        {
            var err = GetTypeHandle(parent, local, out var handle, out var std);

            if (err == CompilerError.UndefinedModule)
                AddError(err, loc, parent);
            else if (err == CompilerError.UndefinedType)
                AddError(err, loc, local);

            return new TypeHandle(handle, std);
        }

        private CompilerError GetTypeHandle(string parent, string local, out int handle, out bool std)
        {
            handle = -1;
            std = false;

            if (parent == null)
                handle = DyType.GetTypeCodeByName(local);

            if (handle > -1)
            {
                std = true;
                return CompilerError.None;
            }

            if (parent == null)
            {
                if (!types.TryGetValue(local, out var ti))
                    return CompilerError.UndefinedType;
                else
                {
                    handle = ti.Unit.Handle | ti.TypeId << 8;
                    return CompilerError.None;
                }
            }
            else
            {
                if (!referencedUnits.TryGetValue(parent, out var ui))
                    return CompilerError.UndefinedModule;
                else
                {
                    var ti = -1;

                    for (var i = 0; i < ui.Unit.TypeIds.Count; ti++)
                    {
                        if (ui.Unit.TypeNames[i] == local)
                        {
                            ti = ui.Unit.TypeIds[i];
                            break;
                        }
                    }

                    if (ti == -1)
                        return CompilerError.UndefinedType;

                    handle = ui.Handle | ti << 8;
                    return CompilerError.None;
                }
            }
        }
    }
}
