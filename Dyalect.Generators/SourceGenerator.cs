using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Generators;

public abstract class SourceGenerator : ISourceGenerator
{
    public virtual void Initialize(GeneratorInitializationContext context) { }

    public abstract void Execute(GeneratorExecutionContext context);

    protected List<(string attr, INamedTypeSymbol type)> FindTypesByAttributes(GeneratorExecutionContext ctx, params string[] names)
    {
        var types = new List<(string, INamedTypeSymbol)>();
        FindTypesByAttributes(ctx, ctx.Compilation.GlobalNamespace, types, names);
        return types;
    }

    private void FindTypesByAttributes(GeneratorExecutionContext ctx, INamespaceSymbol ns, List<(string, INamedTypeSymbol)> types, params string[] names)
    {
        foreach (var cns in ns.GetNamespaceMembers())
            FindTypesByAttributes(ctx, cns, types, names);

        foreach (INamedTypeSymbol t in ns.GetMembers().OfType<INamedTypeSymbol>())
        {
            if (t.ContainingAssembly.Name != ctx.Compilation.AssemblyName)
                continue;

            var attrs = t.GetAttributes();

            foreach (var name in names)
                if (attrs.Any(a => a.AttributeClass.Name == name))
                    types.Add((name, t));
        }
    }

    protected bool CheckBaseType(ITypeSymbol type, string fullName)
    {
        var bas = type.BaseType;

        if (bas is null)
            return false;

        if (bas.ToString() == fullName)
            return true;

        return CheckBaseType(bas, fullName);
    }

    protected bool IsDyObject(ITypeSymbol type) => CheckBaseType(type, Types.DyObject);

    protected void Warning(GeneratorExecutionContext ctx, string text) =>
        ReportMessage(ctx, DiagnosticSeverity.Warning, text);

    protected bool Error(GeneratorExecutionContext ctx, string text)
    {
        ReportMessage(ctx, DiagnosticSeverity.Error, text);
        return false;
    }

    private void ReportMessage(GeneratorExecutionContext ctx, DiagnosticSeverity severity, string text)
    {
        ctx.ReportDiagnostic(Diagnostic.Create($"Dy0001", "Dyalect.Generator", text,
            severity, severity, true, severity == DiagnosticSeverity.Warning ? 1 : 0, false));
    }
}
