﻿namespace Dyalect.Compiler;

internal static class OpStackHelper
{
    public static int[] Op =
    {
        0,  //Nop
        0,  //Str
        1,  //This
        -1, //Pop
        1,  //PushNil
        1,  //PushI1_0
        1,  //PushI1_1
        1,  //PushI8_0
        1,  //PushI8_1
        1,  //PushR8_0
        1,  //PushR8_1
        1,  //PushObj
        0,  //Br
        -1, //Brtrue
        -1, //Brfalse
        -1, //Shl
        -1, //Shr
        -1, //And
        -1, //Or
        -1, //Xor
        -1, //Add
        -1, //Sub
        -1, //Mul
        -1, //Div
        -1, //Rem
        0,  //Neg
        0,  //Plus
        0,  //Not
        0,  //BitNot
        0,  //Len
        -1, //Gt
        -1, //Lt
        -1, //Eq
        -1, //NotEq
        -1, //GtEq
        -1, //LtEq
        1,  //Pushloc
        1,  //Pushvar
        1,  //Pushext
        -1, //Poploc
        -1, //Popvar
        -1, //Ret
        1,  //Dup
        0,  //Fail
        1,  //NewFun
        1,  //NewFunV
        1,  //NewIter
        -2, //SetMember
        0,  //GetMember
        0,  //HasMember
        -2, //SetMemberS
        -1, //Get
        -2, //Set
        1,  //RunMod
        1,  //Type
        0,  //Tag
        0,  //Term
        -1, //Yield
        1,  //PushNilT
        0,  //Brterm
        0,  //Briter
        0,  //RgDI
        0,  //RgFI
        0,  //FunPrep
        -1, //FunArgIx
        -1, //FunArgNm
        0,  //FunCall
        0,  //NewTuple **dynamic
        0,  //Contains
        -2, //TypeCheck
        0,  //Start
        0,  //End
        -2, //NewObj
        1,  //NewType
        0,  //CtorCheck
        0,  //IsNull
        0,  //GetIter
        0,  //Mut
        -1, //Annot
        0,  //FunAttr
        -3, //NewCast
        0,  //Cast
        -2, //Mixin
        0,  //StdCall_0
        -1, //StdCall_1
        0,  //StdCall **dynamic
        0,  //Debug
        0,  //NewArgs **dynamic
        0,  //NewDict **dynamic
        0,  //GetPriv
        -1, //SetPriv
    };
}
