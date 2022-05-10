using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Dyalect.Generators
{
    [Generator]
    public class DyGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext ctx)
        {
            var syntaxReceiver = (DyTypeGeneratorSyntaxReceiver)ctx.SyntaxReceiver;

            if (syntaxReceiver.Class is null)
                return;

            var userClass = syntaxReceiver.Class;
            var dyTypes = userClass.Members
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables.Select(v => (ctx.Compilation.GetSemanticModel(f.SyntaxTree), v)))
                .Select(kv => kv.Item1.GetDeclaredSymbol(kv.v))
                .OfType<IFieldSymbol>()
                .Where(m => m.IsConst && m.ConstantValue is int)
                .Select(f => ((int)f.ConstantValue, f.Name, f.GetAttributes().Any(a => a.AttributeClass.ToString() == "Dyalect.Codegen.MixinAttribute")))
                .OrderBy(kv => kv.Item1)
                .Select(kv => (kv.Name, kv.Item3))
                .ToArray();

            static string GetClassName((string name, bool mixin) info) =>
                info.name switch
                {
                    "TypeInfo" => "new DyMetaTypeInfo()",
                    _ when info.mixin => $"Dy{info.name}TypeInfo.Instance",
                    _ => $"new Dy{info.name}TypeInfo()"
                };

            var source = $@"using System;
using Dyalect.Runtime.Types;
namespace Dyalect;

partial class Dy
{{
    static partial void GetAllGenerated(FastList<DyTypeInfo> types)
    {{
        types.AddRange
        (
            new DyTypeInfo[] {{ null!, {string.Join(", ", dyTypes.Select(GetClassName))} }},
            0,
            {dyTypes.Length + 1}
        );
    }}

    static partial void GetTypeCodeByNameGenerated(string name, ref int code)
    {{
        code = name switch
        {{
            {string.Join(", ", dyTypes.Select(s => $"\"{s.Name}\" => {s.Name}"))},
            _ => default
        }};
    }}

    static partial void GetTypeNameByCodeGenerated(int code, ref string name)
    {{
        name = code switch
        {{
            {string.Join(", ", dyTypes.Select(s => $"{s.Name} => \"{s.Name}\""))},
            _ => code.ToString()
        }};
    }}
}}";
            ctx.AddSource($"Dy.generated.cs", source);
        }

        public void Initialize(GeneratorInitializationContext ctx)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();

            ctx.RegisterForSyntaxNotifications(() => new DyTypeGeneratorSyntaxReceiver());
        }
    }

    public sealed class DyTypeGeneratorSyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax Class { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && cds.Identifier.ValueText == "Dy")
                Class = cds;
        }
    }
}
