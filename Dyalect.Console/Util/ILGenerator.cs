using Dyalect.Compiler;
using Dyalect.Parser;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dyalect.Debug;
namespace Dyalect.Util;

public static class ILGenerator
{
    public static string Generate(Unit unit)
    {
        var sb = new StringBuilder();
        Generate(sb, unit);
        return sb.ToString();
    }

    private static bool TryGetVarSym(int offset, Unit unit, Op op, bool loc, out VarSym? vs)
    {
        vs = null;

        if (unit.Symbols is not null)
        {
            var scopeIndex = 0;

            if (!loc)
            {
                var sym = unit.Symbols.FindScopeSym(offset);

                if (sym is null)
                    return false;

                scopeIndex = loc ? 0 : sym.Index - (op.Data >> 8);
            }

            vs = unit.Symbols.FindVarSym(op.Data & byte.MaxValue, scopeIndex);
            return vs is not null;
        }

        return false;
    }

    private static string GetFunSym(Unit unit, Op op)
    {
        var fs = unit.Symbols.Functions[op.Data];
        return fs.Name is null ? "<unnamed>" : fs.Name;
    }

    private static void Generate(StringBuilder sb, Unit unit)
    {
        for (var i = 0; i < unit.Ops.Count; i++)
        {
            var op = unit.Ops[i];
            sb.Append(i.ToString().PadLeft(5, '0'));
            sb.Append(": ");
            sb.Append(op.Code.ToString());

            switch (op.Code)
            {
                case OpCode.PushI8:
                case OpCode.PushR8:
                    sb.Append($" {unit.Objects[op.Data].ToObject()}");
                    break;
                case OpCode.PushStr:
                    var str = unit.Objects[op.Data].ToString();
                    sb.Append($" {StringUtil.Escape(str)}");
                    break;
                case OpCode.PushCh:
                    var str2 = unit.Objects[op.Data].ToString();
                    sb.Append($" {StringUtil.Escape(str2, "'")}");
                    break;
                case OpCode.Br:
                case OpCode.Brtrue:
                case OpCode.Brterm:
                case OpCode.Brfalse:
                case OpCode.Pushext:
                case OpCode.NewIter:
                case OpCode.RunMod:
                case OpCode.Type:
                case OpCode.RgDI:
                case OpCode.RgFI:
                case OpCode.FunPrep:
                case OpCode.FunArgIx:
                case OpCode.FunCall:
                case OpCode.NewTuple:
                case OpCode.Start:
                case OpCode.FunAttr:
                case OpCode.StdCall:
                case OpCode.NewArgs:
                case OpCode.NewDict:
                    sb.Append($" #{op.Data}");
                    break;
                case OpCode.Poploc:
                case OpCode.Pushloc:
                    {
                        if (TryGetVarSym(i, unit, op, true, out var vs))
                            sb.Append($" {vs!.Name}");
                        else
                            sb.Append($" #{op.Data}");
                    }
                    break;
                case OpCode.Popvar:
                case OpCode.Pushvar:
                    {
                        if (TryGetVarSym(i, unit, op, false, out var vs))
                            sb.Append($" {vs!.Name}");
                        else
                            sb.Append($" #{op.Data}");
                    }
                    break;
                case OpCode.NewFun:
                case OpCode.NewFunV:
                    sb.Append($" {GetFunSym(unit, op)}");
                    break;
                case OpCode.SetMemberS:
                case OpCode.SetMember:
                case OpCode.GetMember:
                case OpCode.HasMember:
                case OpCode.Tag:
                case OpCode.FunArgNm:
                case OpCode.NewObj:
                case OpCode.NewType:
                case OpCode.CtorCheck:
                case OpCode.GetPriv:
                case OpCode.SetPriv:
                    sb.Append($" {StringUtil.Escape((string)unit.Strings[op.Data])}");
                    break;
                case OpCode.Contains:
                    sb.Append($" {StringUtil.Escape(unit.Objects[op.Data].ToObject().ToString() ?? "")}");
                    break;
            }

            sb.AppendLine();
        }
    }
}
