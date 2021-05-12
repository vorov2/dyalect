using Dyalect.Parser;
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
            StartScope(ScopeKind.Lexical, node.Location);

            ctx = new(ctx)
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
            StartScope(ScopeKind.Lexical, node.Location);
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

        //Main compilation switch with all patterns
        private void BuildPattern(DPattern node, Hints hints, CompilerContext ctx)
        {
            switch (node.NodeType)
            {
                case NodeType.NotPattern:
                    var pt = ((DNotPattern)node).Pattern;
                    PreinitPattern(pt, hints);
                    BuildPattern(pt, hints, ctx);
                    cw.Not();
                    break;
                case NodeType.NamePattern:
                    BuildName((DNamePattern)node, hints);
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
                    BuildRange((DRangePattern)node);
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
                    BuildMethodCheck((DMethodCheckPattern)node);
                    break;
                case NodeType.CtorPattern:
                    BuildCtor((DCtorPattern)node, hints, ctx);
                    break;
                case NodeType.LabelPattern:
                    BuildLabel((DLabelPattern)node, hints, ctx);
                    break;
                case NodeType.ComparisonPattern:
                    BuildComparisonPattern((DComparisonPattern)node, hints, ctx);
                    break;
            }
        }

        private void BuildComparisonPattern(DComparisonPattern node, Hints hints, CompilerContext ctx)
        {
            PreinitPattern(node.Pattern, hints);
            BuildPattern(node.Pattern, hints, ctx);
            AddLinePragma(node);

            switch (node.Operator)
            {
                case BinaryOperator.Gt:
                    cw.Gt();
                    break;
                case BinaryOperator.Lt:
                    cw.Lt();
                    break;
                case BinaryOperator.GtEq:
                    cw.GtEq();
                    break;
                case BinaryOperator.LtEq:
                    cw.LtEq();
                    break;
            }
        }

        private void BuildCtor(DCtorPattern node, Hints hints, CompilerContext ctx)
        {
            var bad = cw.DefineLabel();
            var ok = cw.DefineLabel();

            if (node.TypeName is not null)
            {
                cw.Dup();
                cw.TypeCheck(GetTypeHandle(node.TypeName, node.Location));
                cw.Brfalse(bad);
            }

            cw.Dup();
            cw.CtorCheck(node.Constructor);
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

        private void BuildMethodCheck(DMethodCheckPattern node)
        {
            AddLinePragma(node);
            cw.HasMember(node.Name);
        }

        private void BuildAs(DAsPattern node, Hints hints, CompilerContext ctx)
        {
            cw.Dup();

            BuildPattern(node.Pattern, hints, ctx);
            var bad = cw.DefineLabel();
            var ok = cw.DefineLabel();
            cw.Brfalse(bad);
            int sva;

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

        private void BuildName(DNamePattern node, Hints hints)
        {
            ScopeVar sv = default;
            var found = hints.Has(Rebind)
                ? TryGetVariable(node.Name, out sv)
                : !hints.Has(OpenMatch) && TryGetLocalVariable(node.Name, out sv);
            var sva = sv.Address;

            if (!found)
                sva = AddVariable(node.Name, node, hints.Has(Const) ? VarFlags.Const : VarFlags.None);
            else if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                AddError(CompilerError.UnableAssignConstant, node.Location, node.Name);

            cw.PopVar(sva);
            cw.Push(true);
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
            PreinitOr(node, hints);

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

        private void BuildRange(DRangePattern node)
        {
            var skip = cw.DefineLabel();
            var exit = cw.DefineLabel();

            cw.Dup(); //2 objs
            cw.HasMember(Builtins.Lt);
            cw.Brfalse(skip); //1 left

            cw.Dup(); //2 objs
            cw.HasMember(Builtins.Gt);
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

        private void BuildSequence(DPattern node, List<DNode> elements, Hints hints, CompilerContext ctx)
        {
            var onlyLabels = true;

            for (var i = 0; i < elements.Count; i++)
                if (elements[i].NodeType != NodeType.LabelPattern)
                {
                    onlyLabels = false;
                    break;
                }

            var skip = cw.DefineLabel();
            var ok = cw.DefineLabel();

            if (!onlyLabels)
            {
                cw.Dup(); //2 objs
                cw.HasMember(Builtins.Len);
                cw.Brfalse(skip); //1 obj left to pop
            }

            cw.Dup(); //2 objs
            cw.HasMember(Builtins.Get);
            cw.Brfalse(skip); //1 obj left to pop

            if (!onlyLabels)
            {
                cw.Dup(); //2 objs
                cw.Len();
                cw.Push(elements.Count);
                if (node.NodeType == NodeType.TuplePattern || node.NodeType == NodeType.CtorPattern) cw.Eq(); else cw.GtEq();
                cw.Brfalse(skip); //1 obj left to pop
            }

            for (var i = 0; i < elements.Count; i++)
            {
                cw.Dup(); //2 objs
                var e = elements[i];

                if (e.NodeType == NodeType.LabelPattern)
                    BuildLabel((DLabelPattern)e, hints, ctx);
                else
                {
                    cw.Push(i);
                    cw.Get();
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

        //This is done to initialize all variables in patterns (if needed)
        //before executing the patterns
        private void PreinitPattern(DPattern node, Hints hints)
        {
            switch (node.NodeType)
            {
                case NodeType.TypeTestPattern:
                case NodeType.NilPattern:
                case NodeType.RangePattern:
                case NodeType.WildcardPattern:
                case NodeType.IntegerPattern:
                case NodeType.StringPattern:
                case NodeType.FloatPattern:
                case NodeType.CharPattern:
                case NodeType.BooleanPattern:
                case NodeType.MethodCheckPattern:
                    break;
                case NodeType.NamePattern:
                    PreinitName((DNamePattern)node, hints);
                    break;
                case NodeType.AsPattern:
                    PreinitAs((DAsPattern)node, hints);
                    break;
                case NodeType.TuplePattern:
                    PreinitSequence(((DTuplePattern)node).Elements, hints);
                    break;
                case NodeType.ArrayPattern:
                    PreinitSequence(((DArrayPattern)node).Elements, hints);
                    break;
                case NodeType.AndPattern:
                    PreinitAnd((DAndPattern)node, hints);
                    break;
                case NodeType.OrPattern:
                    PreinitOr((DOrPattern)node, hints);
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
            int sva;

            if (!TryGetLocalVariable(node.Name, out var sv))
                sva = AddVariable(node.Name, node, VarFlags.None);
            else
                sva = sv.Address;

            cw.PushNil();
            cw.PopVar(sva);
        }

        private void PreinitName(DNamePattern node, Hints hints)
        {
            var err = GetTypeHandle(null, node.Name, out _, out _);

            if (err != CompilerError.None)
            {
                var found = TryGetLocalVariable(node.Name, out var sv);
                int sva;

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

        private void PreinitLabel(DLabelPattern node, Hints hints) =>
            PreinitPattern(node.Pattern, hints);

        private void ValidateMatch(DMatch match)
        {
            var count = match.Expression is not null ? match.Expression.GetElementCount() : -1;

            for (var i = 0; i < match.Entries.Count; i++)
            {
                var e = match.Entries[i];

                if (e.Guard is null && e.Pattern is DNamePattern name)
                    name.IsConstructor = IsTypeExists(name.Name);

                if (e.Guard is null)
                {
                    var j = i;

                    while (j > 0)
                    {
                        j--;
                        var prev = match.Entries[j];

                        if (prev.Guard is null && !CanFollow(e.Pattern, prev.Pattern))
                        {
                            AddWarning(CompilerWarning.UnreachableMatchEntry, e.Location, e.Pattern, prev.Pattern);
                            break;
                        }
                    }
                }

                CheckPattern(e.Pattern, count);
            }
        }

        private void CheckPattern(DPattern e, int matchCount)
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

        private bool IsIrrefutable(DPattern node) =>
            node.NodeType == NodeType.NamePattern
                || node.NodeType == NodeType.WildcardPattern
                || node is DAsPattern pas && IsIrrefutable(pas.Pattern)
                || node is DAndPattern dand && IsIrrefutable(dand.Left) && IsIrrefutable(dand.Right)
                || node is DOrPattern dor && IsIrrefutable(dor.Left) && IsIrrefutable(dor.Right);

        private bool IsPureBinding(DPattern node)
        {
            foreach (var n in node.ListElements()!)
                if (n.NodeType != NodeType.NamePattern && n.NodeType != NodeType.WildcardPattern)
                    return false;
            return true;
        }

        private bool CanFollow(List<DNode> now, List<DNode> prev)
        {
            var len = prev.Count < now.Count ? prev.Count : now.Count;

            for (var i = 0; i < len; i++)
            {
                if (CanFollow((DPattern)now[i], (DPattern)prev[i]))
                    return true;
            }

            return false;
        }

        private bool CanFollow(DStringPattern now, DSequencePattern prev)
        {
            for (var i = 0; i < prev.Elements.Count; i++)
            {
                var t = prev.Elements[i];

                if (t.NodeType != NodeType.CharPattern || ((DCharPattern)t).Value != now.Value.Value[i])
                    return true;
            }

            return false;
        }

        //Simple match consistency check
        private bool CanFollow(DPattern now, DPattern prev)
        {
            switch (prev.NodeType)
            {
                case NodeType.NamePattern:
                    {
                        var nm = (DNamePattern)prev;
                        if (!nm.IsConstructor)
                            return false;
                        if (now.NodeType == NodeType.NamePattern)
                        {
                            var nmn = (DNamePattern)now;
                            return !nmn.IsConstructor || nmn.Name != nm.Name;
                        }
                        return true;
                    }
                case NodeType.WildcardPattern: return false;
                case NodeType.AsPattern:
                    return CanFollow(now, ((DAsPattern)prev).Pattern);
                case NodeType.TuplePattern:
                case NodeType.ArrayPattern:
                    {
                        var prevSeq = (DSequencePattern)prev;

                        if (now.NodeType == NodeType.TuplePattern || now.NodeType == NodeType.ArrayPattern)
                        {
                            var nowTuple = (DSequencePattern)now;

                            if (nowTuple.Elements.Count != prevSeq.Elements.Count)
                                return true;

                            return CanFollow(nowTuple.Elements, prevSeq.Elements);
                        }
                        else if (now.NodeType == NodeType.StringPattern)
                        {
                            var str = (DStringPattern)now;

                            if ((prevSeq.NodeType == NodeType.TuplePattern && prevSeq.Elements.Count != str.Value.Value.Length)
                                || (prevSeq.NodeType == NodeType.ArrayPattern && prevSeq.Elements.Count > str.Value.Value.Length))
                                return true;

                            return CanFollow(str, prevSeq);
                        }
                        else
                            return true;
                    }
                case NodeType.LabelPattern:
                    {
                        if (now.NodeType != NodeType.LabelPattern)
                            return true;
                        return ((DLabelPattern)prev).Label != ((DLabelPattern)now).Label
                            || CanFollow(((DLabelPattern)now).Pattern, ((DLabelPattern)prev).Pattern);
                    }
                case NodeType.OrPattern:
                    {
                        if (now.NodeType != NodeType.OrPattern)
                            return true;
                        var andNow = (DOrPattern)now;
                        var andPrev = (DOrPattern)prev;
                        return CanFollow(andNow.Left, andPrev.Left) && CanFollow(andNow.Right, andPrev.Right);
                    }
                case NodeType.AndPattern:
                    {
                        if (now.NodeType != NodeType.AndPattern)
                            return true;
                        var andNow = (DAndPattern)now;
                        var andPrev = (DAndPattern)prev;
                        return CanFollow(andNow.Left, andPrev.Left) && CanFollow(andNow.Right, andPrev.Right);
                    }
                case NodeType.RangePattern:
                    {
                        if (now.NodeType != NodeType.RangePattern)
                            return true;
                        var rngNow = (DRangePattern)now;
                        var rngPrev = (DRangePattern)prev;
                        return !rngNow.From.Equals(rngPrev.From) || !rngNow.To.Equals(rngPrev.To);
                    }
                case NodeType.NilPattern:
                    return now.NodeType != NodeType.NilPattern;
                case NodeType.StringPattern:
                    {
                        var prevStr = (DStringPattern)prev;
                        if (now.NodeType == NodeType.StringPattern)
                        {
                            var str = (DStringPattern)now;
                            return str.Value.Value != prevStr.Value.Value;
                        }
                        else if (now.NodeType == NodeType.TuplePattern)
                        {
                            var tup = (DTuplePattern)now;
                            if (tup.Elements.Count != prevStr.Value.Value.Length)
                                return true;
                            return CanFollow(prevStr, tup);
                        }
                        else if (now.NodeType == NodeType.ArrayPattern)
                        {
                            var arr = (DArrayPattern)now;
                            if (arr.Elements.Count > prevStr.Value.Value.Length)
                                return true;
                            return CanFollow(prevStr, arr);
                        }
                        else
                            return true;
                    }
                case NodeType.IntegerPattern:
                    return now.NodeType != NodeType.IntegerPattern
                        || ((DIntegerPattern)now).Value != ((DIntegerPattern)prev).Value;
                case NodeType.FloatPattern:
                    return now.NodeType != NodeType.FloatPattern
                        || ((DFloatPattern)now).Value != ((DFloatPattern)prev).Value;
                case NodeType.CharPattern:
                    return now.NodeType != NodeType.CharPattern
                        || ((DCharPattern)now).Value != ((DCharPattern)prev).Value; ;
                case NodeType.BooleanPattern:
                    return now.NodeType != NodeType.BooleanPattern
                        || ((DBooleanPattern)now).Value != ((DBooleanPattern)prev).Value; ;
                case NodeType.TypeTestPattern:
                    {
                        if (now.NodeType != NodeType.TypeTestPattern)
                            return true;
                        return !((DTypeTestPattern)prev).TypeName.IsPossibleEquality(((DTypeTestPattern)now).TypeName);
                    }
                case NodeType.MethodCheckPattern:
                    {
                        if (now.NodeType != NodeType.MethodCheckPattern)
                            return true;
                        return ((DMethodCheckPattern)prev).Name != ((DMethodCheckPattern)now).Name;
                    }
                case NodeType.CtorPattern:
                    {
                        if (now.NodeType != NodeType.CtorPattern)
                            return true;

                        var prevc = (DCtorPattern)prev;
                        var nowc = (DCtorPattern)now;

                        if (prevc.Constructor != nowc.Constructor)
                            return true;

                        if (prevc.Arguments == null || prevc.Arguments.Count == 0)
                            return false;

                        if (prevc.Arguments.Count != nowc.Arguments.Count)
                            return true;

                        for (var i = 0; i < prevc.Arguments.Count; i++)
                        {
                            if (CanFollow((DPattern)nowc.Arguments[i], (DPattern)prevc.Arguments[i]))
                                return true;
                        }

                        return false;
                    }
                default: throw Ice();
            }
        }
    }
}
