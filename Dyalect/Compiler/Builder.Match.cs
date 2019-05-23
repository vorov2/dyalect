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

            var sys = AddVariable();
            var push = hints.Append(Push);
            Build(node.Expression, push, ctx);
            cw.PopVar(sys);
            var sysVar = new ScopeVar(sys);

            foreach (var e in node.Entries)
                BuildEntry(e, sysVar, push, ctx);

            cw.Push("Match failed.");
            cw.Fail();
            cw.MarkLabel(ctx.MatchExit);
            cw.Nop();
            EndScope();
        }

        private void BuildEntry(DMatchEntry node, ScopeVar sys, Hints hints, CompilerContext ctx)
        {
            StartScope(fun: false, node.Location);
            var skip = cw.DefineLabel();

            cw.PushVar(sys);
            BuildPattern(node.Pattern, ctx);
            cw.Brfalse(skip);

            if (node.Guard != null)
            {
                Build(node.Guard, hints, ctx);
                cw.Brfalse(skip);
            }

            Build(node.Expression, hints, ctx);
            cw.Br(ctx.MatchExit);
            cw.MarkLabel(skip);
            cw.Nop();
            EndScope();
        }

        private void BuildPattern(DPattern node, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
                case NodeType.NamePattern:
                    BuildName((DNamePattern)node, ctx);
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
                    BuildSequence(node, ((DTuplePattern)node).Elements, ctx);
                    break;
                case NodeType.RecordPattern:
                    BuildRecord((DRecordPattern)node, ctx);
                    break;
                case NodeType.ArrayPattern:
                    BuildSequence(node, ((DArrayPattern)node).Elements, ctx);
                    break;
                case NodeType.NilPattern:
                    cw.PushNil();
                    cw.Eq();
                    break;
                case NodeType.RangePattern:
                    BuildRange((DRangePattern)node, ctx);
                    break;
                case NodeType.WildcardPattern:
                    cw.Pop();
                    cw.Push(true);
                    break;
                case NodeType.AsPattern:
                    BuildAs((DAsPattern)node, ctx);
                    break;
                case NodeType.TypeTestPattern:
                    cw.TypeCheck(GetTypeHandle(((DTypeTestPattern)node).TypeName, node.Location));
                    break;
                case NodeType.AndPattern:
                    BuildAnd((DAndPattern)node, ctx);
                    break;
                case NodeType.OrPattern:
                    BuildOr((DOrPattern)node, ctx);
                    break;
            }
        }

        private void BuildAs(DAsPattern node, CompilerContext ctx)
        {
            cw.Dup();

            BuildPattern(node.Pattern, ctx);
            var bad = cw.DefineLabel();
            var ok = cw.DefineLabel();
            cw.Brfalse(bad);

            if (!GetLocalVariable(node.Name, out var sv))
                sv = AddVariable(node.Name, node, VarFlags.None);

            cw.PopVar(sv);
            cw.Push(true);
            cw.Br(ok);
            cw.MarkLabel(bad);
            cw.Pop();
            cw.Push(false);
            cw.MarkLabel(ok);
            cw.Nop();
        }

        private void BuildName(DNamePattern node, CompilerContext ctx)
        {
            var err = GetTypeHandle(null, node.Name, out var handle);

            if (err == CompilerError.None)
                cw.TypeCheck(handle);
            else
            {
                if (!GetLocalVariable(node.Name, out var sv))
                    sv = AddVariable(node.Name, node, VarFlags.None);
                cw.PopVar(sv);
                cw.Push(true);
            }
        }

        private void BuildAnd(DAndPattern node, CompilerContext ctx)
        {
            cw.Dup();
            BuildPattern(node.Left, ctx);
            var termLab = cw.DefineLabel();
            var exitLab = cw.DefineLabel();
            cw.Brfalse(termLab);
            BuildPattern(node.Right, ctx);
            AddLinePragma(node);
            cw.Br(exitLab);
            cw.MarkLabel(termLab);
            cw.Pop();
            AddLinePragma(node);
            cw.Push(false);
            cw.MarkLabel(exitLab);
            cw.Nop();
        }

        private void BuildOr(DOrPattern node, CompilerContext ctx)
        {
            cw.Dup();
            BuildPattern(node.Left, ctx);
            var termLab = cw.DefineLabel();
            var exitLab = cw.DefineLabel();
            cw.Brtrue(termLab);
            BuildPattern(node.Right, ctx);
            AddLinePragma(node);
            cw.Br(exitLab);
            cw.MarkLabel(termLab);
            cw.Pop();
            AddLinePragma(node);
            cw.Push(true);
            cw.MarkLabel(exitLab);
            cw.Nop();
        }

        private void BuildRange(DRangePattern node, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var exit = cw.DefineLabel();

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId("<"));
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId(">"));
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            BuildRangeElement(node.From);
            cw.GtEq();
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            BuildRangeElement(node.To);
            cw.LtEq();
            cw.Brfalse(skip); //1 left

            cw.Push(true);
            cw.Pop(); //0 left
            cw.Br(exit);

            cw.MarkLabel(skip);
            cw.Pop(); //0 left
            cw.Push(false);

            cw.MarkLabel(exit);
            cw.Nop();
        }

        private void BuildRangeElement(DPattern node)
        {
            switch (node.NodeType)
            {
                case NodeType.IntegerPattern:
                    cw.Push(((DIntegerPattern)node).Value);
                    break;
                case NodeType.FloatPattern:
                    cw.Push(((DFloatPattern)node).Value);
                    break;
                case NodeType.BooleanPattern:
                    cw.Push(((DBooleanPattern)node).Value);
                    break;
                case NodeType.CharPattern:
                    cw.Push(((DCharPattern)node).Value);
                    break;
                case NodeType.StringPattern:
                    cw.Push(((DStringPattern)node).Value.Value);
                    break;
                case NodeType.NilPattern:
                    cw.PushNil();
                    break;
                default:
                    AddError(CompilerError.PatternNotSupported, node.Location, node);
                    break;
            }
        }

        private void BuildSequence(DPattern node, List<DPattern> elements, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var ok = cw.DefineLabel();

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId("len"));
            cw.Brfalse(skip); //1 obj left to pop
            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId("get"));
            cw.Brfalse(skip); //1 obj left to pop

            cw.Dup(); //2 objs
            cw.Len();
            cw.Push(elements.Count);
            if (node.NodeType == NodeType.TuplePattern) cw.Eq(); else cw.GtEq();
            cw.Brfalse(skip); //1 obj left to pop

            for (var i = 0; i < elements.Count; i++)
            {
                cw.Dup(); //2 objs
                var e = elements[i];
                cw.Get(i);
                BuildPattern(e, ctx);
                cw.Brfalse(skip); //1 obj left to pop
            }

            cw.Pop(); //0 objs
            cw.Push(true);
            cw.Br(ok);
            cw.MarkLabel(skip);
            cw.Pop(); //0 objs
            cw.Push(false);
            cw.MarkLabel(ok);
            cw.Nop();
        }

        private void BuildRecord(DRecordPattern node, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var ok = cw.DefineLabel();

            for (var i = 0; i < node.Elements.Count; i++)
            {
                cw.Dup(); //2 objs

                var e = node.Elements[i];
                cw.HasField(e.Label);
                cw.Brfalse(skip);

                cw.Dup(); //2 objs
                cw.Push(e.Label);
                cw.Get();
                BuildPattern(e.Pattern, ctx);
                cw.Brfalse(skip); //1 obj left
            }

            cw.Pop(); //0 objs
            cw.Push(true);
            cw.Br(ok);
            cw.MarkLabel(skip);
            cw.Pop(); //0 objs
            cw.Push(false);
            cw.MarkLabel(ok);
            cw.Nop();
        }
    }
}
