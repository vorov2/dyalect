﻿using Dyalect.Compiler;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Linker
{
    partial class DyLinker
    {
        private Unit? LinkForeignModule(Unit self, Reference mod)
        {
            if (!FindModuleExact(self.FileName!, mod.DllName!, mod, out var path))
                return null;

            if (!AssemblyMap.TryGetValue(path!, out var dict))
                dict = LoadAssembly(path!, mod);

            if (dict is not null)
            {
                if (!dict.TryGetValue(mod.ModuleName, out var sysType))
                {
                    AddError(LinkerError.AssemblyModuleNotFound, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!);
                    return null;
                }

                object module;

                try
                {
                    module = Activator.CreateInstance(sysType)!;
                }
                catch (Exception ex)
                {
                    AddError(LinkerError.AssemblyModuleLoadError, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!, ex.Message);
                    return null;
                }

                if (module is not Unit unit)
                {
                    AddError(LinkerError.InvalidAssemblyModule, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!);
                    return null;
                }

                unit.FileName = path;
                return unit;
            }

            return null;
        }

        private Dictionary<string, Type>? LoadAssembly(string path, Reference mod)
        {
            Assembly asm;

            try
            {
                asm = Assembly.LoadFrom(path);
            }
            catch (Exception ex)
            {
                AddError(LinkerError.UnableLoadAssembly, mod.SourceFileName!, mod.SourceLocation,
                    mod.DllName!, ex.Message);
                return null;
            }

            var dict = new Dictionary<string, Type>();

            foreach (var t in asm.GetTypes())
            {
                if (Attribute.GetCustomAttribute(t, typeof(DyUnitAttribute)) is not DyUnitAttribute attr)
                    continue;
                
                if (dict.ContainsKey(attr.Name))
                    AddError(LinkerError.DuplicateModuleName, mod.SourceFileName!, mod.SourceLocation,
                        mod.DllName!, attr.Name);
                else
                    dict.Add(attr.Name, t);
            }

            AssemblyMap.Add(path, dict);
            return dict;
        }
    }
}
