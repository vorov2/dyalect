using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dyalect.Compiler
{
    //This part is responsible for linking referenced modules
    partial class Builder
    {
        private readonly Dictionary<string, Dictionary<string, Type>> assemblies = new();

        private Unit? LinkForeignModule(Unit self, Reference mod)
        {
            if (!lookup.Find(Path.GetDirectoryName(self.FileName), mod.DllName!, out var path))
                return null;

            path = path.Replace('\\', '/');
            
            if (!assemblies.TryGetValue(path!, out var dict))
                dict = LoadAssembly(path!, mod);

            if (dict is not null)
            {
                if (!dict.TryGetValue(mod.ModuleName, out var sysType))
                {
                    AddError(CompilerError.AssemblyModuleNotFound, mod.SourceFileName!, mod.SourceLocation,
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
                    AddError(CompilerError.AssemblyModuleLoadError, mod.SourceFileName!, mod.SourceLocation,
                        mod.ModuleName, mod.DllName!, ex.Message);
                    return null;
                }

                if (module is not Unit unit)
                {
                    AddError(CompilerError.InvalidAssemblyModule, mod.SourceFileName!, mod.SourceLocation,
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
                AddError(CompilerError.UnableLoadAssembly, mod.SourceFileName!, mod.SourceLocation,
                    mod.DllName!, ex.Message);
                return null;
            }

            var dict = new Dictionary<string, Type>();

            foreach (var t in asm.GetTypes())
            {
                var attr = Attribute.GetCustomAttribute(t, typeof(DyUnitAttribute)) as DyUnitAttribute;

                if (attr is null)
                    continue;

                if (dict.ContainsKey(attr.Name))
                    AddError(CompilerError.DuplicateModuleName, mod.SourceFileName!, mod.SourceLocation,
                        mod.DllName!, attr.Name);
                else
                    dict.Add(attr.Name, t);
            }

            assemblies.Add(path, dict);
            return dict;
        }
    }
}
