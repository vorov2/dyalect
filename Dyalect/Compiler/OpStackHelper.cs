namespace Dyalect.Compiler
{
    internal static class OpStackHelper
    {
        internal static int[] Op =
        {
            0,  //Nop
            1,  //Self
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
            0,  //Call **dynamic
            0,  //NewFun
            0,  //NewFunV
            -2, //Set
            0,  //TraitG
            1,  //RunMod
            0,  //Type
            0,  //Tag
            0,  //Term
        };
    }
}
