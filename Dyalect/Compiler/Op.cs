using System.Collections.Generic;
using System.Reflection;

namespace Dyalect.Compiler
{
    internal sealed class Op
    {
        public static readonly Op Nop = new Op(OpCode.Nop);
        public static readonly Op Str = new Op(OpCode.Str);
        public static readonly Op Get = new Op(OpCode.Get);
        public static readonly Op Set = new Op(OpCode.Set);
        public static readonly Op This = new Op(OpCode.This);
        public static readonly Op Unbox = new Op(OpCode.Unbox);
        public static readonly Op Type = new Op(OpCode.Type);
        public static readonly Op Pop = new Op(OpCode.Pop);
        public static readonly Op PushNil = new Op(OpCode.PushNil);
        public static readonly Op PushI1_0 = new Op(OpCode.PushI1_0);
        public static readonly Op PushI1_1 = new Op(OpCode.PushI1_1);
        public static readonly Op PushI8_0 = new Op(OpCode.PushI8_0);
        public static readonly Op PushI8_1 = new Op(OpCode.PushI8_1);
        public static readonly Op PushR8_0 = new Op(OpCode.PushR8_0);
        public static readonly Op PushR8_1 = new Op(OpCode.PushR8_1);
        public static readonly Op Shl = new Op(OpCode.Shl);
        public static readonly Op Shr = new Op(OpCode.Shr);
        public static readonly Op And = new Op(OpCode.And);
        public static readonly Op Or = new Op(OpCode.Or);
        public static readonly Op Xor = new Op(OpCode.Xor);
        public static readonly Op Add = new Op(OpCode.Add);
        public static readonly Op Sub = new Op(OpCode.Sub);
        public static readonly Op Mul = new Op(OpCode.Mul);
        public static readonly Op Div = new Op(OpCode.Div);
        public static readonly Op Rem = new Op(OpCode.Rem);
        public static readonly Op Neg = new Op(OpCode.Neg);
        public static readonly Op Plus = new Op(OpCode.Plus);
        public static readonly Op Not = new Op(OpCode.Not);
        public static readonly Op BitNot = new Op(OpCode.BitNot);
        public static readonly Op Len = new Op(OpCode.Len);
        public static readonly Op Gt = new Op(OpCode.Gt);
        public static readonly Op Lt = new Op(OpCode.Lt);
        public static readonly Op Eq = new Op(OpCode.Eq);
        public static readonly Op NotEq = new Op(OpCode.NotEq);
        public static readonly Op GtEq = new Op(OpCode.GtEq);
        public static readonly Op LtEq = new Op(OpCode.LtEq);
        public static readonly Op Ret = new Op(OpCode.Ret);
        public static readonly Op Dup = new Op(OpCode.Dup);
        public static readonly Op SyncPoint = new Op(OpCode.SyncPoint);
        public static readonly Op Fail = new Op(OpCode.Fail);
        public static readonly Op Term = new Op(OpCode.Term);
        public static readonly Op Yield = new Op(OpCode.Yield);
        public static readonly Op PushNilT = new Op(OpCode.PushNilT);
        public static readonly Op End = new Op(OpCode.End);
        public static readonly Op Rethrow = new Op(OpCode.Rethrow);
        public static readonly Op CloseSect = new Op(OpCode.CloseSect);

        internal static readonly Dictionary<OpCode, Op> Ops = new Dictionary<OpCode, Op>();

        static Op()
        {
            foreach (var fi in typeof(Op).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var o = (Op)fi.GetValue(null);
                Ops.Add(o.Code, o);
            }
        }

        public Op(OpCode code)
        {
            Code = code;
        }

        public Op(OpCode code, int data)
        {
            Code = code;
            Data = data;
        }

        public readonly OpCode Code;
        public int Data;

        public override string ToString() => Code.ToString();
    }
}
