* [Overview](#overview)
* [Operators](#operators)
* [Fields](#fields) 
* [Methods](#methods) 
* [Constructors](#cons)

<a name="overview"></a>
# Overview

`Guid` data type is a global unique identifier based on .NET implementation `System.Guid`.

<a name="operators"></a>
# Operators

`Result` support the following standard operators:
* `==` (equals)
* `!=` (not equals)
* `!` (not): always returns `true`

<a name="#methods"></a>
# Methods

<a name="toByteArray"></a>
## ToByteArray

```swift
ToByteArray()
```

Converts an instance of `Guid` to a [ByteArray](Library.Core.ByteArray).

<a name="statics"></a>
# Static methods

<a name="empty"></a>
## Empty

```swift
Guid.Empty()
```

Returns an empty `Guid`.

<a name="default"></a>
## Default

```swift
Guid.Default()
```

Same as [Empty](#empty).

<a name="parse"></a>
## Parse

```swift
Guid.Parse(value)
```

Parses `Guid` from a string.

<a name="fromByteArray"></a>
## FromByteArray

```swift
Guid.FromByteArray(value)
```

Create a `Guid` from an instance of [ByteArray][Library.Core.ByteArray].

<a name="cons"></a>
# Constructors

<a name="guid"></a>
## Guid

```swift
Guid() //or Guid.Guid()
```

Generates a new unique identifier.