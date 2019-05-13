using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime.Types;
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
            }
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

                var sv = GetParentVariable(node.Name, node);
                AddLinePragma(node);
                cw.PushVar(sv);
                PopIf(hints);
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
            var sv = GetVariable(Lang.CreateArrayName, node);
            cw.PushVar(sv);
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
                unit.UnitIds.Add(-1); //Реальные хэндлы добавляются линкером

                var addr = AddVariable(node.Alias ?? node.ModuleName, node.Location, VarFlags.Module);
                cw.PopVar(addr);
            }
        }

        private void Build(DBreak node, Hints hints, CompilerContext ctx)
        {
            if (ctx.BlockBreakExit.IsEmpty())
                AddError(CompilerError.NoEnclosingLoop, node.Location);

            if (node.Expression != null)
                Build(node.Expression, hints, ctx);
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

            var inc = AddVariable(node.Variable.Value, node.Variable, VarFlags.Const);
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

            cw.PopVar(inc);

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

        private void _Build(DApplication node, Hints hints, CompilerContext ctx)
        {
            var name = node.Target.NodeType == NodeType.Name ? node.Target.GetName() : null;
            var sv = name != null ? GetVariable(name, node, err: false) : ScopeVar.Empty;

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

            foreach (var a in node.Arguments)
                Build(a, hints.Append(Push), ctx);

            if (sv.IsEmpty())
                Build(node.Target, hints.Append(Push), ctx);
            else
                cw.PushVar(sv);

            AddLinePragma(node);
            cw.Call(node.Arguments.Count);

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
            AddLinePragma(node);
            cw.Push(node.Value);
            PopIf(hints);
        }

        private void Build(DCharLiteral node, Hints hints, CompilerContext ctx)
        {
            AddLinePragma(node);
            cw.Push(node.Value);
            PopIf(hints);
        }

        private void Build(DFloatLiteral node, Hints hints, CompilerContext ctx)
        {
            AddLinePragma(node);
            cw.Push(node.Value);
            PopIf(hints);
        }

        private void Build(DName node, Hints hints, CompilerContext ctx)
        {
            var sv = GetVariable(node.Value, node.Location);
            AddLinePragma(node);

            if (!hints.Has(Pop))
            {
                cw.PushVar(sv);
                PopIf(hints);
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
            AddLinePragma(node);
            cw.Push(node.Value);
            PopIf(hints);
        }

        private void Build(DBooleanLiteral node, Hints hints, CompilerContext ctx)
        {
            AddLinePragma(node);
            cw.Push(node.Value);
            PopIf(hints);
        }

        private void Build(DNilLiteral node, Hints hints, CompilerContext ctx)
        {
            AddLinePragma(node);
            cw.PushNil();
            PopIf(hints);
        }

        private void Build(DBlock node, Hints hints, CompilerContext ctx)
        {
            if (node.Nodes?.Count == 0)
            {
                if (hints.Has(Push))
                    cw.PushNil();
                return;
            }

            //Начинаем лексический скоуп времени компиляции
            StartScope(fun: false, loc: node.Location);

            for (var i = 0; i < node.Nodes.Count; i++)
            {
                var n = node.Nodes[i];
                var push = i == node.Nodes.Count - 1 ? hints.Append(Push) : hints.Remove(Push);
                Build(n, push, ctx);
            }

            EndScope();
            PopIf(hints);
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

        private int GetMemberNameId(string name)
        {
            if (!memberNames.TryGetValue(name, out var id))
            {
                id = unit.MemberIds.Count;
                memberNames.Add(name, id);
                unit.MemberIds.Add(-1);
                unit.MemberNames.Add(name);
            }

            return id;
        }

        private void Build(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            if (node.Name != null)
            {
                var flags = VarFlags.Const | VarFlags.Function;
                var addr = 0;

                if (!node.IsMemberFunction)
                    addr = AddVariable(node.Name, node, flags | VarFlags.Exported);

                BuildFunctionBody(node, hints, ctx);

                if (hints.Has(Push))
                    cw.Dup();

                if (node.IsMemberFunction)
                {
                    var realName = node.Name;

                    if (node.Parameters.Count == 0 && !node.IsStatic)
                    {
                        if (node.Name == Builtins.Sub)
                            realName = Builtins.Neg;
                        else if (node.Name == Builtins.Add)
                            realName = Builtins.Plus;
                    }

                    var nameId = GetMemberNameId(realName);
                    cw.Aux(nameId);
                    var code = GetTypeHandle(node.TypeName, node.Location);

                    if (node.IsStatic)
                        cw.SetMemberS(code);
                    else
                        cw.SetMember(code);
                }

                AddLinePragma(node);

                if (node.IsMemberFunction)
                    cw.Nop();
                else
                    cw.PopVar(addr);
            }
            else
            {
                BuildFunctionBody(node, hints, ctx);
                AddLinePragma(node);
                cw.Nop();
                PopIf(hints);
            }
        }

        private Par[] CompileFunctionParameters(List<DParameter> pars)
        {
            var arr = new Par[pars.Count];

            for (var i = 0; i < pars.Count; i++)
            {
                var p = pars[i];

                if (p.DefaultValue != null)
                {
                    if (p.IsVarArgs)
                    {
                        //TODO: Var args cannot have default values
                    }

                    DyObject val = null;

                    switch (p.DefaultValue.NodeType)
                    {
                        case NodeType.Integer:
                            val = new DyInteger(((DIntegerLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Float:
                            val = new DyFloat(((DFloatLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Char:
                            val = new DyChar(((DCharLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Boolean:
                            val = ((DBooleanLiteral)p.DefaultValue).Value ? DyBool.True : DyBool.False;
                            break;
                        case NodeType.String:
                            val = new DyString(((DStringLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Nil:
                            val = DyNil.Instance;
                            break;
                        default:
                            //Error: only primitive types are supported
                            break;
                    }

                    arr[i] = new Par(p.Name, val, false);
                }
                else
                    arr[i] = new Par(p.Name, null, p.IsVarArgs);
            }

            return arr;
        }

        private void BuildFunctionBody(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            var iter = hints.Has(Iterator);
            var args = CompileFunctionParameters(node.Parameters);
            var argCount = args.Length;
            StartFun(node.Name, args, argCount);

            if (node.IsStatic && !node.IsMemberFunction)
                AddError(CompilerError.StaticOnlyMethods, node.Location, node.Name);

            var startLabel = cw.DefineLabel();
            var funEndLabel = cw.DefineLabel();

            //Functions are always compiled "in place": if we find a function while looping
            //through AST node we compile right away. That is the reason why we need to emit an 
            //additional goto so that we can jump over this function.
            var funSkipLabel = cw.DefineLabel();
            cw.Br(funSkipLabel);

            ctx = new CompilerContext {
                FunctionExit = funEndLabel
            };

            hints = Function | Push;

            //Actual start of a function
            cw.MarkLabel(startLabel);

            //Start of a physical (and not compiler time) lexical scope for a function
            StartScope(fun: true, loc: node.Location);
            StartSection();

            AddLinePragma(node);
            var address = cw.Offset;
            var variadicIndex = -1;

            //Initialize function arguments
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.IsVarArg)
                    variadicIndex = i;

                AddVariable(arg.Name, node, data: VarFlags.Argument);
            }

            //If this is a member function we add an additional system variable that
            //would return an instance of an object to which this function is coupled
            //(same as this in C#)
            if (node.IsMemberFunction && !node.IsStatic)
            {
                var va = AddVariable("this", node, data: VarFlags.Const);
                cw.This();
                cw.PopVar(va);
            }

            //Compile function body
            if (node.IsIterator)
            {
                var dec = new DFunctionDeclaration(node.Location) { Name = node.Name, Body = node.Body };
                Build(dec, hints.Append(Iterator), ctx);
            }
            else
                Build(node.Body, hints, ctx);

            //Возвращаемся из функции. Кстати, любое исполнение функции доходит до сюда,
            //т.е. нельзя выйти раньше. Преждевременный return всё равно прыгает сюда, и здесь
            //уже исполняется реальный return (Ret). Т.е. это эпилог функции.
            cw.MarkLabel(funEndLabel);

            //If this is an iterator function push a terminator at the end (and pop a normal value)
            if (iter)
            {
                cw.Pop();
                cw.PushNilT();
            }

            cw.Ret();
            cw.MarkLabel(funSkipLabel);

            AddLinePragma(node);

            //Close all lexical scopes and debugging information
            var funHandle = unit.Layouts.Count;
            var ss = EndFun(funHandle);
            unit.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
            EndScope();
            EndSection();

            //Iterators are a separate type (based on function through)
            if (iter)
                cw.NewIter(funHandle);
            else
            {
                //cw.Push(argCount);
                //TODO: Variadic
                //cw.Push(node.IsVariadic ? argCount - 1 : argCount);

                if (variadicIndex > -1)
                {
                    cw.Aux(variadicIndex);
                    cw.NewFunV(funHandle);
                }
                else
                    cw.NewFun(funHandle);
            }
        }

        private int GetTypeHandle(Qualident name, Location loc)
        {
            var stdCode = -1;

            if (name.Parent == null)
                stdCode = StandardType.GetTypeCodeByName(name.Local);

            if (stdCode >= 0)
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