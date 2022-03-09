using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    partial class Builder
    {
        //The main compilation switch that processes all types of AST nodes
        private void Build(DNode node, Hints hints, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
                case NodeType.PrivateScope:
                    Build((DPrivateScope)node, hints, ctx);
                    break;
                case NodeType.Directive:
                    Build((DDirective)node, hints);
                    break;
                case NodeType.Assignment:
                    Build((DAssignment)node, hints, ctx);
                    break;
                case NodeType.Binary:
                    Build((DBinaryOperation)node, hints, ctx);
                    break;
                case NodeType.Binding:
                    Build((DBinding)node, hints, ctx);
                    break;
                case NodeType.Rebinding:
                    Build((DRebinding)node, hints, ctx);
                    break;
                case NodeType.Block:
                    Build((DBlock)node, hints, ctx);
                    break;
                case NodeType.Boolean:
                    Build((DBooleanLiteral)node, hints);
                    break;
                case NodeType.Float:
                    Build((DFloatLiteral)node, hints);
                    break;
                case NodeType.If:
                    Build((DIf)node, hints, ctx);
                    break;
                case NodeType.Integer:
                    Build((DIntegerLiteral)node, hints);
                    break;
                case NodeType.Name:
                    Build((DName)node, hints, ctx);
                    break;
                case NodeType.String:
                    Build((DStringLiteral)node, hints, ctx);
                    break;
                case NodeType.Nil:
                    Build((DNilLiteral)node, hints);
                    break;
                case NodeType.Unary:
                    Build((DUnaryOperation)node, hints, ctx);
                    break;
                case NodeType.Application:
                    Build((DApplication)node, hints, ctx);
                    break;
                case NodeType.While:
                    Build((DWhile)node, hints, ctx);
                    break;
                case NodeType.Break:
                    Build((DBreak)node, hints, ctx);
                    break;
                case NodeType.Return:
                    Build((DReturn)node, hints, ctx);
                    break;
                case NodeType.Continue:
                    Build((DContinue)node, hints, ctx);
                    break;
                case NodeType.Function:
                    Build((DFunctionDeclaration)node, hints, ctx);
                    break;
                case NodeType.Index:
                    Build((DIndexer)node, hints, ctx);
                    break;
                case NodeType.Tuple:
                    Build((DTupleLiteral)node, hints, ctx);
                    break;
                case NodeType.Array:
                    Build((DArrayLiteral)node, hints, ctx);
                    break;
                case NodeType.Access:
                    Build((DAccess)node, hints, ctx);
                    break;
                case NodeType.For:
                    Build((DFor)node, hints, ctx);
                    break;
                case NodeType.Yield:
                    Build((DYield)node, hints, ctx);
                    break;
                case NodeType.YieldMany:
                    Build((DYieldMany)node, hints, ctx);
                    break;
                case NodeType.YieldBreak:
                    Build((DYieldBreak)node, hints, ctx);
                    break;
                case NodeType.Base:
                    Build((DBase)node);
                    break;
                case NodeType.Char:
                    Build((DCharLiteral)node, hints);
                    break;
                case NodeType.Range:
                    Build((DRange)node, hints, ctx);
                    break;
                case NodeType.Match:
                    Build((DMatch)node, hints, ctx);
                    break;
                case NodeType.Iterator:
                    Build((DIteratorLiteral)node, hints, ctx);
                    break;
                case NodeType.YieldBlock:
                    Build((DYieldBlock)node, hints, ctx);
                    break;
                case NodeType.TryCatch:
                    Build((DTryCatch)node, hints, ctx);
                    break;
                case NodeType.Throw:
                    Build((DThrow)node, hints, ctx);
                    break;
                case NodeType.Type:
                    Build((DTypeDeclaration)node, hints, ctx);
                    break;
                case NodeType.Label:
                    AddError(CompilerError.InvalidLabel, node.Location);
                    break;
                case NodeType.RecursiveBlock:
                    Build((DRecursiveBlock)node, hints, ctx);
                    break;
                case NodeType.TestBlock:
                    break;
            }
        }

        private void Build(DRecursiveBlock node, Hints hints, CompilerContext ctx)
        {
            if (node.Functions[0].TypeName is not null)
                AddError(CompilerError.MethodNotRecursive, node.Location);

            for (var i = 0; i < node.Functions.Count; i++)
                AddVariable(node.Functions[i].Name!, node.Functions[i].Location, VarFlags.PreInit | VarFlags.Const | VarFlags.Function);

            for (var i = 0; i < node.Functions.Count; i++)
            {
                var last = i == node.Functions.Count - 1;
                Build(node.Functions[i], last ? hints: hints.Remove(Push), ctx);
            }
        }

        private void Build(DPrivateScope node, Hints hints, CompilerContext ctx)
        {
            if (privateScope)
                AddError(CompilerError.PrivateScopeNested, node.Location);

            if (currentScope != globalScope)
                AddError(CompilerError.PrivateScopeOnlyGlobal, node.Location);

            privateScope = true;
            Build(node.Block, hints.Append(NoScope), ctx);
            privateScope = false;
        }

        private void Build(DDirective node, Hints hints)
        {
            switch (node.Key)
            {
                case "warning":
                    {
                        if (node.Attributes.Count < 2)
                        {
                            AddError(CompilerError.InvalidDirective, node.Location, node.Key);
                            return;
                        }

                        var pragma = node.Attributes[0] as string;

                        switch (pragma)
                        {
                            case "disable":
                                foreach (var i in node.Attributes.Skip(1).OfType<long>())
                                    if (!disabledWarnings.Contains((int)i))
                                        disabledWarnings.Add((int)i);
                                break;
                            case "enable":
                                foreach (var i in node.Attributes.Skip(1).OfType<long>())
                                {
                                    disabledWarnings.Remove((int)i);
                                    enabledWarnings.Add((int)i);
                                }
                                break;
                            case "report":
                                {
                                    var msg = node.Attributes.Skip(1).Take(1).FirstOrDefault();
                                    AddWarning(CompilerWarning.UserWarning, node.Location, msg ?? "");
                                }
                                break;
                        }
                    }
                    break;
                case "optimizer":
                    {
                        if (node.Attributes.Count < 1)
                        {
                            AddError(CompilerError.InvalidDirective, node.Location, node.Key);
                            return;
                        }

                        var pragma = node.Attributes[0] as string;

                        switch (pragma)
                        {
                            case "enable": options.NoOptimizations = false; break;
                            case "disable": options.NoOptimizations = true; break;
                        }
                    }
                    break;
                default:
                    AddError(CompilerError.UnknownDirective, node.Location, node.Key);
                    break;
            }

            PushIf(hints);
        }

        private void Build(DThrow node, Hints hints, CompilerContext ctx)
        {
            if (node.Expression is not null)
            {
                Build(node.Expression, hints.Append(Push), ctx);
                AddLinePragma(node);
                cw.Fail();
            }
            else
            {
                if (!hints.Has(Catch) || ctx.Errors.Count is 0)
                    AddError(CompilerError.InvalidRethrow, node.Location);
                else
                    cw.PushVar(new ScopeVar(ctx.Errors.Peek()));

                AddLinePragma(node);
                cw.Fail();
            }
        }

        private void Build(DTryCatch node, Hints hints, CompilerContext ctx)
        {
            var gotcha = cw.DefineLabel();
            cw.Start(gotcha);
            Build(node.Expression, hints, ctx);

            AddLinePragma(node);
            cw.End();
            var skip = cw.DefineLabel();
            cw.Br(skip);
            cw.MarkLabel(gotcha);

            StartScope(ScopeKind.Lexical, node.Catch.Location);
            //We are inside a catch, we need preserve an exception object for
            //possible rethrows
            cw.Dup();
            var a = AddVariable();
            cw.PopVar(a);
            ctx.Errors.Push(a);

            if (node.BindVariable is not null)
            {
                AddLinePragma(node.BindVariable);

                if (node.BindVariable.Value is not "_")
                {
                    var sv = AddVariable(node.BindVariable.Value, node.Catch.Location, VarFlags.Const);
                    cw.PopVar(sv);
                }
                else
                    cw.Pop();
            }

            Build(node.Catch, hints.Append(Catch), ctx);
            ctx.Errors.Pop();
            EndScope();

            cw.MarkLabel(skip);
            AddLinePragma(node);
            cw.Nop();
        }

        private void Build(DYieldBlock node, Hints hints, CompilerContext ctx)
        {
            if (node.Elements.Count is 0)
            {
                PushIf(hints);
                return;
            }

            for (var i = 0; i < node.Elements.Count; i++)
            {
                var n = node.Elements[i];
                var last = i == node.Elements.Count - 1;
                Build(n, hints.Append(Push), ctx);
                AddLinePragma(node);
                cw.Yield();

                if (last)
                    PushIf(hints);
            }
        }

        private void Build(DIteratorLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.YieldBlock.Elements.Count is 1 && node.YieldBlock.Elements[0].NodeType == NodeType.Range)
            {
                Build(node.YieldBlock.Elements[0], hints.Append(Push), ctx);
                PopIf(hints);
            }
            else
            {
                var dec = new DFunctionDeclaration(node.Location) { Body = node.YieldBlock };
                Build(dec, hints.Append(IteratorBody), ctx);
            }
        }

        private void Build(DRange range, Hints hints, CompilerContext ctx)
        {
            cw.Type(DyType.Iterator);
            cw.GetMember(Builtins.Range);
            cw.FunPrep(4);

            if (range.From is not null)
                Build(range.From, hints.Append(Push), ctx);
            else
                cw.Push(0);

            cw.FunArgIx(0);

            if (range.To is not null)
                Build(range.To, hints.Append(Push), ctx);
            else
                cw.PushNil();

            cw.FunArgIx(1);

            if (range.Step is not null)
                Build(range.Step, hints.Append(Push), ctx);
            else
                cw.Push(1);
            
            cw.FunArgIx(2);
            cw.Push(range.Exclusive);
            cw.FunArgIx(3);
            AddLinePragma(range);
            cw.FunCall(4);
            PopIf(hints);
        }

        private void Build(DYield node, Hints hints, CompilerContext ctx)
        {
            Build(node.Expression, hints.Append(Push), ctx);
            AddLinePragma(node);
            cw.Yield();
            PushIf(hints);
        }

        private void Build(DYieldMany node, Hints hints, CompilerContext ctx)
        {
            var sys = AddVariable();
            var initSkip = cw.DefineLabel();
            var exit = cw.DefineLabel();

            Build(node.Expression, hints.Append(Push), ctx);
            cw.Briter(initSkip);
            cw.GetMember(Builtins.Iterator);
            cw.FunPrep(0);
            cw.FunCall(0);

            cw.MarkLabel(initSkip);
            AddLinePragma(node);
            cw.GetIter();
            cw.PopVar(sys);

            var iter = cw.DefineLabel();
            cw.MarkLabel(iter);
            cw.PushVar(new ScopeVar(sys));
            cw.FunPrep(0);
            cw.FunCall(0);
            cw.Brterm(exit);
            cw.Yield();
            cw.Br(iter);

            cw.MarkLabel(exit);
            cw.Pop();
            PushIf(hints);
        }

        private void Build(DYieldBreak node, Hints _, CompilerContext ctx)
        {
            AddLinePragma(node);
            cw.PushNil();
            cw.Br(ctx.FunctionExit);
        }

        private void Build(DBase node) => AddError(CompilerError.BaseNotAllowed, node.Location);

        private void Build(DAccess node, Hints hints, CompilerContext ctx)
        {
            //An access expression can be a reference to a module (and can be optimized away)
            if (node.Target.NodeType == NodeType.Name && !hints.Has(Pop) && !options.NoOptimizations)
            {
                var nm = node.Target.GetName()!;
                var err = GetVariable(nm, currentScope, out var sv);

                if (err is CompilerError.None
                    && (sv.Data & VarFlags.Module) == VarFlags.Module && referencedUnits.TryGetValue(nm, out var ru))
                {
                    if (ru.Unit.ExportList.TryGetValue(node.Name, out var var))
                    {
                        if ((var.Data & VarFlags.Private) == VarFlags.Private)
                            AddError(CompilerError.PrivateNameAccess, node.Location, node.Name);

                        AddLinePragma(node);
                        cw.PushVar(new ScopeVar(ru.Handle | (var.Address >> 8) << 8, VarFlags.External));
                        PopIf(hints);
                        return;
                    }
                    else if (char.IsUpper(node.Name[0])) //If true, it's a type
                    {
                        AddLinePragma(node);
                        PushTypeInfo(ctx, ru, node.Name, node.Location);
                        return;
                    }
                }
            }

            if (node.Target.NodeType == NodeType.Base)
            {
                if (!hints.Has(Function))
                {
                    AddError(CompilerError.BaseNotAllowed, node.Location);
                    return;
                }

                var sv = GetParentVariable(node.Name, node.Location);
                AddLinePragma(node);
                if (hints.Has(Pop))
                    cw.PopVar(sv.Address);
                else
                    cw.PushVar(sv);
                return;
            }

            var push = hints.Remove(Pop).Append(Push);
            Build(node.Target, push, ctx);

            //A method access
            if (char.IsUpper(node.Name[0]))
            {
                AddLinePragma(node);
                cw.GetMember(node.Name);
                PopIf(hints);
            }
            //An indexer
            else
            {
                AddLinePragma(node);
                cw.Push(node.Name);
                if (!hints.Has(Pop))
                {
                    cw.Get();
                    PopIf(hints);
                }
                else
                    cw.Set();
            }
        }

        private void Build(DTupleLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.Elements.Count is 1 && node.Elements[0].NodeType == NodeType.Range)
            {
                Build(node.Elements[0], hints.Append(Push), ctx);
                cw.GetMember(Builtins.ToTuple);
                cw.FunPrep(0);
                AddLinePragma(node);
                cw.FunCall(0);
            }
            else
            {
                for (var i = 0; i < node.Elements.Count; i++)
                {
                    var el = node.Elements[i];

                    if (el.NodeType == NodeType.Label)
                    {
                        var label = (DLabelLiteral)el;
                        Build(label.Expression, hints.Append(Push), ctx);
                        cw.Tag(label.Label);

                        if (char.IsUpper(label.Label[0]) && !label.FromString)
                            AddError(CompilerError.LabelOnlyCamel, label.Location);

                        if (label.Mutable)
                            cw.Mut();
                    }
                    else
                    {
                        Build(el, hints.Append(Push), ctx);
                        string? name;

                        if ((name = el.GetName()) is not null)
                            cw.Tag(name);
                    }
                }

                AddLinePragma(node);
                cw.NewTuple(node.Elements.Count);
            }

            PopIf(hints);
        }

        private void Build(DArrayLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.Elements.Count is 1 && node.Elements[0].NodeType == NodeType.Range)
            {
                Build(node.Elements[0], hints.Append(Push), ctx);
                cw.GetMember(Builtins.ToArray);
                cw.FunPrep(0);
                AddLinePragma(node);
                cw.FunCall(0);
            }
            else
            {
                cw.Type(DyType.Array);
                cw.GetMember(DyTypeNames.Array);
                cw.FunPrep(node.Elements.Count);

                for (var i = 0; i < node.Elements.Count; i++)
                {
                    Build(node.Elements[i], hints.Append(Push), ctx);
                    cw.FunArgIx(i);
                }

                AddLinePragma(node);
                cw.FunCall(node.Elements.Count);
            }

            PopIf(hints);
        }

        private void BuildIndexer(DIndexer node, Hints hints, CompilerContext ctx)
        {
            var push = hints.Remove(Pop).Append(Push);

            if (node.Index.NodeType == NodeType.Range)
            {
                if (hints.Has(Pop))
                    AddError(CompilerError.SliceNotSupported, node.Index.Location);

                var r = (DRange)node.Index;

                if (r.Step is not null || r.Exclusive)
                    AddError(CompilerError.InvalidSlice, r.Location);

                cw.GetMember(Builtins.Slice);
                cw.FunPrep(2);

                if (r.From is null)
                    cw.Push(0);
                else
                    Build(r.From, hints.Append(Push), ctx);

                cw.FunArgIx(0);

                if (r.To is not null)
                    Build(r.To, hints.Append(Push), ctx);
                else
                    cw.PushNil();

                cw.FunArgIx(1);
                AddLinePragma(node);
                cw.FunCall(2);
                PopIf(hints);
            }
            else
            {
                Build(node.Index, push, ctx);
                AddLinePragma(node);

                if (!hints.Has(Pop))
                {
                    cw.Get();
                    PopIf(hints);
                }
                else
                    cw.Set();
            }
        }

        private void Build(DIndexer node, Hints hints, CompilerContext ctx)
        {
            var push = hints.Remove(Pop).Append(Push);
            Build(node.Target, push, ctx);
            BuildIndexer(node, hints, ctx);
        }

        private void BuildImport(DImport node)
        {
            var localPath = node.LocalPath;
            string? dll = null;

            if (localPath is not null && localPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var idx = localPath.LastIndexOf('/');

                if (idx != -1)
                {
                    dll = localPath[(idx + 1)..];
                    localPath = localPath.Substring(0, idx);
                }
                else
                {
                    dll = localPath;
                    localPath = null;
                }
            }
            else if (localPath == "std")
            {
                dll = "Dyalect.Library.dll";
                localPath = null;
            }

            var r = new Reference(node.ModuleName, localPath, dll, node.Location, unit.FileName);
            var res = linker.Link(unit, r);

            if (res.Success)
            {
                r.Checksum = res.Value!.Checksum;
                var referencedUnit = new UnitInfo(unit.UnitIds.Count, res.Value);
                unit.References.Add(r);
                referencedUnits.Add(node.Alias ?? node.ModuleName, referencedUnit);

                cw.RunMod(unit.UnitIds.Count);
                unit.UnitIds.Add(-1); //Real handles are added by a linker

                var addr = AddVariable(node.Alias ?? node.ModuleName, node.Location, VarFlags.Module);
                cw.PopVar(addr);
            }
            else
            {
                AddError(CompilerError.UnableToLinkModule, node.Location, node.ModuleName);
                throw new TerminationException();
            }
        }

        private void Build(DReturn node, Hints hints, CompilerContext ctx)
        {
            if (ctx.FunctionExit.IsEmpty())
                AddError(CompilerError.ReturnNotAllowed, node.Location);

            if (hints.Has(IteratorBody))
                AddError(CompilerError.ReturnInIterator, node.Location);

            if (node.Expression != null)
                Build(node.Expression, hints.Append(Push), ctx);
            else
                cw.PushNil();

            CallAutosForKind(ScopeKind.Function);
            AddLinePragma(node);
            cw.Br(ctx.FunctionExit);
        }

        private void Build(DApplication node, Hints hints, CompilerContext ctx)
        {
            var name = node.Target.NodeType == NodeType.Name ? node.Target.GetName() : null;
            var sv = ScopeVar.Empty;

            if (name is not null)
                GetVariable(name, currentScope, out sv);

            var newHints = hints.Remove(Last);

            //Check if an application is in fact a built-in operator call
            if (name is not null && sv.IsEmpty())
                if (name is "nameof" && node.Arguments.Count == 1)
                {
                    var push = GetExpressionName(node.Arguments[0], ctx);
                    AddLinePragma(node);
                    if (push is not null)
                        cw.Push(push);
                    PopIf(hints);
                    return;
                }

            //This is a special optimization for the 'toString', 'has' and 'len' methods
            //If we see that it is called directly we can emit a direct op code
            if (node.Target.NodeType == NodeType.Access && !options.NoOptimizations
                && node.Target is DAccess meth && meth.Target is not null)
            {
                if (meth.Name == Builtins.ToStr && node.Arguments.Count is 0)
                {
                    Build(meth.Target, newHints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Str();
                    PopIf(hints);
                    return;
                }

                if (meth.Name == Builtins.Len && node.Arguments.Count is 0)
                {
                    Build(meth.Target, newHints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Len();
                    PopIf(hints);
                    return;
                }

                if (meth.Name == Builtins.Has && node.Arguments.Count is 1
                      && node.Arguments[0].NodeType == NodeType.String
                      && node.Arguments[0] is DStringLiteral {Chunks: null} str)
                {
                    Build(meth.Target, newHints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.HasMember(str.Value);
                    PopIf(hints);
                    return;
                }
            }

            //Tail recursion optimization
            if (!options.NoOptimizations && hints.Has(Last)
                && !sv.IsEmpty() && ctx.Function is not null && ctx.Function.TypeName is null && !ctx.Function.IsIterator 
                && name == ctx.Function.Name && node.Arguments.Count == ctx.Function.Parameters.Count 
                && (ctx.FunctionAddress >> 8) == (sv.Address >> 8) 
                && (ctx.FunctionAddress & byte.MaxValue) == (counters.Count - (sv.Address & byte.MaxValue)) 
                && !ctx.Function.IsVariadic() && !HasLabels(node.Arguments))
            {
                for (var i = 0; i < node.Arguments.Count; i++)
                    Build(node.Arguments[i], newHints.Append(Push), ctx);

                for (var i = 0; i < ctx.Function.Parameters.Count; i++)
                {
                    var p = ctx.Function.Parameters[ctx.Function.Parameters.Count - i - 1];
                    PopVariable(ctx, p.Name, p.Location);
                }

                AddLinePragma(node);
                cw.Br(ctx.FunctionStart);
            }
            else
            {
                Build(node.Target, newHints.Append(Push), ctx);
                AddLinePragma(node);
                cw.FunPrep(node.Arguments.Count);
                Dictionary<string, object?>? dict = null;
                var kwArg = false;

                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var a = node.Arguments[i];

                    if (a.NodeType == NodeType.Label)
                    {
                        if (dict is null)
                            dict = new();

                        kwArg = true;
                        var la = (DLabelLiteral)a;
                        if (dict.ContainsKey(la.Label))
                            AddError(CompilerError.NamedArgumentMultipleTimes, la.Location, la.Label);
                        else
                            dict.Add(la.Label, null);

                        Build(la.Expression, newHints.Append(Push).Remove(IteratorBody), ctx);
                        cw.FunArgNm(la.Label);
                    }
                    else
                    {
                        Build(a, newHints.Append(Push), ctx);
                        cw.FunArgIx(i);

                        if (kwArg)
                            AddError(CompilerError.PositionalArgumentAfterKeyword, a.Location);
                    }
                }

                AddLinePragma(node);
                cw.FunCall(node.Arguments.Count);
            }

            PopIf(hints);
        }

        private bool HasLabels(List<DNode> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].NodeType == NodeType.Label)
                    return true;
            }

            return false;
        }

        private void Build(DIf node, Hints hints, CompilerContext ctx)
        {
            var falseLabel = cw.DefineLabel();
            var skipLabel = cw.DefineLabel();

            StartScope(ScopeKind.Lexical, node.Location);
            Build(node.Condition, hints.Remove(Last).Append(Push), ctx);
            AddLinePragma(node);
            cw.Brfalse(falseLabel);
            Build(node.True, hints, ctx);
            AddLinePragma(node);
            cw.Br(skipLabel);
            cw.MarkLabel(falseLabel);

            if (node.False != null)
                Build(node.False, hints, ctx);
            else
                PushIf(hints);

            cw.MarkLabel(skipLabel);
            cw.Nop();
            EndScope();
        }

        private void Build(DStringLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.Chunks is null && NoPush(node, hints))
                return;

            if (node.Chunks is not null)
            {
                cw.Type(DyType.String);
                cw.GetMember(Builtins.Concat);
                cw.FunPrep(node.Chunks.Count);

                for (var i = 0; i < node.Chunks.Count; i++)
                {
                    var c = node.Chunks[i];

                    if (c.IsCode)
                    {
                        var p = new InternalParser("", new(SourceBuffer.FromString(c.GetContent())));
                        p.Parse();

                        if (p.Errors.Count > 0)
                        {
                            foreach (var e in p.Errors)
                                AddError(CompilerError.CodeIslandInvalid, new(node.Location.Line, node.Location.Column + e.Column), e.Message);
                        }
                        else
                        {
                            if (p.Root.Nodes is null || p.Root.Nodes.Count is 0)
                                AddError(CompilerError.CodeIslandEmpty, node.Location);
                            else
                            {
                                corrections.Push(node.Location);
                                Build(p.Root.Nodes[0], hints.Append(Push), ctx);
                                corrections.Pop();
                            }
                        }
                    }
                    else
                        cw.Push(c.GetContent());

                    cw.FunArgIx(i);
                }

                AddLinePragma(node);
                cw.FunCall(node.Chunks.Count);
            }
            else
            {
                AddLinePragma(node);
                cw.Push(node.Value);
            }

            PopIf(hints);
        }

        private void Build(DCharLiteral node, Hints hints)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DFloatLiteral node, Hints hints)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DName node, Hints hints, CompilerContext ctx)
        {
            if (!hints.Has(Pop))
            {
                if (NoPush(node, hints))
                    return;

                PushVariable(ctx, node.Value, node.Location);
            }
            else
                PopVariable(ctx, node.Value, node.Location);
        }

        private void Build(DIntegerLiteral node, Hints hints)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DBooleanLiteral node, Hints hints)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DNilLiteral node, Hints hints)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.PushNil();
        }

        private bool HasAuto(List<DNode> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].HasAuto())
                    return true;
            }

            return false;
        }

        private void Build(DBlock node, Hints hints, CompilerContext ctx)
        {
            var hasPush = hints.Has(Push);
            var hasLast = hints.Has(Last);
            hints = hints.Remove(Last);

            if ((node.Nodes is null || node.Nodes.Count is 0) && !hints.Has(Catch))
            {
                if (hints.Has(Push))
                    cw.PushNil();
                return;
            }

            Label gotcha = default;
            var hasAuto = false;

            //Start a compile time lexical scope
            if (!hints.Has(NoScope))
            {
                hasAuto = node.Nodes is not null && HasAuto(node.Nodes);
                if (hasAuto)
                {
                    gotcha = cw.DefineLabel();
                    cw.Start(gotcha);
                }

                StartScope(ScopeKind.Lexical, loc: node.Location);
            }
            else if (node.Nodes is not null && HasAuto(node.Nodes))
                AddError(CompilerError.AutoNotAllowed, node.Location);

            var newHints = hints.Remove(NoScope);

            if (node.Nodes is not null)
                for (var i = 0; i < node.Nodes.Count; i++)
                {
                    var n = node.Nodes[i];
                    var last = i == node.Nodes.Count - 1;
                    var nh = hasPush && last ? newHints : newHints.Remove(Push);
                    nh = hasLast && last ? nh.Append(Last) : nh;
                    Build(n, nh, ctx);
                }

            if (hasAuto)
            {
                var (skip, exit) = (cw.DefineLabel(), cw.DefineLabel());
                cw.Br(skip);            //goto: regular
                cw.MarkLabel(gotcha);   //|"catch" section
                CallAutos(cls: false);  //|"catch" section
                cw.Fail();              //|"catch" section
                cw.Br(exit);            //goto: exit
                cw.MarkLabel(skip);     //|regular section
                cw.End();               //|regular section
                CallAutos(cls: true);   //|regular section
                cw.MarkLabel(exit);     //exit
                cw.Nop();
            }

            if (!hints.Has(NoScope))
                EndScope();
        }

        private void Build(DBinding node, Hints hints, CompilerContext ctx)
        {
            if (CanBeOptimized(node, hints, ctx))
            {
                PushIf(hints);
                return;
            }

            if (node.Init is not null)
                Build(node.Init, hints.Append(Push), ctx);
            else
                cw.PushNil();

            if (node.Pattern.NodeType == NodeType.NamePattern)
            {
                AddLinePragma(node);
                var flags = node.Constant ? VarFlags.Const : VarFlags.None;
                var nam = node.Pattern.GetName();
                var a = AddVariable(nam!, node.Location, flags);
                cw.PopVar(a);

                if (node.AutoClose)
                    currentScope.Autos.Enqueue((a >> 8, nam!));
            }
            else
            {
                if (node.Init == null)
                    AddError(CompilerError.BindingPatternNoInit, node.Location);

                var nh = hints.Append(OpenMatch);

                if (node.Constant)
                    nh = nh.Append(Const);

                if (node.Init != null)
                    CheckPattern(node.Pattern, node.Init.GetElementCount());

                BuildPattern(node.Pattern, nh, ctx);
                var skip = cw.DefineLabel();
                cw.Brtrue(skip);
                cw.NewErr(DyErrorCode.MatchFailed, 0);
                cw.Fail();
                cw.MarkLabel(skip);
                cw.Nop();
            }

            PushIf(hints);
        }

        private void Build(DRebinding node, Hints hints, CompilerContext ctx)
        {
            if (!CanBeOptimized(node, hints, ctx))
            {
                Build(node.Init, hints.Append(Push), ctx);
                BuildPattern(node.Pattern, hints.Append(Rebind), ctx);
                var skip = cw.DefineLabel();
                cw.Brtrue(skip);
                cw.NewErr(DyErrorCode.MatchFailed, 0);
                cw.Fail();
                cw.MarkLabel(skip);
                cw.Nop();
            }

            PushIf(hints);
        }

        private bool CanBeOptimized(DBindingBase node, Hints hints, CompilerContext ctx)
        {
            if (options.NoOptimizations 
                || node.Init is null || node.Init.NodeType != NodeType.Tuple 
                || node.Pattern.NodeType != NodeType.TuplePattern
                || node.Init.GetElementCount() != node.Pattern.GetElementCount())
                return false;
            
            var init = (DTupleLiteral)node.Init;
            var pat = (DTuplePattern)node.Pattern;

            for (var i = 0; i < pat.Elements.Count; i++)
                if (pat.Elements[i].NodeType is not NodeType.NamePattern and not NodeType.WildcardPattern)
                    return false;

            for (var i = 0; i < init.Elements.Count; i++)
            {
                Build(init.Elements[i], hints.Append(Push), ctx);
            }

            for (var i = 0; i < pat.Elements.Count; i++)
            {
                var e = pat.Elements[pat.Elements.Count - i - 1];
                var nm = e.GetName()!;

                if (nm is null)
                    cw.Pop();
                else if (node.NodeType is NodeType.Binding)
                {
                    var a = AddVariable(nm, e.Location, VarFlags.None);
                    cw.PopVar(a);
                }
                else if (node.NodeType is NodeType.Rebinding)
                {
                    if (VariableExists(nm) is CompilerError.None)
                        PopVariable(ctx, nm, e.Location);
                    else
                    {
                        var a = AddVariable(nm, e.Location, VarFlags.None);
                        cw.PopVar(a);
                    }
                }
            }

            return true;
        }

        private void Build(DUnaryOperation node, Hints hints, CompilerContext ctx)
        {
            Build(node.Node, hints.Append(Push), ctx);
            AddLinePragma(node);

            if (node.Operator == UnaryOperator.Neg)
                cw.Neg();
            else if (node.Operator == UnaryOperator.Not)
                cw.Not();
            else if (node.Operator == UnaryOperator.BitwiseNot)
                cw.BitNot();

            PopIf(hints);
        }

        private void Build(DBinaryOperation node, Hints hints, CompilerContext ctx)
        {
            Label termLab;
            Label exitLab;

            switch (node.Operator)
            {
                case BinaryOperator.Coalesce:
                    exitLab = cw.DefineLabel();
                    Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                    cw.Dup();
                    cw.Brtrue(exitLab);
                    cw.Pop();
                    Build(node.Right, hints.Append(Push), ctx);
                    cw.MarkLabel(exitLab);
                    cw.Nop();
                    break;
                case BinaryOperator.And:
                    Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                    termLab = cw.DefineLabel();
                    exitLab = cw.DefineLabel();
                    cw.Brfalse(termLab);
                    Build(node.Right, hints.Remove(Last).Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Br(exitLab);
                    cw.MarkLabel(termLab);
                    AddLinePragma(node);
                    cw.Push(false);
                    cw.MarkLabel(exitLab);
                    cw.Nop();
                    break;
                case BinaryOperator.Or:
                    Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                    termLab = cw.DefineLabel();
                    exitLab = cw.DefineLabel();
                    cw.Brtrue(termLab);
                    Build(node.Right, hints.Remove(Last).Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Br(exitLab);
                    cw.MarkLabel(termLab);
                    AddLinePragma(node);
                    cw.Push(true);
                    cw.MarkLabel(exitLab);
                    cw.Nop();
                    break;
                case BinaryOperator.Is:
                    {
                        var pat = (DPattern)node.Right;
                        AddLinePragma(node);
                        PreinitPattern(pat, hints.Remove(Last));
                        Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                        BuildPattern(pat, hints, ctx);
                    }
                    break;
                case BinaryOperator.In:
                    {
                        Build(node.Right, hints.Remove(Last).Append(Push), ctx);
                        AddLinePragma(node);
                        cw.GetMember(Builtins.Contains);
                        cw.FunPrep(1);
                        Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                        AddLinePragma(node);
                        cw.FunArgIx(0);
                        cw.FunCall(1);
                    }
                    break;
                default:
                    Build(node.Left, hints.Remove(Last).Append(Push), ctx);
                    Build(node.Right, hints.Remove(Last).Append(Push), ctx);
                    AddLinePragma(node);
                    EmitBinaryOp(node.Operator);
                    break;
            }

            PopIf(hints);
        }

        private void EmitBinaryOp(BinaryOperator op)
        {
            switch (op)
            {
                case BinaryOperator.Add: cw.Add(); break;
                case BinaryOperator.Sub: cw.Sub(); break;
                case BinaryOperator.Mul: cw.Mul(); break;
                case BinaryOperator.Div: cw.Div(); break;
                case BinaryOperator.Rem: cw.Rem(); break;
                case BinaryOperator.Eq: cw.Eq(); break;
                case BinaryOperator.NotEq: cw.NotEq(); break;
                case BinaryOperator.Gt: cw.Gt(); break;
                case BinaryOperator.Lt: cw.Lt(); break;
                case BinaryOperator.GtEq: cw.GtEq(); break;
                case BinaryOperator.LtEq: cw.LtEq(); break;
                case BinaryOperator.ShiftLeft: cw.Shl(); break;
                case BinaryOperator.ShiftRight: cw.Shr(); break;
                case BinaryOperator.BitwiseAnd: cw.And(); break;
                case BinaryOperator.BitwiseOr: cw.Or(); break;
                case BinaryOperator.Xor: cw.Xor(); break;
                default:
                    throw Ice();
            }
        }

        private string? GetExpressionName(DNode node, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
                case NodeType.Name:
                    var name = node.GetName();

                    if (name is not null)
                    {
                        var err = VariableExists(name);
                        if (err is not CompilerError.None && DyType.GetTypeCodeByName(name) == 0)
                            AddError(err, node.Location, name);
                    }

                    return name;
                case NodeType.Access:
                    return ((DAccess)node).Name;
                case NodeType.Index:
                    var idx = (DIndexer)node;
                    return GetExpressionName(idx.Index, ctx);
                case NodeType.String:
                    return ((DStringLiteral)node).Value;
                default:
                    AddError(CompilerError.ExpressionNoName, node.Location);
                    return "";
            }
        }
    }
}