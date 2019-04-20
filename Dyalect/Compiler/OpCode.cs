namespace Dyalect.Compiler
{
    public enum OpCode
    {
        Nop = 0,    //0

        Str,        //0
        Self,       //+1
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
        PushStr,    //+1
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
        Call,       //Dynamic
        NewFun,     //0
        NewFunV,    //0
        TraitS,     //-2
        TraitG,     //-1
        Get,        //-1
        Get0,       //0
        Get1,       //0
        Set,        //-3
        RunMod,     //+1
        Type,       //0
        Tag,        //0
        Term,       //0
        Yield,      //-1
        PushNilT,   //1
        Brterm,     //0
        Briter      //0
    }
}
