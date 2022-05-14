namespace Dyalect.Runtime;

public enum DyError
{
    None,

    UnexpectedError = 601,

    MultipleValuesForArgument = 602,

    ExternalFunctionFailure = 603,

    OperationNotSupported = 604,

    IndexOutOfRange = 605,

    IndexReadOnly = 606,

    DivideByZero = 607,

    TooManyArguments = 608,

    InvalidType = 609,

    PrivateAccess = 610,

    AssertionFailed = 611,

    RequiredArgumentMissing = 612,

    ArgumentNotFound = 613,

    InvalidOperation = 614,

    MatchFailed = 615,

    CollectionModified = 616,

    PrivateNameAccess = 617,

    KeyNotFound = 618,

    KeyAlreadyPresent = 619,

    InvalidOverload = 620,

    InvalidValue = 621,

    InvalidCast = 622,

    ValueMissing = 623,

    Failure = 624,

    Timeout = 625,

    ParsingFailed = 626,

    Overflow = 627,

    MethodNotFound = 628,

    TypeClosed = 629,

    NotImplemented = 630,

    ConstructorFailed = 631,

    IOFailed = 632,

    OverloadProhibited = 633
}
