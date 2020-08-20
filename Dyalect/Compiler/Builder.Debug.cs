using Dyalect.Debug;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dyalect.Compiler
{
    //This part contains everything connected with emitting debug info
    partial class Builder
    {
        //last location passed to AddLineSym.
        //Used by method EndScope to emit location of scope end.
        private Location lastLocation;

        //Corrections used for compilation of code islands inside strings
        private Stack<Location> corrections = new Stack<Location>();

        private DebugWriter pdb; //Symbol writer
        private bool isDebug; //Determines if we need to generate extended debug info

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Line(Location loc)
        {
            if (corrections.Count > 0)
                return corrections.Peek().Line;
            else
                return loc.Line;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Col(Location loc)
        {
            if (corrections.Count > 0)
                return corrections.Peek().Column + loc.Column;
            else
                return loc.Column;
        }

        //Call this to generate the first part of FunSym
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartFun(string name, Par[] pars, int parCount)
        {
            cw.StartFrame();
            pdb.StartFunction(name, cw.Offset, pars);
        }

        //Call this to finalized generation of FunSym
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int EndFun(int handle)
        {
            pdb.EndFunction(handle, cw.Offset);
            return cw.FinishFrame();
        }

        //Generates start of any scope
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartScope(bool fun, Location loc)
        {
            currentScope = new Scope(fun, currentScope);

            if (isDebug)
                pdb.StartScope(cw.Offset, Line(loc), Col(loc));
        }

        //Called when any lexical scope ends
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndScope()
        {
            currentScope = currentScope.Parent != null ? currentScope.Parent : null;

            if (isDebug)
                pdb.EndScope(cw.Offset, Line(lastLocation), Col(lastLocation));
        }

        //Called after StartScope when lexcial code exists in runtime (such as lexical
        //scope of a function).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartSection()
        {
            counters.Push(currentCounter);
            currentCounter = 0;
        }

        //Called when actual (runtime) lexical scope ends
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndSection()
        {
            currentCounter = counters.Pop();
        }

        //Generate line pragma for a string and remembers last position
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddLinePragma(DNode node)
        {
            lastLocation = node.Location;
            pdb.AddLineSym(cw.Offset, Line(lastLocation), Col(lastLocation));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddLinePragma(Location loc)
        {
            lastLocation = loc;
            pdb.AddLineSym(cw.Offset, Line(loc), Col(loc));
        }

        //Used only when extended debug info is generated
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddVarPragma(string name, int address, int offset, int data)
        {
            if (isDebug)
                pdb.AddVarSym(name, address, offset, 0, data);
        }
    }
}
