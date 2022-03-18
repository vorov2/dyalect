* [Overview](#overview)
* [Operators](#operators)
* [Fields](#fields) 
* [Methods](#methods) 
* [Constructors](#cons)

<a name="overview"></a>
# Overview

`Result` data type is a algebraic type that can be used to return values from functions when a result of a function execution can be either a success or a failure. It is an elegant replacement for `nil` in such cases:

```swift
func safeDiv(x, y) {
    return Result.Failure("Divide by zero.") when y is 0
    Result.Success(x / y)
}

match safeDiv(12, 0) {
    Result.Success(x) => { /* Do something with x */ },
    Result.Failure(str) => print("We have failed!", str)
}
```

`Result` type supports indexing and `Length` method, therefore pattern matching can be simplified:

```swift
if safeDiv(12, 0) is (x,) {
    //Do something with x
}
```

<a name="operators"></a>
# Operators

`Result` support the following standard operators:
* `==` (equals)
* `!=` (not equals)
* `!` (not): always returns `true`
* `[]` (indexing): indexing operator can be used obtain a value of `value` field which always have index `0`, if an instance of a type is a [Failure](#failure) indexing would result in a `IndexOutOfRange` exception

<a name="#fields"></a>
# Fields

```swift
value
```

Returns a value provided to [Success](#success) constructor if an instance is a `Success`, otherwise raises `IndexOutOfRange` exception.

```swift
detail
```

Returns a value provided to [Failure](#failure) constructor if an instance is a `Failure`, otherwise raises `IndexOutOfRange` exception.

<a name="#methods"></a>
# Methods

<a name="len"></a>
## Length

```swift
Length()
```

Returns `1` if this instance is a [Success](#success), otherwise - `0`.

<a name="value"></a>
## Value

```swift
Value()
```

Tries to return a value, provided to [Success](#success) constructor. This method can raise an exception if an instance of this type is a [Failure](#failure).

<a name="cons"></a>
# Constructors

<a name="success"></a>
## Success

```swift
Success(value)
```

Creates a "successful" instance of this type.

<a name="failure"></a>
## Failure

```swift
Failure(detail)
```

Create a "failure". A `detail` parameter should normally include failure explanation.