using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
namespace Dyalect.Generators;

public abstract class SourceGenerator : ISourceGenerator
{
    internal static class Types
    {
        public const string Array = "System.Array";
        public const string ExecutionContext = "Dyalect.Runtime.ExecutionContext";
        public const string DyObject = "Dyalect.Runtime.Types.DyObject";
        public const string DyFunction = "Dyalect.Runtime.Types.DyFunction";
        public const string DyForeignFunction = "Dyalect.Runtime.Types.DyForeignFunction";
        public const string DyTuple = "Dyalect.Runtime.Types.DyTuple";
        public const string DyIterator = "Dyalect.Runtime.Types.DyIterator";
        public const string DyInteger = "Dyalect.Runtime.Types.DyInteger";
        public const string DyNil = "Dyalect.Runtime.Types.DyNil";
        public const string DyFloat = "Dyalect.Runtime.Types.DyFloat";
        public const string DyChar = "Dyalect.Runtime.Types.DyChar";
        public const string DyString = "Dyalect.Runtime.Types.DyString";
        public const string DyArray = "Dyalect.Runtime.Types.DyArray";
        public const string DyBool = "Dyalect.Runtime.Types.DyBool";
        public const string DyCollection = "Dyalect.Runtime.Types.DyCollection";
        public const string Par = "Dyalect.Debug.Par";
        public const string ParKind = "Dyalect.Debug.ParKind";
        public const string TypeConverter = "Dyalect.Runtime.TypeConverter";
        public const string InvalidCastException = "System.InvalidCastException";
        public const string DyType = "Dyalect.DyType";
        public const string FunAttr = "Dyalect.Compiler.FunAttr";
        public const string MethodAttribute = "Dyalect.Codegen.MethodAttribute";
        public const string StaticMethodAttribute = "Dyalect.Codegen.StaticMethodAttribute";
        public const string InstanceMethodAttribute = "Dyalect.Codegen.InstanceMethodAttribute";
        public const string InstancePropertyAttribute = "Dyalect.Codegen.InstancePropertyAttribute";
        public const string StaticPropertyAttribute = "Dyalect.Codegen.StaticPropertyAttribute";
    }

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
