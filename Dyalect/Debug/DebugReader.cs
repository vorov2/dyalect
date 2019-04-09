using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DebugReader
    {
        public DebugReader(DebugInfo symbols)
        {
            Symbols = symbols;
        }
        
        public FunSym GetFunSymByHandle(int handle)
        {
            if (Symbols == null)
                return null;

            for (var i = 0; i < Symbols.Functions.Count; i++)
                if (Symbols.Functions[i].Handle == handle)
                    return Symbols.Functions[i];

            return null;
        }

        public FunSym FindFunSym(int offset)
        {
            if (Symbols == null)
                return null;

            var fun = default(FunSym);

            for (var i = 0; i < Symbols.Functions.Count; i++)
            {
                var f = Symbols.Functions[i];

                if (offset > f.StartOffset && offset < f.EndOffset)
                    fun = f;
            }

            return fun;
        }
        
        public LineSym FindLineSym(int offset)
        {
            if (Symbols == null)
                return null;

            for (var i = 0; i < Symbols.Lines.Count; i++)
            {
                var l = Symbols.Lines[i];

                if (l.Offset == offset)
                    return l;
            }

            return offset == 0 ? null : FindLineSym(offset - 1);
        }
        
        public LineSym FindClosestLineSym(int line, int column)
        {
            if (Symbols == null)
                return null;

            var ln = default(LineSym);
            var minDiffCol = Int32.MaxValue;
            var minDiffLine = Int32.MaxValue;

            for (var i = 0; i < Symbols.Lines.Count; i++)
            {
                var l = Symbols.Lines[i];

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

                for (var i = 0; i < Symbols.Lines.Count; i++)
                {
                    var l = Symbols.Lines[i];

                    if (l.Line == ln.Line && l.Column == ln.Column && l.Offset > ln.Offset)
                    {
                        ln = l;
                        maxOff = ln.Offset;
                    }
                }
            }

            return ln;
        }
        
        public ScopeSym GetScopeSymByIndex(int scopeIndex)
        {
            for (var i = 0; i < Symbols.Scopes.Count; i++)
                if (Symbols.Scopes[i].Index == scopeIndex)
                    return Symbols.Scopes[i];

            return null;
        }
        
        public ScopeSym FindScopeSym(int offset)
        {
            if (Symbols == null)
                return null;

            var scope = default(ScopeSym);

            for (var i = 0; i < Symbols.Scopes.Count; i++)
            {
                var s = Symbols.Scopes[i];

                if (offset >= s.StartOffset && offset <= s.EndOffset)
                    scope = s;
            }

            return scope;
        }
        
        public ScopeSym FindScopeSym(int line, int column)
        {
            if (Symbols == null)
                return null;

            for (var i = 0; i < Symbols.Scopes.Count; i++)
            {
                var s = Symbols.Scopes[i];

                if ((line == s.StartLine && column >= s.StartColumn || line > s.StartLine)
                     && line <= s.EndLine
                    )
                    return s;
            }

            return null;
        }
        
        public VarSym FindVarSym(int address, int scopeIndex)
        {
            if (Symbols == null)
                return null;

            for (var i = 0; i < Symbols.Vars.Count; i++)
            {
                var v = Symbols.Vars[i];

                if (v.Address == address && v.Scope >= scopeIndex)
                    return v;
            }

            return default(VarSym);
        }
        
        public IEnumerable<VarSym> FindVarSyms(int offset, ScopeSym scope)
        {
            if (Symbols == null)
                yield break;

            for (var i = 0; i < Symbols.Vars.Count; i++)
            {
                var v = Symbols.Vars[i];

                if ((scope == null && v.Scope == 0 || v.Scope == scope.Index) &&
                    v.Offset <= offset)
                    yield return v;
            }
        }

        public IEnumerable<VarSym> EnumerateVarSyms()
        {
            return Symbols.Vars.ToArray();
        }

        public DebugInfo Symbols { get; }
    }
}
