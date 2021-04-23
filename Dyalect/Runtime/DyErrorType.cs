namespace Dyalect.Runtime
{
    public enum DyErrorCode
    {
        None,

        UserCode = 601,

        //= 602,

        ExternalFunctionFailure = 603,

        OperationNotSupported = 604,

        IndexOutOfRange = 605,

        IndexInvalidType = 606,

        DivideByZero = 607,

        TooManyArguments = 608,

        InvalidType = 609,

        StaticOperationNotSupported = 610,

        AssertFailed = 611,

        RequiredArgumentMissing = 612,

        ArgumentNotFound = 613,

        FailedReadLiteral = 614,

        MatchFailed = 615,

        CollectionModified = 616,

        PrivateNameAccess = 617,

        KeyNotFound = 618,

        KeyAlreadyPresent = 619,

        InvalidRange = 620,

        InvalidValue = 621,

        ValueOutOfRange = 622,

        OpenRangeNotSupported = 623
    }
}
