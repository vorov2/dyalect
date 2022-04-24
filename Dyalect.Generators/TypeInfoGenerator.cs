using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dyalect.Generators
{
    [Generator]
    public class TypeInfoGenerator : ISourceGenerator
    {
        private readonly Dictionary<string, Func<string, string, string>> typeConversions = new()
        {
            { "long", (oname, name) => $"if ({oname}.TypeId is not DyType.Integer) {{ ctx.InvalidType(DyType.Integer, {oname}); return DyNil.Instance; }} var {name} = {oname}.GetInteger();" },
            { "int", (oname, name) => $"if ({oname}.TypeId is not DyType.Integer) {{ ctx.InvalidType(DyType.Integer, {oname}); return DyNil.Instance; }} var {name} = (int){oname}.GetInteger();" },
            { "int?", (oname, name) => $"if ({oname}.TypeId is not DyType.Integer and not DyType.Nil) {{ ctx.InvalidType(DyType.Integer, {oname}); return DyNil.Instance; }} var {name} = {oname}.TypeId == DyType.Nil ? null : (int?){oname}.GetInteger();" },
            { "string", (oname, name) => $"if ({oname}.TypeId is not DyType.String and not DyType.Char) {{ ctx.InvalidType(DyType.String, {oname}); return DyNil.Instance; }} var {name} = {oname}.GetString();" },
            { "char", (oname, name) => $"if ({oname}.TypeId is not DyType.Char) {{ ctx.InvalidType(DyType.String, {oname}); return DyNil.Instance; }} var {name.Substring(2)} = {oname}.GetChar();" },
            { "double", (oname, name) => $"if ({oname}.TypeId is not DyType.Integer and not DyType.Float) {{ ctx.InvalidType(DyType.Float, {oname}); return DyNil.Instance; }} var {name} = {oname}.GetFloat();" },
            { "float", (oname, name) => $"if ({oname}.TypeId is not DyType.Integer and not DyType.Float) {{ ctx.InvalidType(DyType.Float, {oname}); return DyNil.Instance; }} var {name} = (float){oname}.GetFloat();" },
            { "bool", (oname, name) => $"var {name} = !ReferenceEquals({oname}, DyBool.False);" }
        }; 
        
        private readonly Dictionary<string, Func<string, string>> returnTypeConversions = new()
        {
            { "long", name => $"return new DyInteger({name});" },
            { "int", name => $"return new DyInteger({name});" },
            { "string", name => $"return new DyString({name});" },
            { "char", name => $"return new DyChar({name});" },
            { "double", name => $"return new DyFloat({name});" },
            { "single", name => $"return new DyFloat({name});" },
            { "bool", name => $"return {name} ? DyBool.True : DyBool.False;" },
            { "System.Dyalect.Runtime.Types.DyObject", name => $"return {name};"}
        };

        private void TraverseNamespaces(INamespaceSymbol ns, List<INamedTypeSymbol> types)
        {
            foreach (var cns in ns.GetNamespaceMembers())
                TraverseNamespaces(cns, types);

            foreach (INamedTypeSymbol t in ns.GetMembers().OfType<INamedTypeSymbol>())
            {
                if (t.GetAttributes().Any(a => a.AttributeClass.Name == "GeneratedTypeAttribute"))
                    types.Add(t);
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var xs = new List<INamedTypeSymbol>();
            TraverseNamespaces(context.Compilation.GlobalNamespace, xs);
            var builder = new StringBuilder();

            foreach (var t in xs)
            {
                var instanceMethods = new List<string>();

                foreach (IMethodSymbol m in t.GetMembers().OfType<IMethodSymbol>())
                {
                    var attr = m.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == "InstanceMethodAttribute");

                    if (attr is null)
                        continue;

                    var methodNameData = attr.NamedArguments.FirstOrDefault(na => na.Key == "Name");

                    if (methodNameData.Value.Value is null || methodNameData.Value.Value is not string methodName)
                        continue;

                    builder.AppendLine($"internal sealed class {methodName}_WrapperFunction : Dyalect.Runtime.Types.DyForeignFunction");
                    builder.AppendLine("{");
                    builder.AppendLine($"internal override DyObject InternalCall(ExecutionContext ctx, DyObject[] args)");
                    builder.AppendLine("{");
                    var pars = new List<string>();

                    foreach (var mp in m.Parameters.Skip(2))
                        pars.Add(mp.Name);

                    var newPars = new List<string>();
                    
                    for (var i = 0; i < m.Parameters.Length; i++)
                    {
                        var mp = m.Parameters[i];

                        if (i == 0)
                            continue;

                        var dname = mp.Type.OriginalDefinition;

                        if (mp.Type.NullableAnnotation == NullableAnnotation.Annotated)
                        {

                            var name1 = ((INamedTypeSymbol)mp.Type).TypeArguments[0].MetadataName;
                            var name2 = ((INamedTypeSymbol)mp.Type).TypeArguments[0].OriginalDefinition;
                        }

                        if (typeConversions.TryGetValue(mp.Type.ToString(), out var conv1))
                            if (i == 1)
                                builder.AppendLine(conv1($"Self", mp.Name));
                            else
                                builder.AppendLine(conv1($"args[{i - 2}]", mp.Name));
                        
                        newPars.Add(mp.Name);
                    }

                    foreach (var r in m.OriginalDefinition.DeclaringSyntaxReferences)
                    {
                        var syntax = (MethodDeclarationSyntax)r.GetSyntax();
                        builder.Append($"{m.ReturnType.OriginalDefinition} __wrapper() ");

                        if (syntax.Body is not null)
                            builder.AppendLine(syntax.Body.ToString());
                        else
                            builder.AppendLine(syntax.ExpressionBody.ToFullString());
                    }

                    builder.AppendLine("var __ret = __wrapper();");

                    if (returnTypeConversions.TryGetValue(m.ReturnType.ToString(), out var conv2))
                        builder.AppendLine(conv2("__ret"));
                    else
                        builder.AppendLine("return Dyalect.Runtime.TypeConverter.ConvertFrom(__ret);");

                    builder.AppendLine("}");
                    builder.AppendLine($"public {methodName}_WrapperFunction() : base(\"{methodName}\", {string.Join(", ", pars.Select(p => $"new Par(\"{p}\")"))}, -1) {{}}");
                    builder.AppendLine($"protected override DyFunction Clone(ExecutionContext ctx) => new {methodName}_WrapperFunction();");
                    builder.AppendLine("}");

                    instanceMethods.Add(methodName);
                }

                if (instanceMethods.Count > 0)
                {
                    builder.AppendLine($@"
protected override DyFunction? InitializeInstanceMember(DyObject self, string name, ExecutionContext ctx) =>
name switch
{{");
                    foreach (var im in instanceMethods)
                    {
                        builder.AppendLine($"\"{im}\" => new {im}_WrapperFunction(),");
                    }
                    builder.AppendLine(@"_ => base.InitializeInstanceMember(self, name, ctx);
}};");
                }
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
