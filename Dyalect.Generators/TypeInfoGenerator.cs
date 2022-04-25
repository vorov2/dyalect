using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dyalect.Generators
{
    public static class Extensions
    {
        public static string ToString(this IEnumerable<string> elements, string mask, string separator = ", ") =>
            string.Join(separator, elements.Select(s => string.Format(mask, s)));
        
        public static string ToString<T>(this IEnumerable<T> elements, string mask, Func<T, string> selector, string separator = ", ") =>
            string.Join(separator, elements.Select(obj => string.Format(mask, selector(obj))));
    }

    internal enum ParFlags
    {
        None,
        Nullable = 0x01,
        VarArg = 0x02
    }

    internal enum MethodFlags
    {
        None,
        Static = 0x01,
        Property = 0x02
    }


    [Generator]
    public class TypeInfoGenerator : SourceGenerator
    {
        private static readonly object nil = new();
        private static readonly string[] neverNulls = new[] { "int", "long", "float", "double", "char", "bool" };

        private static void ParamCheck(SourceBuilder sb, string input, string par, string conversion, ParFlags flags, params string[] dyTypes)
        {
            if (dyTypes.Length > 0)
            {
                sb.AppendPadding();
                sb.Append($"if ({input}.TypeId is ");

                for (var i = 0; i < dyTypes.Length; i++)
                {
                    if (i > 0)
                        sb.Append(" and ");

                    sb.Append($"not {Types.DyType}.{dyTypes[i]}");
                }

                if ((flags & ParFlags.Nullable) == ParFlags.Nullable)
                    sb.Append($" and not {Types.DyType}.Nil");

                sb.Append($") return ctx.InvalidType({dyTypes.ToString("DyType.{0}")}, {input});");
                sb.AppendLine();
            }

            if ((flags & ParFlags.Nullable) == ParFlags.Nullable)
                sb.AppendLine($"var {par} = {input}.TypeId == {Types.DyType}.Nil ? null : {string.Format(conversion, input)};");
            else
                sb.AppendLine($"var {par} = {string.Format(conversion, input)};");
        }

        private bool ParamCheckArray(string input, string par, ParFlags flags, ITypeSymbol ti, SourceBuilder sb)
        {
            var ati = (IArrayTypeSymbol)ti;
            var getValues = $"(({Types.DyTuple}){input}).GetValues();";

            if ((flags & ParFlags.VarArg) != ParFlags.VarArg)
                getValues = $"{Types.DyIterator}.ToEnumerable(ctx, {input}).ToArray(); if (ctx.HasErrors) return DyNil.Instance;";

            if (ati.ElementType.ToString() == Types.DyObject)
            {
                sb.AppendLine($"var {par} = {getValues}");
                return true;
            }
            else if (IsDyObject(ati.ElementType))
            {
                sb.AppendLine($"var __{par} = {getValues}");
                sb.AppendLine($"var {par} = new {ati.ElementType}[__{par}.Length];");
                sb.AppendLine("try");
                sb.AppendLine("{");
                sb.Indent();
                sb.AppendLine($"for (var __index = 0; __index < __{par}.Length; __index++)");
                sb.AppendInBlock($"{par}[__index] = ({ati.ElementType})__{par}[__index];");
                sb.Outdent();
                sb.AppendLine("}");
                sb.AppendLine($"catch ({Types.InvalidCastException})"); 
                sb.AppendInBlock($"return ctx.InvalidType({input});");
                return true;
            }

            return false;
        }

        private readonly Dictionary<string, Action<SourceBuilder, string, string, ParFlags, ITypeSymbol>> typeConversions = new()
        {
            { "long", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "{0}.GetInteger()", flag, "Integer") },
            { "long?", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(long?){0}.GetInteger()", flag, "Integer") },
            { "int", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(int){0}.GetInteger()", flag, "Integer") },
            { "int?", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(int?){0}.GetInteger()", flag, "Integer") },
            { "double", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "{0}.GetFloat()", flag, "Float") },
            { "double?", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(double?){0}.GetFloat()", flag, "Float") },
            { "float", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(float){0}.GetFloat()", flag, "Float") },
            { "float?", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "(float?){0}.GetFloat()", flag, "Float") },
            { "string", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "{0}.GetString()", flag, "String", "Char") },
            { "char", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "{0}.GetChar()", flag, "Char", "String") },
            { "bool", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, $"!ReferenceEquals({{0}}, {Types.DyBool}.False) && !ReferenceEquals({{0}}, {Types.DyNil}.Instance)", flag) },
            { "bool?", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, $"(bool?)!ReferenceEquals({{0}}, {Types.DyBool}.False) && !ReferenceEquals({{0}}, {Types.DyNil}.Instance)", flag) },
            { $"{Types.DyObject}", (sb, oname, name, flag, ti) => ParamCheck(sb, oname, name, "{0}", flag) },
        }; 
        
        private readonly Dictionary<string, Func<string, string>> returnTypeConversions = new()
        {
            { "long", name => $"return new {Types.DyInteger}({name});" },
            { "long?", name => $"return {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : new {Types.DyInteger}({name});" },
            { "int", name => $"return new {Types.DyInteger}({name});" },
            { "int?", name => $"return new {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : {Types.DyInteger}({name});" },
            { "string", name => $"return new {Types.DyString}({name});" },
            { "char", name => $"return new {Types.DyChar}({name});" },
            { "char?", name => $"return new {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : {Types.DyChar}({name});" },
            { "double", name => $"return new {Types.DyFloat}({name});" },
            { "double?", name => $"return new {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : {Types.DyFloat}({name});" },
            { "single", name => $"return new {Types.DyFloat}({name});" },
            { "single?", name => $"return new {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : {Types.DyFloat}({name});" },
            { "bool", name => $"return {name} ? {Types.DyBool}.True : {Types.DyBool}.False;" },
            { "bool?", name => $"return {name} is null ? ({Types.DyObject}){Types.DyNil}.Instance : ({name} ? {Types.DyBool}.True : {Types.DyBool}.False);" },
            { $"{Types.DyObject}", name => $"return {name};"}
        };

        public override void Execute(GeneratorExecutionContext ctx)
        {
            var xs = FindTypesByAttributes(ctx.Compilation.GlobalNamespace, "GeneratedTypeAttribute");
            
            foreach (var (_, t) in xs)
            {
                var builder = new SourceBuilder();
                var unit = t.DeclaringSyntaxReferences.FirstOrDefault().SyntaxTree.GetRoot() as CompilationUnitSyntax;

                if (unit is not null)
                {
                    foreach (var us in unit.Usings)
                        builder.Append(us.ToFullString());
                }

                builder.AppendLine($"namespace {t.ContainingNamespace};");
                builder.AppendLine();
                var methods = new List<(string name, bool instance)>();

                foreach (IMethodSymbol m in t.GetMembers().OfType<IMethodSymbol>())
                    ProcessMethod(ctx, t, m, builder, methods);

                if (methods.Count > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine($"partial class {t.Name}");
                    builder.StartBlock();

                    builder.AppendLine($"protected override {Types.DyFunction} InitializeInstanceMember({Types.DyObject} self, string name, {Types.ExecutionContext} ctx) =>");
                    builder.Indent();
                    builder.AppendLine("name switch");
                    builder.StartBlock();
                    foreach (var (im,_) in methods.Where(kv => kv.instance))
                        builder.AppendLine($"\"{im}\" => new {t.Name}_{im}_WrapperFunction(),");
                    builder.AppendLine("_ => base.InitializeInstanceMember(self, name, ctx)");
                    builder.Outdent();
                    builder.AppendLine("};");
                    builder.Outdent();

                    builder.AppendLine($"protected override {Types.DyFunction} InitializeStaticMember(string name, {Types.ExecutionContext} ctx) =>");
                    builder.Indent();
                    builder.AppendLine("name switch");
                    builder.StartBlock();
                    foreach (var (im,_) in methods.Where(kv => !kv.instance))
                        builder.AppendLine($"\"{im}\" => new {t.Name}_{im}_WrapperFunction(),");
                    builder.AppendLine("_ => base.InitializeStaticMember(name, ctx)");
                    builder.Outdent();
                    builder.AppendLine("};");
                    builder.Outdent();

                    builder.EndBlock();
                }

                System.IO.File.WriteAllText($"C:\\temp\\{t.Name}.generated.{DateTime.Now.Ticks}.cs", builder.ToString());
                ctx.AddSource($"{t.Name}.generated.cs", builder.ToString());
            }
        }

        private void ProcessMethod(GeneratorExecutionContext ctx, INamedTypeSymbol t, IMethodSymbol m, SourceBuilder builder, List<(string name, bool instance)> methods)
        {
            var attr = m.GetAttributes().FirstOrDefault(a => a.AttributeClass.BaseType.ToString() == Types.MethodAttribute);

            if (attr is not null)
            {
                var methodNameData = attr.ConstructorArguments.Length == 1 ? attr.ConstructorArguments[0].Value : null;

                if (methodNameData is null || methodNameData is not string methodName)
                    methodName = m.Name;

                var isInstance = attr.AttributeClass.ToString() == Types.InstanceMethodAttribute;
                var isProperty = attr.AttributeClass.ToString() is Types.InstancePropertyAttribute or Types.StaticPropertyAttribute;
                var flags = (!isInstance ? MethodFlags.Static : MethodFlags.None) | (isProperty ? MethodFlags.Property : MethodFlags.None);

                if (ProcessMethod(ctx, t, m, builder, methodName, flags))
                    methods.Add((methodName, isInstance));
            }
        }

        private bool ProcessMethod(GeneratorExecutionContext ctx, INamedTypeSymbol t, IMethodSymbol m, SourceBuilder builder, string methodName, MethodFlags implFlags)
        {
            var hasContext = m.Parameters.Length > 0 && m.Parameters[0].Type.ToString() == Types.ExecutionContext;
            var isStatic = (implFlags & MethodFlags.Static) == MethodFlags.Static;

            builder.AppendLine($"internal sealed class {t.Name}_{methodName}_WrapperFunction : {Types.DyForeignFunction}");
            builder.StartBlock();
            builder.AppendLine($"internal override {Types.DyObject} InternalCall({Types.ExecutionContext} ctx, {Types.DyObject}[] args)");
            builder.StartBlock();
            var pars = new List<(string name, object def, bool vararg)>();
            var varArgIndex = -1;

            for (var i = 0; i < m.Parameters.Length; i++)
            {
                var mp = m.Parameters[i];

                if (hasContext && i == 0) //First parameter is a context
                {
                    if (mp.Name != "ctx")
                        builder.AppendLine($"var {mp.Name} = ctx;");
                    continue;
                }

                var attrs = mp.GetAttributes();
                var vararg = attrs.Any(a => a.AttributeClass.Name == "VarArgAttribute");
                var def = attrs.Where(a => a.AttributeClass.Name == "DefaultAttribute")
                    .Select(a => a.ConstructorArguments.Length > 0 ? a.ConstructorArguments[0].Value : nil)
                    .FirstOrDefault();
                var nullable = ReferenceEquals(def, nil);
                var typeName = mp.Type.ToString();

                if (mp.Type.NullableAnnotation == NullableAnnotation.Annotated && mp.Type.IsReferenceType)
                    typeName = mp.Type.OriginalDefinition.ToString();

                var indexShift = 0 - (hasContext ? 1 : 0) - (isStatic ? 0 : 1);

                if (vararg)
                    varArgIndex = i + indexShift;

                if (nullable && neverNulls.Contains(typeName))
                    Error(ctx, $"Type \"{typeName}\" is not nullable.");

                var firstParam = i == 1 && hasContext || i == 0 && !hasContext;
                var sourceParName = firstParam && !isStatic ? "Self" : $"args[{i + indexShift}]";

                if (isStatic || !firstParam)
                    pars.Add((mp.Name, def, vararg));

                var flags = vararg ? ParFlags.VarArg : ParFlags.None;
                if (nullable) flags |= ParFlags.Nullable;

                if (typeConversions.TryGetValue(typeName, out var conv1))
                    conv1(builder, sourceParName, mp.Name, flags, mp.Type);
                else if (mp.Type is IArrayTypeSymbol arr)
                {
                    var elementType = arr.ElementType;
                    var res = ParamCheckArray(sourceParName, mp.Name, flags, mp.Type, builder);
                    
                    if (!res)
                        Error(ctx, $"Parameter type \"{typeName}\" is not supported.");
                }
                else if (IsDyObject(mp.Type))
                    ConvertToDyObject(builder, typeName, sourceParName, mp.Name);
                else
                    Error(ctx, $"Parameter type \"{typeName}\" is not supported.");
            }

            foreach (var r in m.OriginalDefinition.DeclaringSyntaxReferences)
            {
                var syntax = (MethodDeclarationSyntax)r.GetSyntax();
                builder.AppendPadding();
                builder.Append($"{m.ReturnType.OriginalDefinition} __wrapper() ");

                if (syntax.Body is not null)
                    builder.Append(syntax.Body.ToString());
                else
                    builder.Append(syntax.ExpressionBody.ToFullString() + ";");

                builder.AppendLine();
            }

            if (m.ReturnType is not null)
            {
                builder.AppendLine("var __ret = __wrapper();");

                if (returnTypeConversions.TryGetValue(m.ReturnType.ToString(), out var conv2))
                    builder.AppendLine(conv2("__ret"));
                else if (IsDyObject(m.ReturnType))
                    builder.AppendLine("return __ret;"); //ConvertToDyObject(builder, m.ReturnType.ToString(), "__ret");
                else
                    Error(ctx, $"Return type \"{m.ReturnType}\" is not supported.");
            }
            else
            {
                builder.AppendLine("__wrapper();");
                builder.AppendLine($"return {Types.DyNil}.Instance;");
            }

            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine($"public {t.Name}_{methodName}_WrapperFunction() : base(\"{methodName}\", new {Types.Par}[]{{{pars.ToString($"new {Types.Par}({{0}})", p => $"\"{p.name}\"{(p.vararg ? $", {Types.ParKind}.VarArg" : "")}{(p.def is not null ? $", {(ReferenceEquals(p.def, nil) ? $"{Types.DyNil}.Instance" : p.def)}" : "")}")}}}, {varArgIndex})");
            builder.StartBlock();

            if ((implFlags & MethodFlags.Property) == MethodFlags.Property)
                builder.AppendLine($"Attr |= {Types.FunAttr}.Auto;");

            builder.EndBlock();
            builder.AppendLine();
            builder.AppendLine($"protected override {Types.DyFunction} Clone({Types.ExecutionContext} ctx) => new {t.Name}_{methodName}_WrapperFunction();");
            builder.EndBlock();

            return true;
        }

        private void ConvertToDyObject(SourceBuilder builder, string targetType, string oldVar, string newVar = "return")
        {
            if (newVar != "return")
                builder.AppendLine($"{targetType} {newVar};");

            builder.AppendLine("try");
            
            if (newVar != "return")
                builder.AppendInBlock($"{newVar} = ({targetType}){oldVar};");
            else
                builder.AppendInBlock($"return ({targetType}){oldVar};");

            builder.AppendLine($"catch (System.InvalidCastException)");
            builder.AppendInBlock($"return ctx.InvalidType({oldVar});");
        }
    }
}
