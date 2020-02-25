﻿using Dyalect.Parser;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
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
            ValidateMatch(node);
            StartScope(fun: false, node.Location);

            ctx = new CompilerContext(ctx)
            {
                MatchExit = cw.DefineLabel()
            };

            var sys = AddVariable();
            var push = hints.Append(Push);

            if (node.Expression != null)
                Build(node.Expression, push.Remove(Last), ctx);

            cw.PopVar(sys);
            var sysVar = new ScopeVar(sys);

            foreach (var e in node.Entries)
                BuildEntry(e, sysVar, push, ctx);

            //It is some kind of a hack, but Expression can be null
            //only if this match is inside try/catch
            if (node.Expression != null)
                cw.Fail(DyErrorCode.MatchFailed);
            else
            {
                cw.PushVar(sysVar);
                cw.Fail();
            }

            cw.MarkLabel(ctx.MatchExit);
            cw.Nop();
            PopIf(hints);
            EndScope();
        }

        private void BuildEntry(DMatchEntry node, ScopeVar sys, Hints hints, CompilerContext ctx)
        {
            StartScope(fun: false, node.Location);
            var skip = cw.DefineLabel();

            cw.PushVar(sys);
            BuildPattern(node.Pattern, hints.Remove(Last), ctx);
            cw.Brfalse(skip);

            if (node.Guard != null)
            {
                Build(node.Guard, hints.Remove(Last), ctx);
                cw.Brfalse(skip);
            }

            Build(node.Expression, hints, ctx);
            cw.Br(ctx.MatchExit);
            cw.MarkLabel(skip);
            cw.Nop();
            EndScope();
        }

        private void BuildPattern(DPattern node, Hints hints, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
                case NodeType.NamePattern:
                    BuildName((DNamePattern)node, hints, ctx);
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
                    BuildSequence(node, ((DTuplePattern)node).Elements, hints, ctx);
                    break;
                case NodeType.ArrayPattern:
                    BuildSequence(node, ((DArrayPattern)node).Elements, hints, ctx);
                    break;
                case NodeType.NilPattern:
                    cw.PushNil();
                    cw.Eq();
                    break;
                case NodeType.RangePattern:
                    BuildRange((DRangePattern)node, hints, ctx);
                    break;
                case NodeType.WildcardPattern:
                    cw.Pop();
                    cw.Push(true);
                    break;
                case NodeType.AsPattern:
                    BuildAs((DAsPattern)node, hints, ctx);
                    break;
                case NodeType.TypeTestPattern:
                    cw.TypeCheck(GetTypeHandle(((DTypeTestPattern)node).TypeName, node.Location));
                    break;
                case NodeType.AndPattern:
                    BuildAnd((DAndPattern)node, hints, ctx);
                    break;
                case NodeType.OrPattern:
                    BuildOr((DOrPattern)node, hints, ctx);
                    break;
                case NodeType.MethodCheckPattern:
                    BuildMethodCheck((DMethodCheckPattern)node, hints, ctx);
                    break;
                case NodeType.CtorPattern:
                    BuildCtor((DCtorPattern)node, hints, ctx);
                    break;
                case NodeType.LabelPattern:
                    BuildLabel((DLabelPattern)node, hints, ctx);
                    break;
            }
        }

        private void BuildCtor(DCtorPattern node, Hints hints, CompilerContext ctx)
        {
            var bad = cw.DefineLabel();
            var ok = cw.DefineLabel();

            cw.Dup();

            var nameId = GetMemberNameId(node.Constructor);
            cw.CtorCheck(nameId);
            cw.Brfalse(bad);

            if (node.Arguments == null || node.Arguments.Count == 0)
            {
                cw.Pop();
                cw.Push(true);
            }
            else
                BuildSequence(node, node.Arguments, hints, ctx);

            cw.Br(ok);

            cw.MarkLabel(bad);
            AddLinePragma(node);
            cw.Pop();
            cw.Push(false);

            cw.MarkLabel(ok);
        }

        private void BuildMethodCheck(DMethodCheckPattern node, Hints hints, CompilerContext ctx)
        {
            AddLinePragma(node);
            var nameId = GetMemberNameId(node.Name);
            cw.HasMember(nameId);
        }

        private void BuildAs(DAsPattern node, Hints hints, CompilerContext ctx)
        {
            cw.Dup();

            BuildPattern(node.Pattern, hints, ctx);
            var bad = cw.DefineLabel();
            var ok = cw.DefineLabel();
            cw.Brfalse(bad);
            var sva = -1;

            if (!TryGetLocalVariable(node.Name, out var sv))
                sva = AddVariable(node.Name, node, VarFlags.None);
            else
                sva = sv.Address;

            cw.PopVar(sva);
            cw.Push(true);
            cw.Br(ok);
            cw.MarkLabel(bad);
            cw.Pop();
            cw.Push(false);
            cw.MarkLabel(ok);
            cw.Nop();
        }

        private void BuildName(DNamePattern node, Hints hints, CompilerContext ctx)
        {
            var err = GetTypeHandle(null, node.Name, out var handle, out var std);

            if (err == CompilerError.None)
                cw.TypeCheck(new TypeHandle(handle, std));
            else
            {
                ScopeVar sv = default;
                var found = hints.Has(Rebind)
                    ? TryGetVariable(node.Name, out sv)
                    : hints.Has(OpenMatch) ? false : TryGetLocalVariable(node.Name, out sv);
                var sva = sv.Address;

                if (!found)
                    sva = AddVariable(node.Name, node, hints.Has(Const) ? VarFlags.Const : VarFlags.None);
                else if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                    AddError(CompilerError.UnableAssignConstant, node.Location, node.Name);

                cw.PopVar(sva);
                cw.Push(true);
            }
        }

        private void BuildAnd(DAndPattern node, Hints hints, CompilerContext ctx)
        {
            cw.Dup();
            BuildPattern(node.Left, hints, ctx);
            var termLab = cw.DefineLabel();
            var exitLab = cw.DefineLabel();
            cw.Brfalse(termLab);
            BuildPattern(node.Right, hints, ctx);
            AddLinePragma(node);
            cw.Br(exitLab);
            cw.MarkLabel(termLab);
            cw.Pop();
            AddLinePragma(node);
            cw.Push(false);
            cw.MarkLabel(exitLab);
            cw.Nop();
        }

        private void BuildOr(DOrPattern node, Hints hints, CompilerContext ctx)
        {
            cw.Dup();
            BuildPattern(node.Left, hints, ctx);
            var termLab = cw.DefineLabel();
            var exitLab = cw.DefineLabel();
            cw.Brtrue(termLab);
            BuildPattern(node.Right, hints, ctx);
            AddLinePragma(node);
            cw.Br(exitLab);
            cw.MarkLabel(termLab);
            cw.Pop();
            AddLinePragma(node);
            cw.Push(true);
            cw.MarkLabel(exitLab);
            cw.Nop();
        }

        private void BuildRange(DRangePattern node, Hints hints, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var exit = cw.DefineLabel();

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId(Builtins.Lt));
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId(Builtins.Gt));
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            BuildRangeElement(node.From, hints);
            cw.GtEq();
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            BuildRangeElement(node.To, hints);
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

        private void BuildRangeElement(DPattern node, Hints hints)
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

        private void BuildSequence(DPattern node, List<DNode> elements, Hints hints, CompilerContext ctx)
        {
            var skip = cw.DefineLabel();
            var ok = cw.DefineLabel();

            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId(Builtins.Len));
            cw.Brfalse(skip); //1 obj left to pop
            cw.Dup(); //2 objs
            cw.HasMember(GetMemberNameId(Builtins.Get));
            cw.Brfalse(skip); //1 obj left to pop

            cw.Dup(); //2 objs
            cw.Len();
            cw.Push(elements.Count);
            if (node.NodeType == NodeType.TuplePattern || node.NodeType == NodeType.CtorPattern) cw.Eq(); else cw.GtEq();
            cw.Brfalse(skip); //1 obj left to pop

            for (var i = 0; i < elements.Count; i++)
            {
                cw.Dup(); //2 objs
                var e = elements[i];

                if (e.NodeType == NodeType.LabelPattern)
                    BuildLabel((DLabelPattern)e, hints, ctx);
                else
                {
                    cw.Get(i);
                    BuildPattern((DPattern)e, hints, ctx);
                }

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

        private void BuildLabel(DLabelPattern node, Hints hints, CompilerContext ctx)
        {
            var bad = cw.DefineLabel();
            var skip = cw.DefineLabel();
            cw.Dup();
            cw.HasField(node.Label);
            cw.Brfalse(bad);

            cw.Push(node.Label);
            cw.Get();
            BuildPattern(node.Pattern, hints, ctx);

            cw.Br(skip);
            cw.MarkLabel(bad);
            cw.Pop();
            cw.Push(false);
            cw.MarkLabel(skip);
            cw.Nop();
        }

        private void PreinitPattern(DPattern node, Hints hints)
        {
            switch (node.NodeType)
            {
                case NodeType.NamePattern:
                    PreinitName((DNamePattern)node, hints);
                    break;
                case NodeType.IntegerPattern:
                    break;
                case NodeType.StringPattern:
                    break;
                case NodeType.FloatPattern:
                    break;
                case NodeType.CharPattern:
                    break;
                case NodeType.BooleanPattern:
                    break;
                case NodeType.TuplePattern:
                    PreinitSequence(((DTuplePattern)node).Elements, hints);
                    break;
                case NodeType.ArrayPattern:
                    PreinitSequence(((DArrayPattern)node).Elements, hints);
                    break;
                case NodeType.NilPattern:
                    break;
                case NodeType.RangePattern:
                    break;
                case NodeType.WildcardPattern:
                    break;
                case NodeType.AsPattern:
                    PreinitAs((DAsPattern)node, hints);
                    break;
                case NodeType.TypeTestPattern:
                    break;
                case NodeType.AndPattern:
                    PreinitAnd((DAndPattern)node, hints);
                    break;
                case NodeType.OrPattern:
                    PreinitOr((DOrPattern)node, hints);
                    break;
                case NodeType.MethodCheckPattern:
                    break;
                case NodeType.CtorPattern:
                    PreinitCtor((DCtorPattern)node, hints);
                    break;
                case NodeType.LabelPattern:
                    PreinitLabel((DLabelPattern)node, hints);
                    break;
            }
        }

        private void PreinitCtor(DCtorPattern node, Hints hints)
        {
            if (node.Arguments != null && node.Arguments.Count > 0)
                PreinitSequence(node.Arguments, hints);
        }

        private void PreinitAs(DAsPattern node, Hints hints)
        {
            PreinitPattern(node.Pattern, hints);
            var sva = -1;

            if (!TryGetLocalVariable(node.Name, out var sv))
                sva = AddVariable(node.Name, node, VarFlags.None);
            else
                sva = sv.Address;

            cw.PushNil();
            cw.PopVar(sva);
        }

        private void PreinitName(DNamePattern node, Hints hints)
        {
            var err = GetTypeHandle(null, node.Name, out var handle, out var std);

            if (err != CompilerError.None)
            {
                var found = TryGetLocalVariable(node.Name, out var sv);
                var sva = -1;

                if (!found)
                    sva = AddVariable(node.Name, node, hints.Has(Const) ? VarFlags.Const : VarFlags.None);
                else
                    sva = sv.Address;

                cw.PushNil();
                cw.PopVar(sva);
            }
        }

        private void PreinitAnd(DAndPattern node, Hints hints)
        {
            PreinitPattern(node.Left, hints);
            PreinitPattern(node.Right, hints);
        }

        private void PreinitOr(DOrPattern node, Hints hints)
        {
            PreinitPattern(node.Left, hints);
            PreinitPattern(node.Right, hints);
        }

        private void PreinitSequence(List<DNode> elements, Hints hints)
        {
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];

                if (e.NodeType == NodeType.LabelPattern)
                    PreinitLabel((DLabelPattern)e, hints);
                else
                    PreinitPattern((DPattern)e, hints);
            }
        }

        private void PreinitLabel(DLabelPattern node, Hints hints)
        {
            PreinitPattern(node.Pattern, hints);
        }

        private void ValidateMatch(DMatch match)
        {
            var irr = false;
            var count = match.Expression != null ? match.Expression.GetElementCount() : -1;

            foreach (var e in match.Entries)
            {
                var patternCount = e.Pattern.GetElementCount();
                var pt = e.Pattern.NodeType;

                if (irr)
                    AddWarning(CompilerWarning.UnreachableMatchEntry, e.Location, e.Pattern);
                else
                    CheckPattern(e.Pattern, count, patternCount);

                irr = IsIrrefutable(e.Pattern) && e.Guard == null;
            }
        }

        private void CheckPattern(DPattern e, int matchCount, int patternCount)
        {
            int c;
            if (matchCount > -1 && (c = e.GetElementCount()) > -1)
            {
                if (e.NodeType == NodeType.TuplePattern && matchCount != c)
                    AddWarning(CompilerWarning.PatternNeverMatch, e.Location, e);
                if (e.NodeType == NodeType.ArrayPattern && matchCount < c)
                    AddWarning(CompilerWarning.PatternNeverMatch, e.Location, e);
            }
        }

        private bool IsIrrefutable(DPattern node)
        {
            return node.NodeType == NodeType.NamePattern
                || node.NodeType == NodeType.WildcardPattern
                || node is DAsPattern pas && IsIrrefutable(pas.Pattern)
                || node is DAndPattern dand && IsIrrefutable(dand.Left) && IsIrrefutable(dand.Right)
                || node is DOrPattern dor && IsIrrefutable(dor.Left) && IsIrrefutable(dor.Right);
        }

        private bool IsPureBinding(DPattern node)
        {
            foreach (var n in node.ListElements())
                if (n.NodeType != NodeType.NamePattern && n.NodeType != NodeType.WildcardPattern)
                    return false;
            return true;
        }
    }
}
