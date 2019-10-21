using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public static class DebugReaderExtensions
    {
        public static FunSym FindFunSym(this DebugInfo syms, int offset)
        {
            foreach (var f in syms.Functions.Values)
                if (offset > f.StartOffset && offset < f.EndOffset)
                    return f;
            return null;
        }

        public static LineSym FindLineSym(this DebugInfo syms, int offset)
        {
            if (offset < 0)
                return null;

            for (var i = 0; i < syms.Lines.Count; i++)
            {
                var l = syms.Lines[i];

                if (l.Offset == offset)
                    return l;
            }

            return offset == 0 ? null : FindLineSym(syms, offset - 1);
        }

        public static LineSym FindClosestLineSym(this DebugInfo syms, int line, int column)
        {
            var ln = default(LineSym);
            var minDiffCol = int.MaxValue;
            var minDiffLine = int.MaxValue;

            for (var i = 0; i < syms.Lines.Count; i++)
            {
                var l = syms.Lines[i];

                if (l.Line == line && l.Column == column)
                {
                    ln = l;
                    break;
                }
                else if (Math.Abs(l.Line - line) < minDiffLine)
                {
                    minDiffLine = Math.Abs(l.Line - line);
                    minDiffCol = Math.Abs(l.Column - column);
                    ln = l;
                }
                else if (Math.Abs(l.Line - line) == minDiffLine && Math.Abs(l.Column - column) < minDiffCol)
                {
                    minDiffCol = Math.Abs(l.Column - column);
                    ln = l;
                }
            }

            if (ln != null)
            {
                var maxOff = ln.Offset;

                for (var i = 0; i < syms.Lines.Count; i++)
                {
                    var l = syms.Lines[i];

                    if (l.Line == ln.Line && l.Column == ln.Column && l.Offset > ln.Offset)
                    {
                        ln = l;
                        maxOff = ln.Offset;
                    }
                }
            }

            return ln;
        }

        public static ScopeSym GetScopeSymByIndex(this DebugInfo syms, int scopeIndex)
        {
            for (var i = 0; i < syms.Scopes.Count; i++)
                if (syms.Scopes[i].Index == scopeIndex)
                    return syms.Scopes[i];

            return null;
        }

        public static ScopeSym FindScopeSym(this DebugInfo syms, int offset)
        {
            var scope = default(ScopeSym);

            for (var i = 0; i < syms.Scopes.Count; i++)
            {
                var s = syms.Scopes[i];

                if (offset >= s.StartOffset && offset <= s.EndOffset)
                    scope = s;
            }

            return scope;
        }

        public static ScopeSym FindScopeSym(this DebugInfo syms, int line, int column)
        {
            for (var i = 0; i < syms.Scopes.Count; i++)
            {
                var s = syms.Scopes[i];

                if ((line == s.StartLine && column >= s.StartColumn || line > s.StartLine)
                     && line <= s.EndLine
                    )
                    return s;
            }

            return null;
        }

        public static VarSym FindVarSym(this DebugInfo syms, int address, int scopeIndex)
        {
            for (var i = 0; i < syms.Vars.Count; i++)
            {
                var v = syms.Vars[i];

                if (v.Address == address && v.Scope >= scopeIndex)
                    return v;
            }

            return default(VarSym);
        }

        public static IEnumerable<VarSym> FindVarSyms(this DebugInfo syms, int offset, ScopeSym scope)
        {
            for (var i = 0; i < syms.Vars.Count; i++)
            {
                var v = syms.Vars[i];

                if ((scope == null && v.Scope == 0 || v.Scope == scope.Index) &&
                    v.Offset <= offset)
                    yield return v;
            }
        }

        public static IEnumerable<VarSym> EnumerateVarSyms(this DebugInfo syms) => syms.Vars.ToArray();
    }
}
