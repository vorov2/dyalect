using Dyalect.Debug;
using Dyalect.Parser;
using Dyalect.Parser.Model;

namespace Dyalect.Compiler
{
    //This part contains everything connected with emitting debug info
    partial class Builder
    {
        //last location passed to AddLineSym.
        //Used by method EndScope to emit location of scope end.
        private Location lastLocation;

        private DebugWriter pdb; //Symbol writer
        private bool isDebug; //Determines if we need to generate extended debug info

        //Call this to generate the first part of FunSym
        private void StartFun(string name, Par[] pars, int parCount)
        {
            cw.StartFrame();
            pdb.StartFunction(name, cw.Offset, pars);
        }

        //Call this to finalized generation of FunSym
        private int EndFun(int handle)
        {
            pdb.EndFunction(handle, cw.Offset);
            return cw.FinishFrame();
        }

        //Generates start of any scope
        private void StartScope(bool fun, Location loc)
        {
            currentScope = new Scope(fun, currentScope);

            if (isDebug)
                pdb.StartScope(cw.Offset, loc.Line, loc.Column);
        }

        //Called when any lexical scope ends
        private void EndScope()
        {
            currentScope = currentScope.Parent != null ? currentScope.Parent : null;

            if (isDebug)
                pdb.EndScope(cw.Offset, lastLocation.Line, lastLocation.Column);
        }

        //Called after StartScope when lexcial code exists in runtime (such as lexical
        //scope of a function).
        private void StartSection()
        {
            counters.Push(currentCounter);
            currentCounter = 0;
        }

        //Called when actual (runtime) lexical scope ends
        private void EndSection()
        {
            currentCounter = counters.Pop();
        }

        //Generate line pragma for a string and remembers last position
        private void AddLinePragma(DNode node)
        {
            lastLocation = node.Location;
            pdb.AddLineSym(cw.Offset, lastLocation.Line, lastLocation.Column);
        }

        private void AddLinePragma(Location loc)
        {
            lastLocation = loc;
            pdb.AddLineSym(cw.Offset, loc.Line, loc.Column);
        }

        //Used only when extended debug info is generated
        private void AddVarPragma(string name, int address, int offset, int data)
        {
            if (isDebug)
                pdb.AddVarSym(name, address, offset, 0, data);
        }
    }
}
