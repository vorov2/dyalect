using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using System.Collections.Generic;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    partial class Builder
    {
        private void Build(DNode node, Hints hints, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
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
                    Build((DBooleanLiteral)node, hints, ctx);
                    break;
                case NodeType.Float:
                    Build((DFloatLiteral)node, hints, ctx);
                    break;
                case NodeType.If:
                    Build((DIf)node, hints, ctx);
                    break;
                case NodeType.Integer:
                    Build((DIntegerLiteral)node, hints, ctx);
                    break;
                case NodeType.Name:
                    Build((DName)node, hints, ctx);
                    break;
                case NodeType.String:
                    Build((DStringLiteral)node, hints, ctx);
                    break;
                case NodeType.Nil:
                    Build((DNilLiteral)node, hints, ctx);
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
                case NodeType.Base:
                    Build((DBase)node, hints, ctx);
                    break;
                case NodeType.Char:
                    Build((DCharLiteral)node, hints, ctx);
                    break;
                case NodeType.MemberCheck:
                    Build((DMemberCheck)node, hints, ctx);
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
            }
        }

        private void Build(DThrow node, Hints hints, CompilerContext ctx)
        {
            Build(node.Expression, hints.Append(Push), ctx);
            AddLinePragma(node);
            cw.Fail();
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

            StartScope(false, node.Catch.Location);

            if (node.BindVariable != null)
            {
                AddLinePragma(node.BindVariable);

                if (node.BindVariable.Value != "_")
                {
                    var sv = AddVariable(node.BindVariable.Value, node.Catch, VarFlags.Const);
                    cw.PopVar(sv);
                }
                else
                    cw.Pop();
            }

            Build(node.Catch, hints, ctx);
            EndScope();

            cw.MarkLabel(skip);
            AddLinePragma(node);
            cw.Nop();
        }

        private void Build(DYieldBlock node, Hints hints, CompilerContext ctx)
        {
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
            var dec = new DFunctionDeclaration(node.Location) { Body = node.YieldBlock };
            Build(dec, hints.Append(Iterator), ctx);
        }

        private void Build(DRange range, Hints hints, CompilerContext ctx)
        {
            Build(range.From, hints.Append(Push), ctx);
            cw.GetMember(GetMemberNameId("to"));
            cw.FunPrep(1);

            Build(range.To, hints.Append(Push), ctx);
            cw.FunArgIx(0);

            AddLinePragma(range);
            cw.FunCall(1);

            PopIf(hints);
        }

        private void Build(DMemberCheck node, Hints hints, CompilerContext ctx)
        {
            Build(node.Target, hints.Append(Push), ctx);
            AddLinePragma(node);
            var nameId = GetMemberNameId(node.Name);
            cw.HasMember(nameId);
            PopIf(hints);
        }

        private void Build(DYield node, Hints hints, CompilerContext ctx)
        {
            Build(node.Expression, hints.Append(Push), ctx);
            AddLinePragma(node);
            cw.Yield();
            PushIf(hints);
        }

        private void Build(DBase node, Hints hints, CompilerContext ctx)
        {
            AddError(CompilerError.BaseNotAllowed, node.Location);
        }

        private void Build(DAccess node, Hints hints, CompilerContext ctx)
        {
            if (node.Target.NodeType == NodeType.Base)
            {
                if (!hints.Has(Function))
                {
                    AddError(CompilerError.BaseNotAllowed, node.Location);
                    return;
                }

                if (NoPush(node, hints))
                    return;

                var sv = GetParentVariable(node.Name, node);
                AddLinePragma(node);
                cw.PushVar(sv);
                return;
            }
            else if (node.Target.NodeType == NodeType.Name)
            {
                var nm = node.Target.GetName();
                var sv = GetVariable(nm, node.Target, err: false);

                if ((sv.Data & VarFlags.Module) == VarFlags.Module 
                    && referencedUnits.TryGetValue(nm, out var ru)
                   )
                {
                    if (ru.Unit.ExportList.TryGetValue(node.Name, out var var))
                    {
                        if ((var.Data & VarFlags.Private) == VarFlags.Private)
                            AddError(CompilerError.PrivateNameAccess, node.Location, node.Name);

                        AddLinePragma(node);
                        cw.PushVar(new ScopeVar(ru.Handle | (var.Address >> 8) << 8, VarFlags.External));
                        return;
                    }
                    else if (GetTypeHandle(nm, node.Name, out var handle, out var std) == CompilerError.None)
                    {
                        GetMemberNameId(node.Name);
                        AddLinePragma(node);
                        cw.Type(new TypeHandle(handle, std));
                        return;
                    }
                }
            }

            Build(node.Target, hints.Remove(Pop).Append(Push), ctx);

            AddLinePragma(node);
            var nameId = GetMemberNameId(node.Name);

            if (hints.Has(Pop))
            {
                cw.Push(node.Name);
                cw.Set();
            }
            else
            {
                cw.GetMember(nameId);
                PopIf(hints);
            }
        }

        private void Build(DTupleLiteral node, Hints hints, CompilerContext ctx)
        {
            for (var i = 0; i < node.Elements.Count; i++)
            {
                var el = node.Elements[i];
                string name;

                if (el.NodeType == NodeType.Label)
                {
                    var label = (DLabelLiteral)el;
                    Build(label.Expression, hints.Append(Push), ctx);
                    cw.Tag(label.Label);
                }
                else
                {
                    Build(el, hints.Append(Push), ctx);

                    if ((name = el.GetName()) != null)
                        cw.Tag(name);
                }
            }

            AddLinePragma(node);
            cw.NewTuple(node.Elements.Count);
            PopIf(hints);
        }

        private void Build(DArrayLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.Elements.Count == 1 && node.Elements[0].NodeType == NodeType.Range)
            {
                Build(node.Elements[0], hints.Append(Push), ctx);
                cw.GetMember(GetMemberNameId("toArray"));
                cw.FunPrep(0);
                AddLinePragma(node);
                cw.FunCall(0);
            }
            else
            {
                cw.Type(new TypeHandle(DyType.Array, true));
                cw.GetMember(GetMemberNameId(DyTypeNames.Array));
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

        private void Build(DIndexer node, Hints hints, CompilerContext ctx)
        {
            var push = hints.Remove(Pop).Append(Push);
            Build(node.Target, push, ctx);

            if (node.Index.NodeType == NodeType.Range)
            {
                if (hints.Has(Pop))
                    AddError(CompilerError.RangeIndexNotSupported, node.Index.Location);

                var r = (DRange)node.Index;
                cw.GetMember(GetMemberNameId("slice"));
                cw.FunPrep(2);
                Build(r.From, hints.Append(Push), ctx);
                cw.FunArgIx(0);
                Build(r.To, hints.Append(Push), ctx);
                Build(r.From, hints.Append(Push), ctx);
                cw.Sub();
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

        private void BuildImport(DImport node, CompilerContext ctx)
        {
            var r = new Reference(node.ModuleName, node.LocalPath, node.Dll, node.Location, unit.FileName);
            var res = linker.Link(unit, r);

            if (res.Success)
            {
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

            if (node.Expression != null)
                Build(node.Expression, hints.Append(Push), ctx);
            else
                cw.PushNil();

            AddLinePragma(node);
            cw.Br(ctx.FunctionExit);
        }

        private void Build(DApplication node, Hints hints, CompilerContext ctx)
        {
            var name = node.Target.NodeType == NodeType.Name ? node.Target.GetName() : null;
            var sv = name != null ? GetVariable(name, node, err: false) : ScopeVar.Empty;

            //Check if an application is in fact a built-in operator call
            if (name != null && sv.IsEmpty() && node.Arguments.Count == 1)
                if (name == "nameof")
                {
                    var push = GetExpressionName(node.Arguments[0]);
                    AddLinePragma(node);
                    cw.Push(push);
                    return;
                }
                else if (name == "valueof")
                {
                    Build(node.Arguments[0], hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Unbox();
                    return;
                }
                else if (name == "new")
                {
                    if (ctx.Function?.TypeName == null)
                    {
                        AddError(CompilerError.CtorNoMethod, node.Location);
                        return;
                    }

                    if (ctx.Function.TypeName.Parent != null ||
                        !TryGetLocalType(ctx.Function.TypeName.Local, out var ti))
                    {
                        AddError(CompilerError.CtorOnlyLocalType, node.Location, ctx.Function.TypeName);
                        return;
                    }

                    Build(node.Arguments[0], hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Aux(GetMemberNameId(ctx.Function.Name));
                    cw.NewType(ti.TypeId);
                    return;
                }

            //This is a special optimization for the 'toString' and 'len' methods
            //If we see that it is called directly we than emit a direct Str or Len op code
            if (node.Target.NodeType == NodeType.Access)
            {
                var meth = (DAccess)node.Target;

                if (meth.Name == Builtins.ToStr && node.Arguments.Count == 0)
                {
                    Build(meth.Target, hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Str();
                    PopIf(hints);
                    return;
                }

                if (meth.Name == Builtins.Len && node.Arguments.Count == 0)
                {
                    Build(meth.Target, hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Len();
                    PopIf(hints);
                    return;
                }

                if (meth.Name == Builtins.Has && node.Arguments.Count == 1
                    && node.Arguments[0].NodeType == NodeType.String
                    && node.Arguments[0] is DStringLiteral str
                    && str.Chunks == null)
                {
                    Build(meth.Target, hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.HasMember(GetMemberNameId(str.Value));
                    PopIf(hints);
                    return;
                }
            }

            if (!sv.IsEmpty())
                cw.PushVar(sv);
            else
                Build(node.Target, hints.Append(Push), ctx);

            AddLinePragma(node);
            cw.FunPrep(node.Arguments.Count);
            Dictionary<string, object> dict = null;

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var a = node.Arguments[i];

                if (a.NodeType == NodeType.Label)
                {
                    if (dict == null)
                        dict = new Dictionary<string, object>();

                    var la = (DLabelLiteral)a;
                    if (dict.ContainsKey(la.Label))
                        AddError(CompilerError.NamedArgumentMultipleTimes, la.Location, la.Label);
                    else
                        dict.Add(la.Label, null);

                    Build(la.Expression, hints.Append(Push), ctx);
                    cw.FunArgNm(la.Label);
                }
                else
                {
                    Build(a, hints.Append(Push), ctx);
                    cw.FunArgIx(i);
                }
            }

            AddLinePragma(node);
            cw.FunCall(node.Arguments.Count);
            PopIf(hints);
        }

        private void Build(DIf node, Hints hints, CompilerContext ctx)
        {
            var falseLabel = cw.DefineLabel();
            var skipLabel = cw.DefineLabel();

            StartScope(false, node.Location);
            Build(node.Condition, hints.Append(Push), ctx);
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
            if (node.Chunks == null && NoPush(node, hints))
                return;

            if (node.Chunks != null)
            {
                cw.Type(new TypeHandle(DyType.String, true));
                cw.GetMember(GetMemberNameId("concat"));
                cw.FunPrep(node.Chunks.Count);

                for (var i = 0; i < node.Chunks.Count; i++)
                {
                    var c = node.Chunks[i];

                    if (c.IsCode)
                    {
                        var p = new InternalParser(new Scanner(SourceBuffer.FromString(c.GetContent())));
                        p.Parse();

                        if (p.Errors.Count > 0)
                        {
                            foreach (var e in p.Errors)
                                AddError(CompilerError.CodeIslandInvalid, new Location(node.Location.Line, node.Location.Column + e.Column), e.Message);
                        }
                        else
                        {
                            if (p.Root.Nodes == null || p.Root.Nodes.Count == 0)
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

        private void Build(DCharLiteral node, Hints hints, CompilerContext ctx)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DFloatLiteral node, Hints hints, CompilerContext ctx)
        {
            if (NoPush(node, hints))
                return;
            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DName node, Hints hints, CompilerContext ctx)
        {
            var sv = GetVariable(node.Value, node.Location, err: false);

            if (sv.IsEmpty())
            {
                var th = GetTypeHandle(null, node.Value, out var hdl, out var std);

                if (th != CompilerError.None)
                    AddError(CompilerError.UndefinedVariable, node.Location, node.Value);
                else
                    GetMemberNameId(node.Value);

                AddLinePragma(node);
                cw.Type(new TypeHandle(hdl, std));
                return;
            }

            AddLinePragma(node);

            if (!hints.Has(Pop))
            {
                if (NoPush(node, hints))
                    return;

                cw.PushVar(sv);
            }
            else
            {
                if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                    AddError(CompilerError.UnableAssignConstant, node.Location, node.Value);

                cw.PopVar(sv.Address);
            }
        }

        private void Build(DIntegerLiteral node, Hints hints, CompilerContext ctx)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DBooleanLiteral node, Hints hints, CompilerContext ctx)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.Push(node.Value);
        }

        private void Build(DNilLiteral node, Hints hints, CompilerContext ctx)
        {
            if (NoPush(node, hints))
                return;

            AddLinePragma(node);
            cw.PushNil();
        }

        private void Build(DBlock node, Hints hints, CompilerContext ctx)
        {
            var hasPush = hints.Has(Push);
            var hasLast = hints.Has(Last);
            hints = hints.Remove(Last);

            if (node.Nodes?.Count == 0)
            {
                if (hints.Has(Push))
                    cw.PushNil();
                return;
            }

            //Start a compile time lexical scope
            StartScope(fun: false, loc: node.Location);

            for (var i = 0; i < node.Nodes.Count; i++)
            {
                var n = node.Nodes[i];
                var last = i == node.Nodes.Count - 1;
                var nh = hasPush && last ? hints : hints.Remove(Push);
                nh = hasLast && last ? nh.Append(Last) : nh;
                Build(n, nh, ctx);
            }

            EndScope();
        }

        private void Build(DAssignment node, Hints hints, CompilerContext ctx)
        {
            if (node.AutoAssign != null)
                Build(node.Target, hints.Append(Push), ctx);

            Build(node.Value, hints.Append(Push), ctx);

            if (node.AutoAssign != null)
                EmitBinaryOp(node.AutoAssign.Value);

            if (node.Target.NodeType != NodeType.Name
                && node.Target.NodeType != NodeType.Index
                && node.Target.NodeType != NodeType.Access)
                AddError(CompilerError.UnableAssignExpression, node.Target.Location, node.Target);

            if (hints.Has(Push))
                cw.Dup();

            Build(node.Target, hints.Append(Pop), ctx);
        }

        private void Build(DBinding node, Hints hints, CompilerContext ctx)
        {
            if (node.Init != null)
                Build(node.Init, hints.Append(Push), ctx);
            else
                cw.PushNil();

            if (node.Pattern.NodeType == NodeType.NamePattern)
            {
                var flags = node.Constant ? VarFlags.Const : VarFlags.None;
                var a = AddVariable(node.Pattern.GetName(), node, flags);
                cw.PopVar(a);
            }
            else
            {
                var nh = hints;

                if (node.Constant)
                    nh = nh.Append(Const);

                BuildPattern(node.Pattern, nh, ctx);
                var skip = cw.DefineLabel();
                cw.Brtrue(skip);
                cw.Fail(DyErrorCode.MatchFailed);
                cw.MarkLabel(skip);
                cw.Nop();
            }

            PushIf(hints);
        }

        private void Build(DRebinding node, Hints hints, CompilerContext ctx)
        {
            Build(node.Init, hints.Append(Push), ctx);
            BuildPattern(node.Pattern, hints.Append(Rebind), ctx);
            var skip = cw.DefineLabel();
            cw.Brtrue(skip);
            cw.Fail(DyErrorCode.MatchFailed);
            cw.MarkLabel(skip);
            cw.Nop();
            PushIf(hints);
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
            var termLab = default(Label);
            var exitLab = default(Label);

            switch (node.Operator)
            {
                case BinaryOperator.Coalesce:
                    exitLab = cw.DefineLabel();
                    Build(node.Left, hints.Append(Push), ctx);
                    cw.Dup();
                    cw.Brtrue(exitLab);
                    cw.Pop();
                    Build(node.Right, hints.Append(Push), ctx);
                    cw.MarkLabel(exitLab);
                    cw.Nop();
                    break;
                case BinaryOperator.And:
                    Build(node.Left, hints.Append(Push), ctx);
                    termLab = cw.DefineLabel();
                    exitLab = cw.DefineLabel();
                    cw.Brfalse(termLab);
                    Build(node.Right, hints.Append(Push), ctx);
                    AddLinePragma(node);
                    cw.Br(exitLab);
                    cw.MarkLabel(termLab);
                    AddLinePragma(node);
                    cw.Push(false);
                    cw.MarkLabel(exitLab);
                    cw.Nop();
                    break;
                case BinaryOperator.Or:
                    Build(node.Left, hints.Append(Push), ctx);
                    termLab = cw.DefineLabel();
                    exitLab = cw.DefineLabel();
                    cw.Brtrue(termLab);
                    Build(node.Right, hints.Append(Push), ctx);
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
                        PreinitPattern(pat, hints);
                        Build(node.Left, hints.Append(Push), ctx);
                        BuildPattern(pat, hints, ctx);
                    }
                    break;
                default:
                    Build(node.Left, hints.Append(Push), ctx);
                    Build(node.Right, hints.Append(Push), ctx);
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

        private string GetExpressionName(DNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.Name:
                    var name = node.GetName();
                    GetVariable(name, node);
                    return name;
                case NodeType.Access:
                    return ((DAccess)node).Name;
                case NodeType.Index:
                    var idx = (DIndexer)node;
                    return GetExpressionName(idx.Index);
                case NodeType.String:
                    return ((DStringLiteral)node).Value;
                default:
                    AddError(CompilerError.ExpressionNoName, node.Location);
                    return "";
            }
        }
    }
}