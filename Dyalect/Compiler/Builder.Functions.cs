using Dyalect.Debug;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System;
using System.Collections.Generic;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //Contains function related compilation logic
    partial class Builder
    {
        private void Build(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            BuildFunctionDeclaration(node, hints.Remove(IteratorBody), ctx);
        }

        private void BuildFunctionDeclaration(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            if (node.Name is not null || node.TargetTypeName is not null)
            {
                var flags = VarFlags.Const | VarFlags.Function;
                var addr = 0;

                if (node.TypeName is null && node.Name is not null)
                    addr = AddVariable(node.Name, node.Location, flags);
                else if (privateScope)
                    AddError(CompilerError.PrivateMethod, node.Location);

                BuildFunctionBody(addr, node, hints, ctx);

                if (hints.Has(Push))
                    cw.Dup();

                if (node.TypeName is not null)
                {
                    if (node.TargetTypeName is not null)
                    {
                        if (node.IsStatic || node.Setter || node.Getter)
                            AddError(CompilerError.InvalidCast, node.Location);

                        var t1 = PushTypeInfo(ctx, node.TargetTypeName, node.Location);

                        if (t1 < 0 && -t1 == DyType.Bool)
                            AddError(CompilerError.BoolCastNotAllowed, node.Location);

                        var t2 = PushTypeInfo(ctx, node.TypeName, node.Location);

                        if (t1 == t2)
                            AddError(CompilerError.SelfCastNotAllowed, node.Location);

                        cw.NewCast();
                    }

                    if (node.IsIndexer)
                    {
                        if (node.IsStatic)
                            AddError(CompilerError.IndexerStatic, node.Location);

                        if (node.Setter && node.Parameters.Count is not 2)
                            AddError(CompilerError.IndexerWrongArguments, node.Location);

                        if (node.Getter && node.Parameters.Count is not 1)
                            AddError(CompilerError.IndexerWrongArguments, node.Location);

                        if (!node.Getter && !node.Setter)
                            AddError(CompilerError.IndexerSetOrGet, node.Location);
                    }

                    if (node.Name is not null)
                    {
                        var realName = node.Name;

                        if (realName != Builtins.Get && realName != Builtins.Set
                            && !char.IsUpper(realName[0]) && !Builtins.OperatorSymbols.Contains(realName[0]))
                            AddError(CompilerError.MemberNameCamel, node.Location);

                        if (!node.IsStatic)
                            realName = GetMethodName(realName, node);

                        if (node.Setter && !node.IsIndexer)
                        {
                            realName = Builtins.Setter(realName);

                            if (node.Parameters.Count != 1)
                                AddError(CompilerError.SetterWrongArguments, node.Location);
                        }

                        if (node.Getter && !node.IsIndexer && node.Parameters.Count > 0)
                            AddError(CompilerError.GetterWrongArguments, node.Location);

                        if (node.Name is Builtins.Has || (!node.IsStatic && node.Name is Builtins.Type))
                            AddError(CompilerError.OverrideNotAllowed, node.Location, node.Name);

                        PushTypeInfo(ctx, node.TypeName, node.Location);

                        if (node.IsStatic)
                            cw.SetMemberS(realName);
                        else
                            cw.SetMember(realName);
                    }
                }

                AddLinePragma(node);

                if (node.TypeName is not null)
                    cw.Nop();
                else
                    cw.PopVar(addr);
            }
            else
            {
                BuildFunctionBody(-1, node, hints, ctx);
                AddLinePragma(node);
                cw.Nop();
                PopIf(hints);
            }
        }

        //Converts symbolic names (used when overriding operators) to special internal
        //names, e.g. "*" becomes "__op_mul"
        private string GetMethodName(string name, DFunctionDeclaration node)
        {
            switch (name)
            {
                case "+" when node.Parameters.Count == 0: return Builtins.Plus;
                case "-" when node.Parameters.Count == 0: return Builtins.Neg;
                case "!":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Not;
                case "~~~":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.BitNot;
                case "Length":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "ToLiteral":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "Iterate":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "Dispose":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "Clone":
                    if (node.Parameters.Count > 0) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "ToString":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return name;
                case "+": 
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Add;
                case "-":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Sub;
                case "*":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Mul;
                case "/":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Div;
                case "<<<":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Shl;
                case ">>>":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Shr;
                case "==":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Eq;
                case "!=":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Neq;
                case ">":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Gt;
                case "<":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Lt;
                case ">=":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Gte;
                case "<=":
                    if (node.Parameters.Count > 1) AddError(CompilerError.BuiltinWrongArguments, node.Location);
                    return Builtins.Lte;
                default:
                    return name;
            }
        }

        //Compilation of function parameters with support for variable
        //arguments and default values
        private Par[] CompileFunctionParameters(DFunctionDeclaration node)
        {
            if (node.IsNullary && node.Body is not null && node.Body.NodeType != NodeType.Block)
            {
                var set = new HashSet<string>();

                if (!CrawlVariables(node.Body, set))
                    return Array.Empty<Par>();

                var pars = new List<Par>();

                foreach (var n in set)
                    pars.Add(new Par(n));

                return pars.ToArray();
            }
            else
            {
                var pars = node.Parameters;
                var arr = new Par[pars.Count];
                var hasVarArg = false;

                for (var i = 0; i < pars.Count; i++)
                {
                    var p = pars[i];

                    if (p.IsVarArgs)
                    {
                        if (hasVarArg)
                            AddError(CompilerError.VarArgOnlyOne, p.Location);

                        hasVarArg = true;
                    }

                    if (p.DefaultValue is not null)
                    {
                        if (p.IsVarArgs)
                            AddError(CompilerError.VarArgNoDefaultValue, p.Location);

                        DyObject? val = null;

                        switch (p.DefaultValue.NodeType)
                        {
                            case NodeType.Integer:
                                val = new DyInteger(((DIntegerLiteral)p.DefaultValue).Value);
                                if (!CheckRestriction(DyType.Integer, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            case NodeType.Float:
                                val = new DyFloat(((DFloatLiteral)p.DefaultValue).Value);
                                if (!CheckRestriction(DyType.Float, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            case NodeType.Char:
                                val = new DyChar(((DCharLiteral)p.DefaultValue).Value);
                                if (!CheckRestriction(DyType.Char, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            case NodeType.Boolean:
                                val = ((DBooleanLiteral)p.DefaultValue).Value ? DyBool.True : DyBool.False;
                                if (!CheckRestriction(DyType.Bool, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            case NodeType.String:
                                val = new DyString(((DStringLiteral)p.DefaultValue).Value);
                                if (!CheckRestriction(DyType.String, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            case NodeType.Nil:
                                val = DyNil.Instance;
                                if (!CheckRestriction(DyType.Nil, p.TypeAnnotation))
                                    AddError(CompilerError.InvalidTypeDefaultValue, p.DefaultValue.Location);
                                break;
                            default:
                                AddError(CompilerError.InvalidDefaultValue, p.DefaultValue.Location, p.Name);
                                break;
                        }

                        arr[i] = new Par(p.Name, val, false, p.TypeAnnotation);
                    }
                    else
                        arr[i] = new Par(p.Name, null, p.IsVarArgs, p.TypeAnnotation);
                }

                return arr;
            }
        }

        private bool CheckRestriction(int code, TypeAnnotation? restriction)
        {
            if (restriction is null)
                return true;

            foreach (var q in restriction)
                if (q.Parent is not null || code != DyType.GetTypeCodeByName(q.Local))
                    return false;

            return true;
        }

        private int BuildFunctionArguments(CompilerContext ctx, DFunctionDeclaration node, Par[] args)
        {
            var variadicIndex = -1;

            //Initialize function arguments
            if (args.Length > 0)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];

                    if (arg.IsVarArg)
                        variadicIndex = i;

                    var sys = AddVariable(arg.Name, node.Location, data: VarFlags.Argument);

                    if (arg.TypeAnnotation is not null)
                    {
                        var skip = cw.DefineLabel();
                        AddLinePragma(node.Parameters[i]);

                        //We type check for all the types in annotation. If at least one
                        //is a success, we goto to the exit section
                        foreach (var q in arg.TypeAnnotation)
                        {
                            cw.PushVar(new ScopeVar(sys));
                            PushTypeInfo(ctx, q, node.Location);
                            cw.TypeCheck();
                            cw.Brtrue(skip);
                        }

                        //We fall in here if all checks are not successful
                        //Here we try to obtain some additional info to generate nice
                        //error message
                        ThrowErrorProlog(DyErrorCode.InvalidType, 1);
                        cw.PushVar(new ScopeVar(sys));
                        cw.CallNullaryMember(Builtins.Type);
                        cw.FunArgIx(0);
                        cw.FunCall(1);
                        cw.Fail();
                        //Exit section for success
                        cw.MarkLabel(skip);
                        cw.Nop();
                    }
                }
            }

            return variadicIndex;
        }

        private void BuildFunctionBody(int addr, DFunctionDeclaration node, Hints hints, CompilerContext oldctx)
        {
            var iterBody = hints.Has(IteratorBody);
            var args = CompileFunctionParameters(node);
            StartFun(node.Setter ? Builtins.Setter(node.Name!) : node.Name!, args);

            if (node.IsStatic && node.TypeName is null)
                AddError(CompilerError.StaticOnlyMethods, node.Location, node.Name!);

            var startLabel = cw.DefineLabel();
            var funEndLabel = cw.DefineLabel();

            //Functions are always compiled "in place": if we find a function while looping
            //through AST node we compile right away. That is the reason why we need to emit an
            //additional goto so that we can jump over this function.
            var funSkipLabel = cw.DefineLabel();
            cw.Br(funSkipLabel);
            hints = Function | Push | (iterBody ? IteratorBody : None);

            if (node.TypeName is not null && oldctx.Function is not null && oldctx.Function.TypeName is not null)
                AddError(CompilerError.NestedMethod, node.Location);

            var ctx = new CompilerContext
            {
                FunctionAddress = counters.Count | (addr >> 8) << 8,
                FunctionStart = startLabel,
                FunctionExit = funEndLabel,
                Function = node
            };

            //Start of a physical (and not compiler time) lexical scope for a function
            StartScope(ScopeKind.Function, loc: node.Location);
            StartSection();

            AddLinePragma(node);
            var address = cw.Offset;
            
            //args was here
            var variadicIndex = BuildFunctionArguments(ctx, node, args);

            //If this is a member function we add an additional system variable that
            //would return an instance of an object to which this function is coupled
            //(same as "this" in C#)
            if (node.TypeName is not null && !node.IsStatic)
            {
                var va = AddVariable("this", node.Location, data: VarFlags.Const | VarFlags.This);
                cw.This();
                cw.PopVar(va);

                va = AddVariable("ini", node.Location, data: VarFlags.Const);
                cw.Unbox();
                cw.PopVar(va);
            }
            
            if ((node.Getter || node.Setter) && node.TypeName is null)
                AddError(CompilerError.AccessorOnlyMethod, node.Location);

            //Start of a function that is used for tail call optimization
            cw.MarkLabel(startLabel);

            if (node.IsConstructor) //This is a constructor
                GenerateConstructor(node, ctx);
            else if (node.IsIterator) //Compile function body
            {
                var dec = new DFunctionDeclaration(node.Location) { Name = node.Name, Body = node.Body };
                BuildFunctionDeclaration(dec, hints.Append(IteratorBody), ctx);
            }
            else
                Build(node.Body!, hints.Append(Last), ctx);

            //Return from a function. Any function execution should get here, it is not possible
            //to break early. An early return would actually goto here, where a real return (OpCode.Ret)
            //is executed. This is a function epilogue.
            cw.MarkLabel(funEndLabel);

            //If this is an iterator function push a terminator at the end (and pop a normal value)
            if (iterBody)
            {
                cw.Pop();
                cw.PushNilT();
            }

            cw.Ret();
            cw.MarkLabel(funSkipLabel);
            AddLinePragma(node);

            //Close all lexical scopes and debugging information
            var funHandle = unit.Layouts.Count;
            var ss = EndFun(funHandle);
            unit.Layouts.Add(new MemoryLayout(currentCounter, ss, address));
            EndScope();
            EndSection();

            //Iterators are a separate type (based on function through)
            if (iterBody)
                cw.NewIter(funHandle);
            else
            {
                if (variadicIndex > -1)
                {
                    cw.RgDI(variadicIndex);
                    cw.NewFunV(funHandle);
                }
                else
                    cw.NewFun(funHandle);
            }
            
            if (node.Getter)
                cw.FunAttr(FunAttr.Auto);

            if (node.IsPrivate)
                cw.FunAttr(FunAttr.Priv);
        }
    }
}
