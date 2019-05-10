using System;
using System.Collections.Generic;

namespace Dyalect.Debug
{
    public sealed class DebugWriter
    {
        private Stack<ScopeSym> scopes;
        private Stack<FunSym> funs;
        private int scopeCount;
        private readonly static FunctionParameter[] emptyPars = new FunctionParameter[0];

        public DebugWriter()
        {
            Symbols = new DebugInfo();
            scopes = new Stack<ScopeSym>();
            funs = new Stack<FunSym>();
            var glob = new ScopeSym(0, 0, 0, 0, 0) { EndOffset = int.MaxValue };
            scopes.Push(glob);
            Symbols.Scopes.Add(glob);
        }

        private DebugWriter(DebugWriter dw)
        {
            Symbols = dw.Symbols.Clone();
            scopes = new Stack<ScopeSym>(dw.scopes.ToArray());
            funs = new Stack<FunSym>(dw.funs.ToArray());
        }

        public DebugWriter Clone()
        {
            return new DebugWriter(this);
        }

        public void StartFunction(string name, int offset, FunctionParameter[] pars = null)
        {
            funs.Push(new FunSym(name, offset, pars ?? emptyPars));
        }

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

        public void AddVarSym(string name, int address, int offset, int flags, int data)
        {
            Symbols.Vars.Add(LastVarSym = new VarSym(name, address, offset, scopes.Peek().Index, flags, data));
        }

        public void AddLineSym(int offset, int line, int col)
        {
            Symbols.Lines.Add(new LineSym(offset, line, col));
        }

        public DebugInfo Symbols { get; private set; }

        public VarSym LastVarSym { get; private set; }
    }
}
