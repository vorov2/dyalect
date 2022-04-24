using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dyalect.Generators
{
    [Generator]
    public class DyTypeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var ep = context.Compilation.GetEntryPoint(context.CancellationToken);
            var type = context.Compilation.Assembly.GetTypeByMetadataName("Dyalect.DyType");
            var dyTypeInfos = new List<(int,string)>();

            if (type is not null)
            {
                string GetClassName(string name) =>
                    name switch
                    {
                        "TypeInfo" => "DyMetaTypeInfo",
                        "Interop" => "DyInteropObjectTypeInfo",
                        "Collection" => "DyCollTypeInfo",
                        _ => $"Dy{name}TypeInfo"
                    };

                var members = type.GetMembers();

                foreach (IFieldSymbol f in members.OfType<IFieldSymbol>().Where(m => m.IsConst))
                    dyTypeInfos.Add(((int)f.ConstantValue, f.Name));

                var dyTypes = dyTypeInfos.OrderBy(kv => kv.Item1).Select(kv => kv.Item2).ToArray();

                var source = $@"
using System;
using Dyalect.Runtime.Types;
namespace Dyalect;

partial class DyType
{{
    static partial void GetAllGenerated(FastList<DyTypeInfo> types)
    {{
        types.AddRange
        (
            new DyTypeInfo[] {{ null!, {string.Join(", ", dyTypes.Select(s => $"new {GetClassName(s)}()"))} }},
            0,
            {dyTypes.Length + 1}
        );
    }}

    static partial void GetTypeCodeByNameGenerated(string name, ref int code)
    {{
        code = name switch
        {{
            {string.Join(", ", dyTypes.Select(s => $"\"{s}\" => {s}"))},
            _ => default
        }};
    }}

static partial void GetTypeNameByCodeGenerated(int code, ref string name)
    {{
        name = code switch
        {{
            {string.Join(", ", dyTypes.Select(s => $"{s} => \"{s}\""))},
            _ => code.ToString()
        }};
    }}
}}";
                context.AddSource($"DyType.generated.cs", source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG_GENERATORS
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif 
        }
    }
}
