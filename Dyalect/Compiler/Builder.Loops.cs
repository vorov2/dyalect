using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //Contains compilation logic for loops
    partial class Builder
    {
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

            if (node.Pattern.NodeType == NodeType.NamePattern
                && GetTypeHandle(null, node.Pattern.GetName(), out var _) != CompilerError.None)
                inc = AddVariable(node.Pattern.GetName(), node.Pattern, VarFlags.Const);

            var sys = AddVariable();
            var initSkip = cw.DefineLabel();
            Build(node.Target, hints.Append(Push), ctx);

            cw.Briter(initSkip);

            cw.GetMember(GetMemberNameId(Builtins.Iterator));

            cw.FunPrep(0);
            cw.FunCall(0);

            cw.MarkLabel(initSkip);
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
                BuildPattern(node.Pattern, hints, ctx);
                cw.Brfalse(ctx.BlockSkip);
            }

            if (node.Guard != null)
            {
                Build(node.Guard, hints.Append(Push), ctx);
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
    }
}
