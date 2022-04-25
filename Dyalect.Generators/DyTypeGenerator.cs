using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Dyalect.Generators
{
    [Generator]
    public class DyTypeGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext ctx)
        {
            var syntaxReceiver = (DyTypeGeneratorSyntaxReceiver)ctx.SyntaxReceiver;
            var userClass = syntaxReceiver.Class;
            var dyTypes = userClass.Members
                .OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables.Select(v => (ctx.Compilation.GetSemanticModel(f.SyntaxTree), v)))
                .Select(kv => kv.Item1.GetDeclaredSymbol(kv.v))
                .OfType<IFieldSymbol>()
                .Where(m => m.IsConst && m.ConstantValue is int)
                .Select(f => ((int)f.ConstantValue, f.Name))
                .OrderBy(kv => kv.Item1)
                .Select(kv => kv.Name)
                .ToArray();

            static string GetClassName(string name) =>
                name switch
                {
                    "TypeInfo" => "DyMetaTypeInfo",
                    "Interop" => "DyInteropObjectTypeInfo",
                    "Collection" => "DyCollTypeInfo",
                    _ => $"Dy{name}TypeInfo"
                };

            var source = $@"using System;
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
            ctx.AddSource($"DyType.generated.cs", source);
        }

        public void Initialize(GeneratorInitializationContext ctx)
        {
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            ctx.RegisterForSyntaxNotifications(() => new DyTypeGeneratorSyntaxReceiver());
        }
    }

    public sealed class DyTypeGeneratorSyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax Class { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && cds.Identifier.ValueText == "DyType")
                Class = cds;
        }
    }
}
