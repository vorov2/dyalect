using Dyalect.Parser;
using Dyalect.Parser.Model;
using System.Collections.Generic;

namespace Dyalect.Compiler
{
    //This part is responsible for adding/resolving variables
    partial class Builder
    {
        //Variables indexers
        private Stack<int> counters; //Stack of indices for the lexical scope
        private int currentCounter; //Global indexer

        //Standard routine to add variables, can be used when an internal unnamed variable is need
        //which won't be visible to the user (for system purposes).
        private int AddVariable()
        {
            var ret = 0 | currentCounter << 8;
            currentCounter++;
            return ret;
        }

        private int AddSystemVariable(string name)
        {
            return AddVariable(name, null, -1);
        }

        public int AddVariable(string name, DNode node, int data) =>
            AddVariable(name, node != null ? node.Location : default, data);

        //Add a regular named variable
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

            if (currentScope == globalScope && (data & VarFlags.Function) == VarFlags.Function)
                unit.ExportList.Add(name, new ScopeVar(retval, data));

            return retval;
        }

        //Find a variable in a global scope
        private ScopeVar GetParentVariable(string name, DNode node)
        {
            return GetParentVariable(name, node.Location);
        }

        private ScopeVar GetParentVariable(string name, Location loc)
        {
            var cur = currentScope;

            //Backtracks the scopes to find parent
            while (cur.Parent != null)
            {
                if (cur.Function)
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
        private ScopeVar GetVariable(string name, DNode node, bool err = true)
        {
            return GetVariable(name, currentScope, node.Location, err);
        }

        private int GetVariableToAssign(string name, DNode node, bool err = true)
        {
            return GetVariableToAssign(name, node.Location, err);
        }

        private ScopeVar GetVariable(string name, Location loc, bool err = true)
        {
            return GetVariable(name, currentScope, loc, err);
        }

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

        private ScopeVar GetVariable(string name, Scope startScope, Location loc, bool err)
        {
            var cur = startScope;
            var shift = 0;
            var var = ScopeVar.Empty;

            //Search all upper scopes recursively
            do
            {
                if (cur.Locals.TryGetValue(name, out var))
                    return new ScopeVar(shift | var.Address << 8, var.Data);

                if (cur.Function)
                    shift++;

                var = ScopeVar.Empty;
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

        //Look for the scope with such a variable, return the firt that match
        private Scope GetScope(string name)
        {
            var cur = currentScope;

            do
            {
                if (cur.Locals.ContainsKey(name))
                    return cur;

                cur = cur.Parent;
            }
            while (cur != null);

            return null;
        }
    }
}
