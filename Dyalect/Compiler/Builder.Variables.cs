using Dyalect.Parser;
using Dyalect.Parser.Model;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Dyalect.Compiler
{
    //This part is responsible for adding/resolving variables
    partial class Builder
    {
        //Variables indexers
        private readonly Stack<int> counters; //Stack of indices for the lexical scope
        private int currentCounter; //Global indexer

        private bool privateScope; //Identifies that a current scope is private

        //Standard routine to add variables, can be used when an internal unnamed variable is need
        //which won't be visible to the user (for system purposes).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AddVariable()
        {
            var ret = 0 | currentCounter << 8;
            currentCounter++;
            return ret;
        }

        //Call close for all variables in this scope, registered as autos
        private void CallAutos()
        {
            System.Console.WriteLine($"***Here for LOCALS");
            PeekAutos(currentScope);
            currentScope.Autos.Clear();
        }

        bool hasAutos()
        {
            var s = currentScope;
            do
            {
                if (s.Autos.Count > 0)
                    return true;
                s = s.Parent;
            }
            while (s != globalScope);
            return false;
        }

        private void CallAutosForKind(ScopeKind kind)
        {
            if (!hasAutos()) return ;

            System.Console.WriteLine($"***Here for {kind}");
            var scope = currentScope;
            var last = false;
            var shift = 0;

            while (true)
            {
                PeekAutos(scope, shift);

                if (kind != ScopeKind.Loop)
                    scope.Autos.Clear();

                if (last)
                    break;

                scope = scope.Parent;

                if (scope.Kind == kind)
                    last = true;

                if (scope.Kind == ScopeKind.Function)
                    shift++;
            }
        }

        private void PeekAutos(Scope scope, int shift = 0)
        {
            foreach (var a in scope.Autos)
            {
                System.Console.WriteLine($"shift:{shift},address:{a}");
                var sv = new ScopeVar(shift | a.Item1 << 8);
                cw.PushVar(sv);
                cw.GetMember(GetMemberNameId("close"));
                cw.FunPrep(0);
                cw.FunCall(0);
                cw.Pop();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddVariable(string name, DNode node, int data) =>
            AddVariable(name, node != null ? node.Location : default, data);

        //Add a regular named variable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AddVariable(string name, Location loc, int data)
        {
            //Check if we already have such a variable in the local scope
            if (currentScope.Locals.ContainsKey(name))
            {
                AddError(CompilerError.VariableAlreadyDeclared, loc, name);
                return -1;
            }

            currentScope.Locals.Add(name, new ScopeVar(currentCounter, data));

            //An extended debug info is generated only in debug mode
            if (isDebug && !loc.IsEmpty)
            {
                AddVarPragma(name, currentCounter, cw.Offset, data);
                AddLinePragma(loc);
                cw.Nop();
            }

            var retval = AddVariable();

            if (currentScope == globalScope && (data & VarFlags.Const) == VarFlags.Const)
            {
                if (privateScope)
                    data |= VarFlags.Private;

                unit.ExportList.Remove(name);
                unit.ExportList.Add(name, new ScopeVar(retval, data));
            }

            return retval;
        }

        //Find a variable in a global scope
        private ScopeVar GetParentVariable(string name, DNode node) =>
            GetParentVariable(name, node.Location);

        private ScopeVar GetParentVariable(string name, Location loc)
        {
            var cur = currentScope;

            //Backtracks the scopes to find parent
            while (cur.Parent != null)
            {
                if (cur.Kind == ScopeKind.Function)
                {
                    cur = cur.Parent;
                    break;
                }

                cur = cur.Parent;
            }

            if (cur.Locals.TryGetValue(name, out var var))
                return new ScopeVar(1 | var.Address << 8, var.Data);

            AddError(CompilerError.UndefinedBaseVariable, loc, name);
            return ScopeVar.Empty;
        }

        //Search a vriable by its name, starting from current lexical scope
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ScopeVar GetVariable(string name, DNode node, bool err = true) =>
            GetVariable(name, currentScope, node.Location, err);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetVariableToAssign(string name, DNode node, bool err = true) =>
            GetVariableToAssign(name, node.Location, err);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ScopeVar GetVariable(string name, Location loc, bool err = true) =>
            GetVariable(name, currentScope, loc, err);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetVariableToAssign(string name, Location loc, bool err = true)
        {
            var sv = GetVariable(name, currentScope, loc, err);

            if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                AddError(CompilerError.UnableAssignConstant, loc, loc);

            return sv.Address;
        }

        private bool TryGetLocalVariable(string name, out ScopeVar var)
        {
            var = default;

            if (currentScope.Locals.TryGetValue(name, out var sv))
            {
                var = new ScopeVar(0 | sv.Address << 8, sv.Data);
                return true;
            }

            return false;
        }

        private bool TryGetVariable(string name, out ScopeVar var)
        {
            var = default;
            var sv = GetVariable(name, currentScope, default, err: false);

            if (!sv.IsEmpty())
            {
                var = sv;
                return true;
            }
            else
                return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ScopeVar GetVariable(string name, Scope startScope, Location loc, bool err)
        {
            var cur = startScope;
            var shift = 0;

            //Search all upper scopes recursively
            do
            {
                if (cur.Locals.TryGetValue(name, out var var))
                    return new ScopeVar(shift | var.Address << 8, var.Data);

                if (cur.Kind == ScopeKind.Function)
                    shift++;

                cur = cur.Parent;
            }
            while (cur != null);

            //No luck. Need to check if this variable is imported from some module
            if (TryGetImport(name, out var sv, out var moduleHandle))
            {
                if ((sv.Data & VarFlags.Private) == VarFlags.Private)
                    AddError(CompilerError.PrivateNameAccess, loc, name);

                return new ScopeVar(moduleHandle | (sv.Address >> 8) << 8, sv.Data | VarFlags.External);
            }

            if (err)
                AddError(CompilerError.UndefinedVariable, loc, name);

            return ScopeVar.Empty;
        }

        private bool TryGetImport(string name, out ScopeVar sv, out int moduleHandle)
        {
            foreach (var u in referencedUnits.Values)
            {
                if (u.Unit.ExportList.TryGetValue(name, out sv))
                {
                    moduleHandle = u.Handle;
                    return true;
                }
            }

            moduleHandle = default;
            sv = default;
            return false;
        }
    }
}
