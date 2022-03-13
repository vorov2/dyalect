using Dyalect.Runtime.Types;
using Dyalect.Linker;
using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    internal sealed partial class Builder
    {
        private const int ERROR_LIMIT = 100;

        private readonly bool iterative; //Compilation is performed in interactive mode
        private readonly BuilderOptions options; //Build options
        private readonly CodeWriter cw; //Helper for byte code emit
        private readonly Scope globalScope; //Global scope (for variables) of the current unit
        private readonly Unit unit; //Unit (file) that is beign compiler
        private Scope currentScope; //Current lexical scope
        private readonly Label programEnd; //Label that marks an end of program
        private readonly DyLinker linker; //Linker
        private readonly Dictionary<string, UnitInfo> referencedUnits;

        private readonly Dictionary<string, TypeInfo> types;
        private readonly static DImport defaultInclude = new(default) { ModuleName = "lang" };

        public Builder(BuilderOptions options, DyLinker linker)
        {
            referencedUnits = new();
            types = new();

            this.options = options;
            this.linker = linker;
            counters = new();
            pdb = new();
            isDebug = options.Debug;
            globalScope = new(ScopeKind.Function, null);
            unit = new()
            {
                GlobalScope = globalScope,
                Symbols = pdb.Symbols
            };
            cw = new(unit);
            currentScope = globalScope;
            programEnd = cw.DefineLabel();
        }

        public Builder(Builder builder)
        {
            iterative = true;
            linker = builder.linker;
            types = builder.types;
            referencedUnits = builder.referencedUnits;
            counters = new();
            options = builder.options;
            pdb = builder.pdb.Clone();
            unit = builder.unit.Clone(pdb.Symbols);
            cw = builder.cw.Clone(unit);
            globalScope = unit.GlobalScope!;
            currentScope = builder.currentScope != builder.globalScope
                ? builder.currentScope.Clone() : globalScope;
            isDebug = builder.isDebug;
            lastLocation = builder.lastLocation;
            Messages = new();
            counters = new(builder.counters.ToArray());
            currentCounter = builder.currentCounter;
            programEnd = cw.DefineLabel();
        }

        public Unit? Build(DyCodeModel codeModel)
        {
            Messages.Clear();
            unit.FileName = codeModel.FileName;

            if (unit.Layouts.Count == 0)
                unit.Layouts.Add(null!); //A layout reserved for the top level

            cw.StartFrame(); //Start a new global frame
            var res = TryBuild(codeModel);

            if (!res)
                return null;

            cw.MarkLabel(programEnd);
            cw.Term(); //Program should always end with this op code
            cw.CompileOpList();

            //Finalizing compilation, fixing top level layout
            unit.Layouts[0] = new(currentCounter, cw.FinishFrame(), 0);
            return unit;
        }

        //Main build cycle with error handling logic
        private bool TryBuild(DyCodeModel codeModel)
        {
            try
            {
                var ctx = new CompilerContext();

                if (!options.NoLangModule && !iterative)
                    BuildImport(defaultInclude);

                foreach (var imp in codeModel.Imports)
                    BuildImport(imp);

                //This is a self-reference to simplify type resolution
                unit.UnitIds.Add(0);
                var root = codeModel.Root;

                //What if we have no code, just imports? We shouldn't crush in this case
                if (root.Nodes.Count == 0)
                    cw.PushNil();

                for (var i = 0; i < root.Nodes.Count; i++)
                {
                    var n = root.Nodes[i];
                    var last = i == root.Nodes.Count - 1;
                    Build(n, last ? Push : None, ctx);
                }

                //Dispose auto's declared in global scope
                CallAutos(cls: true);
                return Success;
            }
            //This is thrown when an error limit is exceeded
            catch (TerminationException)
            {
                return false;
            }
#if !DEBUG
            catch (Exception ex)
            {
                throw Ice(ex);
            }
#endif
        }

        //If some code block usually yields a value but in this specific context
        //we don't need it we have to pop it from stack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PopIf(Hints hints)
        {
            if (!hints.Has(Push))
                cw.Pop();
        }

        //If some code block usually don't yield a value but in this specific context
        //we need a value we have to push a default 'nil' onto the stack
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushIf(Hints hints)
        {
            if (hints.Has(Push))
                cw.PushNil();
        }

        //This method is called to check if we really need to emit code
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool NoPush(DNode node, Hints hints)
        {
            if (!hints.Has(Push))
            {
                AddLinePragma(node);
                cw.Nop();
                return true;
            }

            return false;
        }

        private DyBuildException Ice(Exception? ex = null) => new($"Internal compiler error: {ex?.Message}", ex);
    }
}
