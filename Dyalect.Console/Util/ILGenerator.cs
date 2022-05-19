using Dyalect.Compiler;
using Dyalect.Debug;
using Dyalect.Parser;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dyalect.Util;

public static class ILGenerator
{
    private static readonly char[] cz = new char[] { '\\', '/' };

    public static string Generate(IEnumerable<Unit> units)
    {
        var builder = new StringBuilder();

        foreach (var u in units)
        {
            var sb = new StringBuilder();
            Generate(sb, u);

            if (sb.Length > 0)
            {
                builder.AppendLine();
            
                if (u.FileName is not null)
                {
                    if (u.FileName.IndexOfAny(cz) == -1)
                        builder.AppendLine($"{u.FileName} (size {u.Ops.Count}):");
                    else
                    {
                        var fi = new FileInfo(u.FileName);
                        builder.AppendLine($"{fi.Directory?.Name}/{fi.Name} (size {u.Ops.Count}):");
                    }
                }
                else
                    builder.AppendLine($"Size {u.Ops.Count}:");

                builder.Append(sb);
            }
        }

        return builder.ToString();
    }

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

    private static string GetFunName(FunSym funSym)
    {
        if (funSym.Name is not null && funSym.TypeName is not null)
            return $"{funSym.TypeName}.{funSym.Name}";
        else if (funSym.Name is not null)
            return funSym.Name;
        else
            return "lambda@" + funSym.Handle;
    }

    private static string GetFunSym(Unit unit, Op op)
    {
        var fs = unit.Symbols.Functions[op.Data];
        return GetFunName(fs);
    }

    private static string FormatOffset(int offset) => offset.ToString().PadLeft(5, '0');

    private static void Generate(StringBuilder sb, Unit unit)
    {
        var funs = new Stack<FunSym>();

        for (var i = 0; i < unit.Ops.Count; i++)
        {
            var op = unit.Ops[i];
            sb.Append(FormatOffset(i));
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
                case OpCode.Start:
                    sb.Append(" " + FormatOffset(op.Data));
                    break;
                case OpCode.Pushext:
                case OpCode.Type:
                case OpCode.RunMod:
                case OpCode.NewIter:
                    sb.Append(" " + op.Data);
                    break;
                case OpCode.RgDI:
                case OpCode.RgFI:
                case OpCode.FunPrep:
                case OpCode.FunArgIx:
                case OpCode.FunCall:
                case OpCode.NewTuple:
                case OpCode.StdCall:
                case OpCode.NewArgs:
                case OpCode.NewDict:
                    sb.Append(" " + op.Data);
                    break;
                case OpCode.FunAttr:
                    if ((op.Data & 0x01) == 0x01)
                        sb.Append(" Auto");
                    if ((op.Data & 0x02) == 0x02)
                        sb.Append(" Variadic");
                    if ((op.Data & 0x04) == 0x04)
                        sb.Append(" Final");
                    break;
                case OpCode.Poploc:
                case OpCode.Pushloc:
                    {
                        if (TryGetVarSym(i, unit, op, true, out var vs))
                            sb.Append(" " + vs!.Name);
                        else
                            sb.Append(" #" + op.Data);
                    }
                    break;
                case OpCode.Popvar:
                case OpCode.Pushvar:
                    {
                        if (TryGetVarSym(i, unit, op, false, out var vs))
                            sb.Append(" " + vs!.Name);
                        else
                            sb.Append(" #" + op.Data);
                    }
                    break;
                case OpCode.NewFun:
                case OpCode.NewFunV:
                    sb.Append(" " + GetFunSym(unit, op));
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
                    sb.Append(" " + StringUtil.Escape((string)unit.Strings[op.Data]));
                    break;
                case OpCode.Contains:
                    sb.Append(" " + StringUtil.Escape(unit.Objects[op.Data].ToObject().ToString() ?? ""));
                    break;
            }

            var funSym = unit.Symbols?.FindFunSymByStart(i - 1);

            if (funSym is not null)
            {
                sb.Append($" //Start of {GetFunName(funSym)}");
                funs.Push(funSym);
            }
            else if (funs.Count > 0 && funs.Peek().EndOffset == i + 1)
            {
                funSym = funs.Pop();
                sb.Append($" //End of {GetFunName(funSym)}");
            }

            sb.AppendLine();
        }
    }
}
