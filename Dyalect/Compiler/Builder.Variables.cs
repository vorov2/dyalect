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

        private void CrawlVariables(DNode? node, HashSet<string> vars)
        {
            if (node is null)
                return;

            switch (node.NodeType)
            {
                case NodeType.Binary:
                    CrawlVariables(((DBinaryOperation)node).Left, vars);
                    CrawlVariables(((DBinaryOperation)node).Right, vars);
                    break;
                case NodeType.If:
                    CrawlVariables(((DIf)node).Condition, vars);
                    CrawlVariables(((DIf)node).True, vars);
                    CrawlVariables(((DIf)node).False, vars);
                    break;
                case NodeType.Name:
                    vars.Add(((DName)node).Value);
                    break;
                case NodeType.Unary:
                    CrawlVariables(((DUnaryOperation)node).Node, vars);
                    break;
                case NodeType.Application:
                    {
                        CrawlVariables(((DApplication)node).Target, vars);
                        foreach (var a in ((DApplication)node).Arguments)
                            CrawlVariables(a, vars);
                    }
                    break;
                case NodeType.Index:
                    CrawlVariables(((DIndexer)node).Target, vars);
                    CrawlVariables(((DIndexer)node).Index, vars);
                    break;
                case NodeType.Tuple:
                    {
                        foreach (var n in ((DTupleLiteral)node).Elements)
                            CrawlVariables(n, vars);
                    }
                    break;
                case NodeType.Array:
                    {
                        foreach (var n in ((DArrayLiteral)node).Elements)
                            CrawlVariables(n, vars);
                    }
                    break;
                case NodeType.Iterator:
                    CrawlVariables(((DIteratorLiteral)node).YieldBlock, vars);
                    break;
                case NodeType.YieldBlock:
                    {
                        foreach (var n in ((DYieldBlock)node).Elements)
                            CrawlVariables(n, vars);
                    }
                    break;
                case NodeType.Range:
                    CrawlVariables(((DRange)node).From, vars);
                    CrawlVariables(((DRange)node).Step, vars);
                    CrawlVariables(((DRange)node).To, vars);
                    break;
                case NodeType.Access:
                    CrawlVariables(((DAccess)node).Target, vars);
                    break;
                case NodeType.Throw:
                    CrawlVariables(((DThrow)node).Expression, vars);
                    break;
                case NodeType.As:
                    CrawlVariables(((DAs)node).Expression, vars);
                    break;
            }
        }

        private CompilerError VariableExists(string name)
        {
            var err = GetVariable(name, out _);

            if (err is CompilerError.None)
                return err;

            return err;
        }

        private int PushParentVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetParentVariable(name, out var sv);

            if (err is not CompilerError.None)
            {
                AddError(err, loc, name);
                return default;
            }

            AddLinePragma(loc);
            DirectPushScopeVar(name, sv);
            return sv.Address;
        }

        private void PopParentVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetParentVariable(name, out var sv);

            if (err is not CompilerError.None)
            {
                AddError(err, loc, name);
                return;
            }

            AddLinePragma(loc);
            cw.PopVar(sv.Address);

            if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                AddError(CompilerError.UnableAssignConstant, loc, name);
        }

        private int PushVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetVariable(name, out var sv);

            if (err is not CompilerError.None)
            {
                if (string.IsNullOrEmpty(name))
                    return default;

                if (char.IsUpper(name[0]))
                {
                    var ti = DyType.GetTypeCodeByName(name);

                    if (ti != 0)
                    {
                        AddLinePragma(loc);
                        cw.Type(ti);
                        return -ti;
                    }
                    else
                        AddError(CompilerError.UndefinedType, loc, name);

                    return default;
                }

                AddError(err, loc, name);
                return default;
            }

            AddLinePragma(loc);
            DirectPushScopeVar(name, sv);
            return sv.Address;
        }

        private void DirectPushScopeVar(string name, ScopeVar sv)
        {
            cw.PushVar(sv);

            if ((sv.Data & VarFlags.Lazy) == VarFlags.Lazy)
            {
                cw.CallNullaryFunction();
                cw.Dup();
                cw.PopVar(sv.Address);
                PatchVariable(name, sv, VarFlags.Const);
            }
        }

        private void PopVariable(CompilerContext ctx, string name, Location loc)
        {
            var err = GetVariable(name, out var sv);

            if (err is not CompilerError.None)
            {
                AddError(err, loc, name);
                return;
            }

            AddLinePragma(loc);
            cw.PopVar(sv.Address);
            
            if ((sv.Data & VarFlags.Const) == VarFlags.Const)
                AddError(CompilerError.UnableAssignConstant, loc, name);
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
        private CompilerError GetParentVariable(string name, out ScopeVar var)
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

            if (cur.Locals.TryGetValue(name, out var sv))
            {
                var = new(1 | sv.Address << 8, sv.Data);
                return CompilerError.None;
            }

            var = default;
            return CompilerError.UndefinedBaseVariable;
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

        private void PatchVariable(string name, ScopeVar var, int data)
        {
            var shift = var.Address & byte.MaxValue;
            var cur = currentScope;

            do
            {
                if (shift == 0 && cur.Locals.ContainsKey(name))
                {
                    cur.AddData(name, data);
                    return;
                }

                if (cur.Kind == ScopeKind.Function)
                    shift--;

                cur = cur.Parent;
            }
            while (cur != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CompilerError GetVariable(string name, out ScopeVar var)
        {
            var cur = currentScope;
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
