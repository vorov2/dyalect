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

        private CompilerError VariableExists(CompilerContext ctx, string name, bool checkType = true)
        {
            var err = GetVariable(name, currentScope, out _);

            if (err is CompilerError.None)
                return err;

            if (checkType && GetTypeHandle(null, name, out _, out _) is CompilerError.None)
                return CompilerError.None;

            if (err is CompilerError.UndefinedVariable
                && ctx.LocalType is not null
                && typeScopes.TryGetValue(ctx.LocalType, out var tscope)
                && tscope.Locals.TryGetValue(name, out _))
                return CompilerError.None;

            return err;
        }

        private void PushVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetVariable(name, currentScope, out var sv);

            if (err is not CompilerError.None)
            {
                if (string.IsNullOrEmpty(name))
                    return;

                var th = GetTypeHandle(null, name, out var hdl, out var std);

                if (th is CompilerError.None)
                {
                    AddLinePragma(loc);
                    cw.Type(new(hdl, std));
                    return;
                }

                AddError(err, loc, name);
                return;
            }

            if ((sv.Data & VarFlags.InitBlock) == VarFlags.InitBlock)
            {
                GetVariable(ctx.Function!.IsStatic ? "$this" : "this", currentScope, out var thisVar);
                AddLinePragma(loc);
                cw.PushVar(thisVar);
                cw.Priv();
                cw.Push(name);
                cw.Get();
                return;
            }

            AddLinePragma(loc);
            cw.PushVar(sv);
        }

        private void PopVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetVariable(name, currentScope, out var sv);

            if (err is not CompilerError.None)
            {
                if (string.IsNullOrEmpty(name))
                    return;

                AddError(err, loc, name);
                return;
            }

            if ((sv.Data & VarFlags.InitBlock) == VarFlags.InitBlock)
            {
                GetVariable(ctx.Function!.IsStatic ? "$this" : "this", currentScope, out var thisVar);
                AddLinePragma(loc);
                cw.PushVar(thisVar);
                cw.Priv();
                cw.Push(name);
                cw.Set();
                return;
            }

            AddLinePragma(loc);
            cw.PopVar(sv.Address);
        }


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
        private void CallAutos(bool cls = false)
        {
            PeekAutos(currentScope);
            if (cls)
                currentScope.Autos.Clear();
        }

        private void CallAutosForKind(ScopeKind kind)
        {
            var scope = currentScope;
            var last = false;
            var shift = 0;

            while (true)
            {
                PeekAutos(scope, shift);

                if (last)
                    break;

                scope = scope.Parent;

                if (scope!.Kind == kind)
                    last = true;

                if (scope.Kind == ScopeKind.Function)
                    shift++;
            }
        }

        private void PeekAutos(Scope scope, int shift = 0)
        {
            foreach (var a in scope.Autos)
            {
                var sv = new ScopeVar(shift | a.Item1 << 8);
                var escape = cw.DefineLabel();
                cw.PushVar(sv);
                cw.IsNull();
                cw.Brtrue(escape);
                cw.PushVar(sv);
                cw.GetMember(Builtins.Dispose);
                cw.FunPrep(0);
                cw.FunCall(0);
                cw.Pop();
                cw.MarkLabel(escape);
                cw.Nop();
            }
        }

        //Add a regular named variable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int AddVariable(string name, Location loc, int data)
        {
            //Check if we already have such a variable in the local scope
            if (currentScope.Locals.TryGetValue(name, out var exist))
            {
                if ((exist.Data & VarFlags.PreInit) == VarFlags.PreInit && (data & VarFlags.Function) == VarFlags.Function)
                {
                    currentScope.Locals[name] = new ScopeVar(exist.Address, exist.Data ^ VarFlags.PreInit);
                    return (0 | exist.Address << 8);
                }
                else
                {
                    AddError(CompilerError.VariableAlreadyDeclared, loc, name);
                    return -1;
                }
            }

            currentScope.Locals.Add(name, new(currentCounter, data));

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
                unit.ExportList.Add(name, new(retval, data));
            }

            return retval;
        }

        //Find a variable in a global scope
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
                return new(1 | var.Address << 8, var.Data);

            AddError(CompilerError.UndefinedBaseVariable, loc, name);
            return ScopeVar.Empty;
        }

        private bool TryGetLocalVariable(string name, out ScopeVar var)
        {
            var = default;

            if (currentScope.Locals.TryGetValue(name, out var sv))
            {
                var = new(0 | sv.Address << 8, sv.Data);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CompilerError GetVariable(string name, Scope startScope, out ScopeVar var)
        {
            var cur = startScope;
            var shift = 0;

            //Search all upper scopes recursively
            do
            {
                if (cur.Locals.TryGetValue(name, out var sv))
                {
                    var = new(shift | sv.Address << 8, sv.Data);
                    return CompilerError.None;
                }

                if (cur.Kind == ScopeKind.Function)
                    shift++;

                cur = cur.Parent;
            }
            while (cur != null);

            //No luck. Need to check if this variable is imported from some module
            if (TryGetImport(name, out var sv1, out var moduleHandle))
            {
                if ((sv1.Data & VarFlags.Private) == VarFlags.Private)
                {
                    var = ScopeVar.Empty;
                    return CompilerError.PrivateNameAccess;
                }

                var = new(moduleHandle | (sv1.Address >> 8) << 8, sv1.Data | VarFlags.External);
                return CompilerError.None;
            }

            var = ScopeVar.Empty;
            return CompilerError.UndefinedVariable;
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
