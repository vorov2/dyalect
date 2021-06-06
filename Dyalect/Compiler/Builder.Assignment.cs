using Dyalect.Parser.Model;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //This part contains compilation of assignments/compound assignments
    partial class Builder
    {
        private bool Build(DAssignment node, Hints hints, CompilerContext ctx)
        {
            ValidateAssignment(node);

            if (node.Target.NodeType is NodeType.Access && node.AutoAssign is BinaryOperator.Coalesce)
                return BuildSetterCoalesce(node, hints, ctx);

            if (node.Target.NodeType is NodeType.Access && node.AutoAssign is not null)
                return BuildSetterAutoAssign(node, hints, ctx);

            if (node.Target.NodeType is NodeType.Access)
                return BuildSetter(node, hints, ctx);

            if (node.AutoAssign == BinaryOperator.Coalesce)
                return BuildCoalesce(node, hints, ctx);

            if (node.AutoAssign is not null)
                return BuildAutoAssign(node, hints, ctx);

            return BuildAssignment(node, hints, ctx);
        }

        private void EmitGetter(DNode target, string field, Hints hints, CompilerContext ctx)
        {
            Build(target, hints.Remove(Last).Append(Push), ctx);
            cw.GetMember(field);
        }

        private void EmitSetter(DNode target, DNode value, string field, Hints hints, CompilerContext ctx)
        {
            Build(target, hints.Append(Push), ctx);
            cw.GetMember("set_" + field);
            cw.FunPrep(1);
            Build(value, hints.Append(Push), ctx);
            cw.FunArgIx(0);
            cw.FunCall(1);
        }

        private bool BuildSetter(DAssignment node, Hints hints, CompilerContext ctx)
        {
            var acc = (DAccess)node.Target;
            EmitSetter(acc.Target, node.Value, acc.Name, hints, ctx);
            PopIf(hints);
            return true;
        }

        private bool BuildSetterCoalesce(DAssignment node, Hints hints, CompilerContext ctx)
        {
            var exitLab = cw.DefineLabel();
            var acc = (DAccess)node.Target;
            EmitGetter(acc.Target, acc.Name, hints, ctx);
            cw.Brtrue(exitLab);
            EmitSetter(acc.Target, node.Value, acc.Name, hints, ctx);
            cw.MarkLabel(exitLab);
            cw.Nop();
            PushIf(hints);
            return true;
        }

        private bool BuildCoalesce(DAssignment node, Hints hints, CompilerContext ctx)
        {
            var exitLab = cw.DefineLabel();
            Build(node.Target, hints.Remove(Last).Append(Push), ctx);
            cw.Brtrue(exitLab);
            Build(node.Value, hints.Append(Push), ctx);
            Build(node.Target, hints.Append(Pop), ctx);
            cw.MarkLabel(exitLab);
            cw.Nop();
            PushIf(hints);
            return true;
        }

        private bool BuildSetterAutoAssign(DAssignment node, Hints hints, CompilerContext ctx)
        {
            var acc = (DAccess)node.Target;
            Build(acc.Target, hints.Remove(Last).Append(Push), ctx);
            cw.GetMember("set_" + acc.Name);
            cw.FunPrep(1);
            EmitGetter(acc.Target, acc.Name, hints, ctx);
            Build(node.Value, hints.Append(Push), ctx);
            EmitBinaryOp(node.AutoAssign!.Value);
            cw.FunArgIx(0);
            cw.FunCall(1);
            PopIf(hints);
            return true;
        }

        private bool BuildAutoAssign(DAssignment node, Hints hints, CompilerContext ctx)
        {
            Build(node.Target, hints.Append(Push), ctx);
            Build(node.Value, hints.Append(Push), ctx);
            EmitBinaryOp(node.AutoAssign!.Value);
            Build(node.Target, hints.Append(Pop), ctx);
            PushIf(hints);
            return true;
        }

        private bool BuildAssignment(DAssignment node, Hints hints, CompilerContext ctx)
        {
            Build(node.Value, hints.Append(Push), ctx);
            Build(node.Target, hints.Append(Pop), ctx);
            PushIf(hints);
            return true;
        }

        private void ValidateAssignment(DAssignment node)
        {
            if (node.Target.NodeType != NodeType.Name
                && node.Target.NodeType != NodeType.Index
                && node.Target.NodeType != NodeType.Access)
                AddError(CompilerError.UnableAssignExpression, node.Target.Location, node.Target);

            if (node.Target.NodeType == node.Value.NodeType
                && node.Target.NodeType == NodeType.Name
                && node.Target.GetName() == node.Value.GetName()
                && node.AutoAssign is null)
                AddWarning(CompilerWarning.AssignmentSameVariable, node.Location);
        }
    }
}
