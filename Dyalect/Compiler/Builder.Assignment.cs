using Dyalect.Parser.Model;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler;

//This part contains compilation logic for assignments/compound assignments
partial class Builder
{
    private bool Build(DAssignment node, Hints hints, CompilerContext ctx)
    {
        ValidateAssignment(node);

        if (node.Target.NodeType is NodeType.Access && IsMemberAccess(node.Target) && node.AutoAssign is BinaryOperator.Coalesce)
            return BuildSetterCoalesce(node, hints, ctx);

        if (node.Target.NodeType is NodeType.Access && IsMemberAccess(node.Target) && node.AutoAssign is not null)
            return BuildSetterAutoAssign(node, hints, ctx);

        if (node.Target.NodeType is NodeType.Access && IsMemberAccess(node.Target))
            return BuildSetter(node, hints, ctx);

        if (node.AutoAssign is BinaryOperator.Coalesce)
            return BuildCoalesce(node, hints, ctx);

        if (node.AutoAssign is not null)
            return BuildAutoAssign(node, hints, ctx);

        return BuildAssignment(node, hints, ctx);
    }

    private bool IsMemberAccess(DNode node) => char.IsUpper(((DAccess)node).Name[0]);

    private void EmitGetter(DNode target, string field, Hints hints, CompilerContext ctx)
    {
        Build(target, hints.Remove(Last).Append(Push), ctx);
        cw.GetMember(field);
    }

    private void EmitSetter(DNode target, DNode value, string field, Hints hints, CompilerContext ctx)
    {
        Build(target, hints.Append(Push), ctx);
        cw.GetMember(Builtins.Setter(field));
        cw.FunPrep(1);
        Build(value, hints.Append(Push), ctx);
        cw.FunArgIx(0);
        cw.FunCall(1);
        PopIf(hints);
    }

    private bool BuildSetter(DAssignment node, Hints hints, CompilerContext ctx)
    {
        var acc = (DAccess)node.Target;
        EmitSetter(acc.Target, node.Value, acc.Name, hints, ctx);
        return true;
    }

    private bool BuildSetterCoalesce(DAssignment node, Hints hints, CompilerContext ctx)
    {
        var exitLab = cw.DefineLabel();
        var acc = (DAccess)node.Target;
        EmitGetter(acc.Target, acc.Name, hints, ctx);
        cw.Brtrue(exitLab);
        EmitSetter(acc.Target, node.Value, acc.Name, hints.Remove(Push), ctx);
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
        cw.GetMember(Builtins.Setter(acc.Name));
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

        if (ErrorCount == 0) //To avoid double error reporting
            Build(node.Target, hints.Append(Pop), ctx);

        PushIf(hints);
        return true;
    }

    private bool BuildAssignment(DAssignment node, Hints hints, CompilerContext ctx)
    {
        Build(node.Value, hints.Append(Push), ctx);
        Build(node.Target, hints.Remove(Push).Append(Pop), ctx);
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
            && node.Target is INamedNode nn1
            && node.Value is INamedNode nn2
            && nn1.NodeName == nn2.NodeName
            && node.AutoAssign is null)
            AddWarning(CompilerWarning.AssignmentSameVariable, node.Location);
    }
}
