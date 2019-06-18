namespace Dyalect.Compiler
{
    internal static class OpStackHelper
    {
        internal static int[] Op =
        {
            0,  //Nop
            0,  //Str
            1,  //This
            -1, //Pop
            1,  //PushNil
            1,  //PushI1_0
            1,  //PushI1_1
            1,  //PushI8
            1,  //PushI8_0
            1,  //PushI8_1
            1,  //PushR8
            1,  //PushR8_0
            1,  //PushStr
            1,  //PushCh
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
            0,  //SyncPoint
            -1, //Fail
            0,  //FailSys
            1,  //NewFun
            1,  //NewFunV
            1,  //NewFunA
            1,  //NewIter
            -1, //SetMember
            -1, //SetMemberT
            0,  //GetMember
            0,  //HasMember
            -1, //SetMemberS
            -1, //SetMemberST
            -1, //Get
            -3, //Set
            1,  //RunMod
            0,  //Type
            0,  //Tag
            0,  //Term
            -1, //Yield
            1,  //PushNilT
            0,  //Brterm
            0,  //Briter
            0,  //Aux

            0,  //FunPrep
            -1, //FunArgIx
            -1, //FunArgNm
            0,  //FunCall

            0,  //NewTuple **dynamic
            0,  //GetIx
            -2, //SetIx
            0,  //HasField
            0,  //TypeCheck
            0,  //TypeCheckT
            0,  //Start
            0,  //End
            0,  //NewType
            1,  //TypeS
            1,  //TypeST
            0,  //CtorCheck
        };
    }
}
