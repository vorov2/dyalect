using Dyalect.Debug;
using Dyalect.Parser.Model;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections.Generic;
using static Dyalect.Compiler.Hints;

namespace Dyalect.Compiler
{
    //Contains function related compilation logic
    partial class Builder
    {
        private void Build(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            if (node.Name is not null)
            {
                var flags = VarFlags.Const | VarFlags.Function;
                var addr = 0;

                if (node.TypeName is null)
                    addr = AddVariable(node.Name, node.Location, flags);
                else if (privateScope)
                    AddError(CompilerError.PrivateMethod, node.Location);

                BuildFunctionBody(addr, node, hints, ctx);

                if (hints.Has(Push))
                    cw.Dup();

                if (node.TypeName is not null)
                {
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

                    var realName = node.Name;

                    if (realName != Builtins.Get && realName != Builtins.Set
                        && !char.IsUpper(realName[0]) && !Builtins.OperatorSymbols.Contains(realName[0]))
                        AddError(CompilerError.MemberNameCamel, node.Location);

                    if (!node.IsStatic)
                        realName = GetMethodName(realName, node);

                    if (node.Setter && !node.IsIndexer)
                        realName = Builtins.Setter(realName);

                    if (node.Name is Builtins.Has || (!node.IsStatic && node.Name is Builtins.Type))
                        AddError(CompilerError.OverrideNotAllowed, node.Location, node.Name);

                    PushTypeInfo(ctx, node.TypeName, node.Location);

                    if (node.IsStatic)
                        cw.SetMemberS(realName);
                    else
                        cw.SetMember(realName);
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
        //names, e.g. "*" becomes "op_mul"
        private string GetMethodName(string name, DFunctionDeclaration node) =>
            name switch
            {
                "+" => node.Parameters.Count == 0 ? Builtins.Plus : Builtins.Add,
                "-" => node.Parameters.Count == 0 ? Builtins.Neg : Builtins.Sub,
                "GetItem" => Builtins.Get,
                "SetItem" => Builtins.Set,
                "*" => Builtins.Mul,
                "/" => Builtins.Div,
                "%" => Builtins.Rem,
                "<<<" => Builtins.Shl,
                ">>>" => Builtins.Shr,
                "^" => Builtins.Xor,
                "==" => Builtins.Eq,
                "!=" => Builtins.Neq,
                ">" => Builtins.Gt,
                "<" => Builtins.Lt,
                ">=" => Builtins.Gte,
                "<=" => Builtins.Lte,
                "!" => Builtins.Not,
                "~~~" => Builtins.BitNot,
                _ => name
            };

        //Compilation of function parameters with support for variable
        //arguments and default values
        private Par[] CompileFunctionParameters(List<DParameter> pars)
        {
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

        private bool CheckRestriction(int code, Qualident? restriction)
        {
            if (restriction is null)
                return true;

            if (restriction.Parent is not null)
                return false;

            return code == DyType.GetTypeCodeByName(restriction.Local);
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

                    var a = AddVariable(arg.Name, node.Location, data: VarFlags.Argument);

                    if (arg.TypeAnnotation is not null)
                    {
                        cw.PushVar(new ScopeVar(a));
                        cw.Dup();
                        PushTypeInfo(ctx, arg.TypeAnnotation, node.Location);
                        AddLinePragma(node.Parameters[i]);
                        cw.TypeCheck(); 
                        var skip = cw.DefineLabel();
                        cw.Brtrue(skip);
                        cw.GetMember(Builtins.Type);
                        cw.FunPrep(0);
                        cw.FunCall(0);
                        cw.NewErr(DyErrorCode.InvalidType, 1);
                        cw.Fail();
                        cw.MarkLabel(skip);
                        cw.Pop();
                    }
                }
            }

            return variadicIndex;
        }

        private void BuildFunctionBody(int addr, DFunctionDeclaration node, Hints hints, CompilerContext oldctx)
        {
            var iterBody = hints.Has(IteratorBody);
            var args = CompileFunctionParameters(node.Parameters);
            StartFun(node.Setter ? Builtins.Setter(node.Name) : node.Name!, args);

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

            //This is an autogenerated constructor
            if (node.IsConstructor && node.Body is null)
                GenerateConstructor(node, ctx);
            else if (node.IsIterator) //Compile function body
            {
                var dec = new DFunctionDeclaration(node.Location) { Name = node.Name, Body = node.Body };
                Build(dec, hints.Append(IteratorBody), ctx);
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

            if (node.IsConstructor && node.Body is not null)
            {
                //Constructor returns a type instance, not a value. We need to pop this value from stack
                cw.Pop();

                //Push privates to stack
                PushVariable(ctx, "this", node.Body.Location);
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
