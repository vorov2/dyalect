using System.Collections.Generic;
using System.Reflection;
namespace Dyalect.Compiler;

public sealed class Op
{
    public static readonly Op Nop = new(OpCode.Nop);
    public static readonly Op Str = new(OpCode.Str);
    public static readonly Op Get = new(OpCode.Get);
    public static readonly Op Set = new(OpCode.Set);
    public static readonly Op This = new(OpCode.This);
    public static readonly Op Unbox = new(OpCode.Unbox);
    public static readonly Op Type = new(OpCode.Type);
    public static readonly Op Pop = new(OpCode.Pop);
    public static readonly Op PushNil = new(OpCode.PushNil);
    public static readonly Op PushI1_0 = new(OpCode.PushI1_0);
    public static readonly Op PushI1_1 = new(OpCode.PushI1_1);
    public static readonly Op PushI8_0 = new(OpCode.PushI8_0);
    public static readonly Op PushI8_1 = new(OpCode.PushI8_1);
    public static readonly Op PushR8_0 = new(OpCode.PushR8_0);
    public static readonly Op PushR8_1 = new(OpCode.PushR8_1);
    public static readonly Op Shl = new(OpCode.Shl);
    public static readonly Op Shr = new(OpCode.Shr);
    public static readonly Op And = new(OpCode.And);
    public static readonly Op Or = new(OpCode.Or);
    public static readonly Op Xor = new(OpCode.Xor);
    public static readonly Op Add = new(OpCode.Add);
    public static readonly Op Sub = new(OpCode.Sub);
    public static readonly Op Mul = new(OpCode.Mul);
    public static readonly Op Div = new(OpCode.Div);
    public static readonly Op Rem = new(OpCode.Rem);
    public static readonly Op Neg = new(OpCode.Neg);
    public static readonly Op Plus = new(OpCode.Plus);
    public static readonly Op Not = new(OpCode.Not);
    public static readonly Op BitNot = new(OpCode.BitNot);
    public static readonly Op Len = new(OpCode.Len);
    public static readonly Op Gt = new(OpCode.Gt);
    public static readonly Op Lt = new(OpCode.Lt);
    public static readonly Op Eq = new(OpCode.Eq);
    public static readonly Op NotEq = new(OpCode.NotEq);
    public static readonly Op GtEq = new(OpCode.GtEq);
    public static readonly Op LtEq = new(OpCode.LtEq);
    public static readonly Op Ret = new(OpCode.Ret);
    public static readonly Op Dup = new(OpCode.Dup);
    public static readonly Op Fail = new(OpCode.Fail);
    public static readonly Op Term = new(OpCode.Term);
    public static readonly Op Yield = new(OpCode.Yield);
    public static readonly Op PushNilT = new(OpCode.PushNilT);
    public static readonly Op End = new(OpCode.End);
    public static readonly Op IsNull = new(OpCode.IsNull);
    public static readonly Op GetIter = new(OpCode.GetIter);
    public static readonly Op Mut = new(OpCode.Mut);
    public static readonly Op Annot = new(OpCode.Annot);
    public static readonly Op TypeCheck = new(OpCode.TypeCheck);
    public static readonly Op NewCast = new(OpCode.NewCast);
    public static readonly Op Cast = new(OpCode.Cast);
    public static readonly Op Mixin = new(OpCode.Mixin);
    public static readonly Op StdCall_0 = new(OpCode.StdCall_0);
    public static readonly Op StdCall_1 = new(OpCode.StdCall_1);
    public static readonly Op Debug = new(OpCode.Debug);

    internal static readonly Dictionary<OpCode, Op> Ops = new();

    public readonly OpCode Code;
    
    public int Data;

    static Op()
    {
        foreach (var fi in typeof(Op).GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            var o = (Op)fi.GetValue(null)!;
            Ops.Add(o.Code, o);
        }
    }

    public Op(OpCode code) => Code = code;

    public Op(OpCode code, int data) => (Code, Data) = (code, data);

    public override string ToString() => Code.ToString();
}
