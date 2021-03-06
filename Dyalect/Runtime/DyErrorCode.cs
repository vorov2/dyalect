﻿namespace Dyalect.Runtime
{
    public enum DyErrorCode
    {
        None,

        UnexpectedError = 601,

        MultipleValuesForArgument = 602,

        ExternalFunctionFailure = 603,

        OperationNotSupported = 604,

        IndexOutOfRange = 605,

        FieldReadOnly = 606,

        DivideByZero = 607,

        TooManyArguments = 608,

        InvalidType = 609,

        PrivateAccess = 610,

        AssertFailed = 611,

        RequiredArgumentMissing = 612,

        ArgumentNotFound = 613,

        FormatException = 614,

        MatchFailed = 615,

        CollectionModified = 616,

        PrivateNameAccess = 617,

        KeyNotFound = 618,

        KeyAlreadyPresent = 619,

        FieldNotFound = 620,

        InvalidValue = 621
    }
}
