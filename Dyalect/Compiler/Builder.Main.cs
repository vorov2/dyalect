using Dyalect.Parser;
using Dyalect.Parser.Model;
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
            }
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
            var sv = GetVariable(DyTypeNames.Array, node);
            cw.PushVar(sv);
            cw.GetMember(GetMemberNameId(Builtins.New));
            cw.FunPrep(node.Elements.Count);

            for (var i = 0; i < node.Elements.Count; i++)
            {
                Build(node.Elements[i], hints.Append(Push), ctx);
                cw.FunArgIx(i);
            }

            AddLinePragma(node);
            cw.FunCall(node.Elements.Count);
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
            var r = new Reference(node.ModuleName, node.Dll, node.Location, unit.FileName);
            var res = linker.Link(r);

            if (res.Success)
            {
                var referencedUnit = new UnitInfo(unit.UnitIds.Count, res.Value);
                unit.References.Add(res.Value);
                referencedUnits.Add(node.Alias ?? node.ModuleName, referencedUnit);

                foreach (var n in res.Value.ExportList)
                {
                    var imp = new ImportedName(node.ModuleName, unit.UnitIds.Count, n);

                    if (imports.ContainsKey(n.Name))
                        imports[n.Name] = imp;
                    else
                        imports.Add(n.Name, imp);
                }

                for (var i = 0; i < res.Value.TypeNames.Count; i++)
                {
                    var name = res.Value.TypeNames[i];
                    var ti = new TypeInfo(res.Value.TypeIds[i], referencedUnit);

                    if (types.ContainsKey(name))
                        types[name] = ti;
                    else
                        types.Add(name, ti);
                }

                cw.RunMod(unit.UnitIds.Count);
                unit.UnitIds.Add(-1); //Real handles are added by a linker

                var addr = AddVariable(node.Alias ?? node.ModuleName, node.Location, VarFlags.Module);
                cw.PopVar(addr);
            }
        }

        private void Build(DBreak node, Hints hints, CompilerContext ctx)
        {
            if (ctx.BlockBreakExit.IsEmpty())
                AddError(CompilerError.NoEnclosingLoop, node.Location);

            if (node.Expression != null)
                Build(node.Expression, hints.Append(Push), ctx);
            else
                cw.PushNil();

            AddLinePragma(node);
            cw.Br(ctx.BlockBreakExit);
        }

        private void Build(DReturn node, Hints hints, CompilerContext ctx)
        {
            if (ctx.FunctionExit.IsEmpty())
                AddError(CompilerError.ReturnNotAllowed, node.Location);

            if (node.Expression != null)
                Build(node.Expression, hints, ctx);
            else
                cw.PushNil();

            AddLinePragma(node);
            cw.Br(ctx.FunctionExit);
        }

        private void Build(DContinue node, Hints hints, CompilerContext ctx)
        {
            if (ctx.BlockSkip.IsEmpty())
                AddError(CompilerError.NoEnclosingLoop, node.Location);

            AddLinePragma(node);
            cw.Br(ctx.BlockSkip);
            PushIf(hints);
        }

        private void Build(DWhile node, Hints hints, CompilerContext ctx)
        {
            ctx = new CompilerContext(ctx)
            {
                BlockSkip = cw.DefineLabel(),
                BlockExit = cw.DefineLabel(),
                BlockBreakExit = cw.DefineLabel()
            };
            var iter = cw.DefineLabel();

            cw.MarkLabel(iter);
            Build(node.Condition, hints.Append(Push), ctx);
            cw.Brfalse(ctx.BlockExit);

            Build(node.Body, hints.Remove(Push), ctx);

            cw.MarkLabel(ctx.BlockSkip);
            cw.Br(iter);

            cw.MarkLabel(ctx.BlockExit);
            PushIf(hints);
            AddLinePragma(node);
            cw.MarkLabel(ctx.BlockBreakExit);
            cw.Nop();
        }

        private void Build(DFor node, Hints hints, CompilerContext ctx)
        {
            ctx = new CompilerContext(ctx)
            {
                BlockSkip = cw.DefineLabel(),
                BlockExit = cw.DefineLabel(),
                BlockBreakExit = cw.DefineLabel()
            };
            StartScope(false, node.Location);

            var inc = -1;

            if (node.Pattern.NodeType == NodeType.NamePattern)
                inc = AddVariable(node.Pattern.GetName(), node.Pattern, VarFlags.Const);

            var sys = AddVariable();
            var skip = cw.DefineLabel();
            Build(node.Target, hints.Append(Push), ctx);

            cw.Briter(skip);

            cw.GetMember(GetMemberNameId(Builtins.Iterator));

            cw.FunPrep(0);
            cw.FunCall(0);

            cw.MarkLabel(skip);
            cw.PopVar(sys);

            var iter = cw.DefineLabel();
            cw.MarkLabel(iter);
            cw.PushVar(new ScopeVar(sys));

            cw.FunPrep(0);
            cw.FunCall(0);

            cw.Brterm(ctx.BlockExit);

            if (inc > -1)
                cw.PopVar(inc);
            else
            {
                BuildPattern(node.Pattern, ctx);
                cw.Brfalse(ctx.BlockSkip);
            }

            Build(node.Body, hints.Remove(Push), ctx);

            cw.MarkLabel(ctx.BlockSkip);
            cw.Br(iter);

            cw.MarkLabel(ctx.BlockExit);
            cw.Pop();
            PushIf(hints);
            AddLinePragma(node);
            cw.MarkLabel(ctx.BlockBreakExit);
            cw.Nop();
            EndScope();
        }

        private void Build(DApplication node, Hints hints, CompilerContext ctx)
        {
            var name = node.Target.NodeType == NodeType.Name ? node.Target.GetName() : null;
            var sv = name != null ? GetVariable(name, node, err: false) : ScopeVar.Empty;

            //Check if an application is in fact a built-in operator call
            if (name != null && sv.IsEmpty())
                if (name == "nameof")
                {
                    if (node.Arguments.Count != 1)
                    {
                        AddError(CompilerError.InvalidNameOfOperator, node.Location);
                        return;
                    }

                    var push = GetExpressionName(node.Arguments[0]);
                    cw.Push(push);
                    return;
                }
                else if (name == "typeof")
                {
                    if (node.Arguments.Count != 1)
                    {
                        AddError(CompilerError.InvalidTypeOfOperator, node.Location);
                        return;
                    }

                    Build(node.Arguments[0], hints.Append(Push), ctx);
                    cw.Type();
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
            }

            if (!sv.IsEmpty())
                cw.PushVar(sv);
            else
                Build(node.Target, hints.Append(Push), ctx);

            cw.FunPrep(node.Arguments.Count);

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var a = node.Arguments[i];

                if (a.NodeType == NodeType.Label)
                {
                    var la = (DLabelLiteral)a;
                    Build(la.Expression, hints.Append(Push), ctx);
                    cw.FunArgNm(la.Label);
                }
                else
                {
                    Build(a, hints.Append(Push), ctx);
                    cw.FunArgIx(i);
                }
            }

            cw.FunCall(node.Arguments.Count);
            PopIf(hints);
        }

        private void Build(DIf node, Hints hints, CompilerContext ctx)
        {
            var falseLabel = cw.DefineLabel();
            var skipLabel = cw.DefineLabel();
            var push = hints.Append(Push);

            Build(node.Condition, push, ctx);
            AddLinePragma(node);
            cw.Brfalse(falseLabel);
            Build(node.True, push, ctx);
            PopIf(hints);
            AddLinePragma(node);
            cw.Br(skipLabel);
            cw.MarkLabel(falseLabel);

            if (node.False != null)
            {
                Build(node.False, push, ctx);
                PopIf(hints);
            }
            else
                PushIf(hints);

            cw.MarkLabel(skipLabel);
            cw.Nop();
        }

        private void Build(DStringLiteral node, Hints hints, CompilerContext ctx)
        {
            if (node.Chunks == null && NoPush(node, hints))
                return;

            if (node.Chunks != null)
            {
                cw.PushVar(GetVariable(DyTypeNames.String, node));
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
                            else if (p.Root.Nodes.Count > 1)
                                AddError(CompilerError.CodeIslandMultipleExpressions, node.Location);
                            else
                                Build(p.Root.Nodes[0], hints.Append(Push), ctx);
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
            var sv = GetVariable(node.Value, node.Location);
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
                var push = hasPush && i == node.Nodes.Count - 1 ? hints.Append(Push) : hints.Remove(Push);
                Build(n, push, ctx);
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

            var flags = currentScope.IsGlobal ? VarFlags.Exported :  VarFlags.None;
            var a = AddVariable(node.Name, node, node.Constant ? flags | VarFlags.Const : flags);
            cw.PopVar(a);
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



        private int GetTypeHandle(Qualident name, Location loc)
        {
            var stdCode = -1;

            if (name.Parent == null)
                stdCode = DyType.GetTypeCodeByName(name.Local);

            if (stdCode > -1)
                return stdCode;

            if (name.Parent == null)
            {
                if (!types.TryGetValue(name.Local, out var ti))
                {
                    AddError(CompilerError.UndefinedType, loc, name.Local);
                    return 0;
                }
                else
                    return ti.Unit.Handle | ti.Handle << 8;
            }
            else
            {
                if (!referencedUnits.TryGetValue(name.Parent, out var ui))
                {
                    AddError(CompilerError.UndefinedModule, loc, name.Parent);
                    return 0;
                }
                else
                {
                    var ti = -1;

                    for (var i = 0; i < ui.Unit.TypeIds.Count; ti++)
                    {
                        if (ui.Unit.TypeNames[i] == name.Local)
                        {
                            ti = ui.Unit.TypeIds[i];
                            break;
                        }
                    }

                    if (ti == -1)
                        AddError(CompilerError.UndefinedType, loc, name);

                    return ui.Handle | ti << 8;
                }
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