using Dyalect.Parser.Model;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //Contains compilation logic for loops
    partial class Builder
    {
        private void Build(DBreak node, Hints hints, CompilerContext ctx)
        {
            if (ctx.BlockExit.IsEmpty())
                AddError(CompilerError.NoEnclosingLoop, node.Location);

            if (node.Expression is not null)
                Build(node.Expression, hints.Append(Push), ctx);
            else
                cw.PushNil();

            CallAutosForKind(ScopeKind.Loop);
            AddLinePragma(node);
            cw.Br(ctx.BlockBreakExit);
        }

        private void Build(DContinue node, Hints hints, CompilerContext ctx)
        {
            if (ctx.BlockSkip.IsEmpty())
                AddError(CompilerError.NoEnclosingLoop, node.Location);

            CallAutosForKind(ScopeKind.Loop); 
            AddLinePragma(node);
            cw.Br(ctx.BlockSkip);
            PushIf(hints);
        }

        private void Build(DWhile node, Hints hints, CompilerContext ctx)
        {
            ctx = new(ctx)
            {
                BlockSkip = cw.DefineLabel(),
                BlockExit = cw.DefineLabel(),
                BlockBreakExit = cw.DefineLabel()
            };
            StartScope(ScopeKind.Loop, node.Location);
            var iter = cw.DefineLabel();
            hints = hints.Remove(Last);
            var nh = hints.Remove(Push);

            if (node.DoWhile)
                Build(node.Body, nh, ctx);

            cw.MarkLabel(iter);
            Build(node.Condition, hints.Append(Push), ctx);
            cw.Brfalse(ctx.BlockExit);

            Build(node.Body, nh, ctx);

            cw.MarkLabel(ctx.BlockSkip);
            cw.Br(iter);

            cw.MarkLabel(ctx.BlockExit);
            cw.PushNil();
            AddLinePragma(node);

            cw.MarkLabel(ctx.BlockBreakExit);
            PopIf(hints);
            cw.Nop();
            EndScope();
        }

        private void Build(DFor node, Hints hints, CompilerContext ctx)
        {
            ctx = new(ctx)
            {
                BlockSkip = cw.DefineLabel(),
                BlockExit = cw.DefineLabel(),
                BlockBreakExit = cw.DefineLabel()
            };
            StartScope(ScopeKind.Loop, node.Location);
            hints = hints.Remove(Last);

            var inc = false;

            if (node.Pattern.NodeType == NodeType.NamePattern && !char.IsUpper(node.Pattern.GetName()![0]!))
                inc = true;

            var sys = AddVariable();
            var initSkip = cw.DefineLabel();
            Build(node.Target, hints.Append(Push), ctx);
            cw.Briter(initSkip);
            cw.GetMember(Builtins.Iterator);
            cw.CallNullaryFunction();

            cw.MarkLabel(initSkip);
            cw.GetIter();
            cw.PopVar(sys);

            var iter = cw.DefineLabel();
            cw.MarkLabel(iter);
            cw.PushVar(new ScopeVar(sys));
            cw.CallNullaryFunction();
            cw.Brterm(ctx.BlockExit);

            if (inc)
            {
                var ai = AddVariable(node.Pattern.GetName()!, node.Pattern.Location, VarFlags.None);
                cw.PopVar(ai);
            }
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

            var nh = hints.Remove(Push);
            Build(node.Body, nh, ctx);

            cw.MarkLabel(ctx.BlockSkip);
            cw.Br(iter);

            cw.MarkLabel(ctx.BlockExit);
            cw.Pop();
            cw.PushNil();
            AddLinePragma(node);

            cw.MarkLabel(ctx.BlockBreakExit);
            PopIf(hints);
            cw.Nop();
            EndScope();
        }
    }
}
