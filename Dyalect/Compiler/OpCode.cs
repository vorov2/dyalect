namespace Dyalect.Compiler
{
    public enum OpCode
    {
        Nop = 0,    //0

        Str,        //0
        This,       //+1
        Pop,        //-1
        PushNil,    //+1
        PushI1_0,   //+1
        PushI1_1,   //+1
        PushI8,     //+1
        PushI8_0,   //+1
        PushI8_1,   //+1
        PushR8,     //+1
        PushR8_0,   //+1
        PushR8_1,   //+1
        PushStr,    //+1
        PushCh,     //+1
        Br,         //0
        Brtrue,     //-1
        Brfalse,    //-1
        Shl,        //-1
        Shr,        //-1
        And,        //-1
        Or,         //-1
        Xor,        //-1
        Add,        //-1
        Sub,        //-1
        Mul,        //-1
        Div,        //-1
        Rem,        //-1
        Neg,        //0
        Plus,       //0
        Not,        //0
        BitNot,     //0
        Len,        //0
        Gt,         //-1
        Lt,         //-1
        Eq,         //-1
        NotEq,      //-1
        GtEq,       //-1
        LtEq,       //-1
        Pushloc,    //1
        Pushvar,    //1
        Pushext,    //1
        Poploc,     //-1
        Popvar,     //-1
        Ret,        //0
        Dup,        //1
        SyncPoint,  //0
        Fail,       //-1
        FailSys,    //0
        NewFun,     //0
        NewFunV,    //0
        NewIter,    //0
        SetMember,  //-2
        SetMemberT, //-2
        GetMember,  //-1
        HasMember,  //-1
        SetMemberS, //-2
        SetMemberST,//-2
        Get,        //-1
        Set,        //-3
        RunMod,     //+1
        Type,       //0
        Tag,        //0
        Term,       //0
        Yield,      //-1
        PushNilT,   //1
        Brterm,     //0
        Briter,     //0
        Aux,        //0

        FunPrep,    //0
        FunArgIx,   //-1
        FunArgNm,   //-1
        FunCall,    //0
        NewTuple,   //Dynamic
        GetIx,      //0
        SetIx,      //-2
        HasField,   //0
        TypeCheck,  //0
        TypeCheckT, //0

        Start,      //0
        End,        //0
        NewType,    //0

        TypeS,      //1
        TypeST,     //1
        CtorCheck,  //0
        Unbox,      //1
        Rethrow,    //0
        CloseSect,  //0
        ChNoInit    //0
    }
}
