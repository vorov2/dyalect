using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DebugWriter
    {
        private readonly Stack<ScopeSym> scopes;
        private readonly Stack<FunSym> funs;
        private int scopeCount;

        public DebugWriter()
        {
            Symbols = new();
            scopes = new();
            funs = new();
            var glob = new ScopeSym(0, 0, 0, 0, 0) { EndOffset = int.MaxValue };
            scopes.Push(glob);
            Symbols.Scopes.Add(glob);
        }

        private DebugWriter(DebugWriter dw)
        {
            Symbols = dw.Symbols.Clone();
            scopes = new(dw.scopes.ToArray());
            funs = new(dw.funs.ToArray());
        }

        public DebugWriter Clone() => new DebugWriter(this);

        public void StartFunction(string name, int offset, Par[] pars = null) =>
            funs.Push(new FunSym(name, offset, pars ?? Array.Empty<Par>()));
        
        public void EndFunction(int handle, int offset)
        {
            var f = funs.Pop();
            f.Handle = handle;
            f.EndOffset = offset;
            Symbols.Functions.Add(handle, f);
        }

        public void StartScope(int offset, int line, int col)
        {
            var index = ++scopeCount;
            scopes.Push(new ScopeSym(index, scopes.Peek().Index, offset, line, col));
        }

        public void EndScope(int offset, int line, int col)
        {
            var s = scopes.Pop();
            s.EndOffset = offset;
            s.EndLine = line;
            s.EndColumn = col;
            Symbols.Scopes.Add(s);
        }

        public void AddVarSym(string name, int address, int offset, int flags, int data) =>
            Symbols.Vars.Add(LastVarSym = new VarSym(name, address, offset, scopes.Peek().Index, flags, data));
        
        public void AddLineSym(int offset, int line, int col) =>
            Symbols.Lines.Add(new LineSym(offset, line, col));
        
        public DebugInfo Symbols { get; private set; }

        public VarSym LastVarSym { get; private set; }
    }
}
