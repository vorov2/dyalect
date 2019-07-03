using Dyalect.Debug;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Parser.Model;
using System;
using System.Collections.Generic;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    internal sealed partial class Builder
    {
        private const int ERROR_LIMIT = 100;

        private readonly bool iterative; //Compilation is performed in interactive mode
        private readonly BuilderOptions options; //Build options
        private readonly CodeWriter cw; //Helper for byte code emit
        private readonly DyLinker linker; //Linker to link referenced modules
        private Scope globalScope; //Global scope (for variables) of the current unit
        private Unit unit; //Unit (file) that is beign compiler
        private Scope currentScope; //Current lexical scope
        private Label programEnd; //Label that marks an end of program
        private Dictionary<string, UnitInfo> referencedUnits;

        private Dictionary<string, TypeInfo> types;
        private Dictionary<string, TypeInfo> localTypes;
        private Dictionary<string, int> memberNames;

        private readonly static DImport defaultInclude = new DImport(default) { ModuleName = "lang" };

        public Builder(BuilderOptions options, DyLinker linker)
        {
            this.referencedUnits = new Dictionary<string, UnitInfo>();
            this.types = new Dictionary<string, TypeInfo>();
            this.localTypes = new Dictionary<string, TypeInfo>();
            this.memberNames = new Dictionary<string, int>();

            this.options = options;
            this.linker = linker;
            counters = new Stack<int>();
            pdb = new DebugWriter();
            isDebug = options.Debug;
            globalScope = new Scope(true, null);
            unit = new Unit
            {
                GlobalScope = globalScope,
                Symbols = pdb.Symbols
            };
            cw = new CodeWriter(unit);
            currentScope = globalScope;
            programEnd = cw.DefineLabel();
        }

        public Builder(Builder builder)
        {
            this.iterative = true;
            this.linker = builder.linker;
            this.imports = builder.imports;
            this.types = builder.types;
            this.localTypes = builder.localTypes;
            this.memberNames = builder.memberNames;
            this.referencedUnits = builder.referencedUnits;
            this.counters = new Stack<int>();
            this.options = builder.options;
            this.pdb = builder.pdb.Clone();
            this.unit = builder.unit.Clone(this.pdb.Symbols);
            this.cw = builder.cw.Clone(this.unit);
            this.globalScope = unit.GlobalScope;
            this.currentScope = builder.currentScope != builder.globalScope
                ? builder.currentScope.Clone() : this.globalScope;
            this.isDebug = builder.isDebug;
            this.lastLocation = builder.lastLocation;
            this.Messages = new List<BuildMessage>();
            this.counters = new Stack<int>(builder.counters.ToArray());
            this.currentCounter = builder.currentCounter;
            this.programEnd = cw.DefineLabel();
        }

        public Unit Build(DyCodeModel codeModel)
        {
            Messages.Clear();
            unit.FileName = codeModel.FileName;

            //It is used internally, so we need to add it even if the code doesn't reference it
            GetMemberNameId(Builtins.Iterator);

            if (unit.Layouts.Count == 0)
                unit.Layouts.Add(null); //A layout reserved for the top level

            cw.StartFrame(); //Start a new global frame
            var res = TryBuild(codeModel);

            if (!res)
                return null;

            cw.MarkLabel(programEnd);
            cw.Term(); //Program should always end with this op code
            cw.CompileOpList();

            //Finalizing compilation, fixing top level layout
            unit.Layouts[0] = new MemoryLayout(currentCounter, cw.FinishFrame(), 0);
            return unit;
        }

        private bool TryBuild(DyCodeModel codeModel)
        {
            try
            {
                var ctx = new CompilerContext();

                if (!options.NoLangModule && !iterative)
                    BuildImport(defaultInclude, ctx);

                foreach (var imp in codeModel.Imports)
                    BuildImport(imp, ctx);

                unit.UnitIds.Add(0);
                var root = codeModel.Root;

                //What if we have no code, only imports? Useless, but we shouldn't crush in this case
                if (root.Nodes.Count == 0)
                    cw.PushNil();

                for (var i = 0; i < root.Nodes.Count; i++)
                {
                    var n = root.Nodes[i];
                    var last = i == root.Nodes.Count - 1;
                    Build(n, last ? Push : None, ctx);
                }

                return Success;
            }
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
        private void PopIf(Hints hints)
        {
            if (!hints.Has(Push))
                cw.Pop();
        }

        //If some code block usually don't yield a value but in this specific context
        //we need a value we have to push a default 'nil' onto the stack
        private void PushIf(Hints hints)
        {
            if (hints.Has(Push))
                cw.PushNil();
        }

        //This method is called to check if we really need to emit code
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

        private Exception Ice(Exception ex = null)
        {
            return new DyBuildException(
                $"Внутренняя ошибка компилятора: {(ex != null ? ex.Message : "Недопустимая операция.")}", ex);
        }
    }
}
