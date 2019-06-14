using Dyalect.Debug;
using Dyalect.Parser.Model;
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
            if (node.Name != null)
            {
                var flags = VarFlags.Const | VarFlags.Function;
                var addr = 0;

                if (!node.IsMemberFunction)
                    addr = AddVariable(node.Name, node, flags | VarFlags.Exported);

                BuildFunctionBody(node, hints, ctx);

                if (hints.Has(Push))
                    cw.Dup();

                if (node.IsMemberFunction)
                {
                    var realName = node.Name;

                    if (!node.IsStatic)
                        realName = GetMethodName(realName, node);

                    if (!node.IsStatic && 
                        (node.Name == Builtins.Has || node.Name == Builtins.GetType)
                        )
                        AddError(CompilerError.OverrideNotAllowed, node.Location, node.Name);

                    var nameId = GetMemberNameId(realName);
                    cw.Aux(nameId);
                    var code = GetTypeHandle(node.TypeName, node.Location);

                    if (node.IsStatic)
                        cw.SetMemberS(code);
                    else
                        cw.SetMember(code);
                }

                AddLinePragma(node);

                if (node.IsMemberFunction)
                    cw.Nop();
                else
                    cw.PopVar(addr);
            }
            else
            {
                BuildFunctionBody(node, hints, ctx);
                AddLinePragma(node);
                cw.Nop();
                PopIf(hints);
            }
        }

        private string GetMethodName(string name, DFunctionDeclaration node)
        {
            switch (name)
            {
                case "+": return node.Parameters.Count == 0 ? Builtins.Plus : Builtins.Add;
                case "-": return node.Parameters.Count == 0 ? Builtins.Neg : Builtins.Sub;
                case "get": return Builtins.Get;
                case "set": return Builtins.Set;
                case "*": return Builtins.Mul;
                case "/": return Builtins.Div;
                case "%": return Builtins.Rem;
                case "<<": return Builtins.Shl;
                case ">>": return Builtins.Shr;
                case "^": return Builtins.Xor;
                case "==": return Builtins.Eq;
                case "!=": return Builtins.Neq;
                case ">": return Builtins.Gt;
                case "<": return Builtins.Lt;
                case ">=": return Builtins.Gte;
                case "<=": return Builtins.Lte;
                case "!": return Builtins.Not;
                case "~": return Builtins.BitNot;
                default: return name;
            }
        }

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

                if (p.DefaultValue != null)
                {
                    if (p.IsVarArgs)
                        AddError(CompilerError.VarArgNoDefaultValue, p.Location);

                    DyObject val = null;

                    switch (p.DefaultValue.NodeType)
                    {
                        case NodeType.Integer:
                            val = new DyInteger(((DIntegerLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Float:
                            val = new DyFloat(((DFloatLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Char:
                            val = new DyChar(((DCharLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Boolean:
                            val = ((DBooleanLiteral)p.DefaultValue).Value ? DyBool.True : DyBool.False;
                            break;
                        case NodeType.String:
                            val = new DyString(((DStringLiteral)p.DefaultValue).Value);
                            break;
                        case NodeType.Nil:
                            val = DyNil.Instance;
                            break;
                        default:
                            AddError(CompilerError.InvalidDefaultValue, p.DefaultValue.Location, p.Name);
                            break;
                    }

                    arr[i] = new Par(p.Name, val, false);
                }
                else
                    arr[i] = new Par(p.Name, null, p.IsVarArgs);
            }

            return arr;
        }

        private void BuildFunctionBody(DFunctionDeclaration node, Hints hints, CompilerContext ctx)
        {
            var iter = hints.Has(Iterator);
            var args = CompileFunctionParameters(node.Parameters);
            var argCount = args.Length;
            StartFun(node.Name, args, argCount);

            if (node.IsStatic && !node.IsMemberFunction)
                AddError(CompilerError.StaticOnlyMethods, node.Location, node.Name);

            var startLabel = cw.DefineLabel();
            var funEndLabel = cw.DefineLabel();

            //Functions are always compiled "in place": if we find a function while looping
            //through AST node we compile right away. That is the reason why we need to emit an 
            //additional goto so that we can jump over this function.
            var funSkipLabel = cw.DefineLabel();
            cw.Br(funSkipLabel);

            ctx = new CompilerContext
            {
                FunctionExit = funEndLabel
            };

            hints = Function | Push;

            //Actual start of a function
            cw.MarkLabel(startLabel);

            //Start of a physical (and not compiler time) lexical scope for a function
            StartScope(fun: true, loc: node.Location);
            StartSection();

            AddLinePragma(node);
            var address = cw.Offset;
            var variadicIndex = -1;

            //Initialize function arguments
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.IsVarArg)
                    variadicIndex = i;

                AddVariable(arg.Name, node, data: VarFlags.Argument);
            }

            //If this is a member function we add an additional system variable that
            //would return an instance of an object to which this function is coupled
            //(same as this in C#)
            if (node.IsMemberFunction && !node.IsStatic)
            {
                var va = AddVariable("this", node, data: VarFlags.Const);
                cw.This();
                cw.PopVar(va);
            }

            //Compile function body
            if (node.IsIterator)
            {
                var dec = new DFunctionDeclaration(node.Location) { Name = node.Name, Body = node.Body };
                Build(dec, hints.Append(Iterator), ctx);
            }
            else
                Build(node.Body, hints.Append(Last), ctx);

            //Return from a function. Any function execution should get here, it is not possible
            //to break early. An early return would actually goto here, where a real return (OpCode.Ret)
            //is executed. This is a function epilogue.
            cw.MarkLabel(funEndLabel);

            //If this is an iterator function push a terminator at the end (and pop a normal value)
            if (iter)
            {
                cw.Pop();
                cw.PushNilT();
            }

            if (node.IsMemberFunction && node.IsStatic && node.Name == Builtins.New
                && node.TypeName.Parent == null
                && localTypes.TryGetValue(node.TypeName.Local, out var ti))
            {
                cw.NewType(ti.TypeId);
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
            if (iter)
                cw.NewIter(funHandle);
            else
            {
                if (variadicIndex > -1)
                {
                    cw.Aux(variadicIndex);
                    cw.NewFunV(funHandle);
                }
                else
                    cw.NewFun(funHandle);
            }
        }

        private int GetMemberNameId(string name)
        {
            if (!memberNames.TryGetValue(name, out var id))
            {
                id = unit.MemberIds.Count;
                memberNames.Add(name, id);
                unit.MemberIds.Add(-1);
                unit.MemberNames.Add(name);
            }

            return id;
        }
    }
}
