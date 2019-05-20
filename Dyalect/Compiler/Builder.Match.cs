using Dyalect.Parser;
using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //Contains compilation logic for pattern matching
    partial class Builder
    {
        private void Build(DMatch node, Hints hints, CompilerContext ctx)
        {
            StartScope(fun: false, node.Location);

            ctx = new CompilerContext(ctx)
            {
                MatchExit = cw.DefineLabel()
            };

            var push = hints.Append(Push);
            Build(node.Expression, push, ctx);

            foreach (var e in node.Entries)
                BuildEntry(e, push, ctx);

            cw.Push("Match failed.");
            cw.Fail();
            cw.MarkLabel(ctx.MatchExit);
            cw.Nop();
            EndScope();
        }

        private void BuildEntry(DMatchEntry node, Hints hints, CompilerContext ctx)
        {
            StartScope(fun: false, node.Location);
            var skip = cw.DefineLabel();

            if (node.Guard != null)
            {
                Build(node.Guard, hints, ctx);
                cw.Brfalse(skip);
            }

            BuildPattern(node.Pattern, ctx);
            cw.Brfalse(skip);

            cw.Pop();
            Build(node.Expression, hints, ctx);
            cw.Br(ctx.MatchExit);
            cw.MarkLabel(skip);
            cw.Nop();
            EndScope();
        }

        private void BuildPattern(DPattern node, CompilerContext ctx)
        {
            cw.Dup();

            switch (node.NodeType)
            {
                case NodeType.NamePattern:
                    var va = AddVariable(((DNamePattern)node).Name, node, VarFlags.None);
                    cw.PopVar(va);
                    cw.Push(true);
                    break;
                case NodeType.IntegerPattern:
                    cw.Push(((DIntegerPattern)node).Value);
                    cw.Eq();
                    break;
                case NodeType.StringPattern:
                    Build(((DStringPattern)node).Value, Push, ctx);
                    cw.Eq();
                    break;
                case NodeType.FloatPattern:
                    cw.Push(((DFloatPattern)node).Value);
                    cw.Eq();
                    break;
                case NodeType.CharPattern:
                    cw.Push(((DCharPattern)node).Value);
                    cw.Eq();
                    break;
                case NodeType.BooleanPattern:
                    cw.Push(((DBooleanPattern)node).Value);
                    cw.Eq();
                    break;
                case NodeType.TuplePattern:
                    BuildTuple((DTuplePattern)node, ctx);
                    break;
            }
        }

        private void BuildTuple(DTuplePattern node, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var ok = cw.DefineLabel();
            var hasPos = false;

            foreach (var e in node.Elements)
            {
                if (e.NodeType != NodeType.LabelPattern)
                {
                    hasPos = true;
                    break;
                }
            }

            if (hasPos)
            {
                cw.Len();
                cw.Push(node.Elements.Count);
                cw.Eq();
                cw.Brtrue(ok);
                cw.Push(false);
                cw.Br(skip);
                cw.MarkLabel(ok);
                cw.Dup();
            }

            for (var i = 0; i < node.Elements.Count; i++)
            {
                if (i > 0)
                    cw.Dup();

                var e = node.Elements[i];

                if (e.NodeType == NodeType.Label)
                {
                    var lab = (DLabelPattern)e;
                    cw.HasField(lab.Label);
                    cw.Brfalse(skip);
                    cw.Dup();
                    cw.Push(lab.Label);
                    cw.Get();
                    BuildPattern(lab.Pattern, ctx);
                }
                else
                {
                    cw.Get(i);
                    BuildPattern(e, ctx);
                }

                cw.Eq();
                cw.Brfalse(skip);
            }

            cw.Push(true);
            cw.MarkLabel(skip);
            cw.Nop();
        }
    }
}
