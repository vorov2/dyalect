* [Overview](#overview)
* [Operators](#operators)
* [Methods](#methods) 
* [Static methods](#statics)
* [Constructors](#cons)
* [Utility objects](#types)

<a name="overview"></a>
# Overview

`Regex` type enables support for regular expressions. You can use this type to perform search and replace. The implementation is based on the .NET `System.Text.RegularExpression` engine.

<a name="operators"></a>
# Operators

`Regex` support the following standard operators:
* `==` (equals)
* `!=` (not equals)
* `!` (not): always returns `true`

<a name="#methods"></a>
# Methods

<a name="append"></a>
## Match

```swift
Match(input, index = 0, count = nil)
```

Searches the `input` string for the first occurrence of a regular expression, beginning at the specified starting position (`index`) and searching only the specified number of characters (`count`). If `count` is not specified than search is performed until the end of input string is met.

This method returns a [Match](#match) object.

<a name="matches"></a>
## Matches

```swift
Matches(input, index = 0)
```

Searches the specified input string for all occurrences of a regular expression, beginning at the specified starting position (`index`) in the string.

This method returns a [tuple](Tuple) with [Match](#match) objects.

<a name="replace"></a>
## Replace

```swift
Replace(value, other)
```

In a specified input string, replaces all strings that match a regular expression pattern with a specified replacement string. Returns an instance of a new string.

<a name="statics"></a>
# Static methods

<a name="replace"></a>
## Replace

```swift
Regex.Replace(pattern, value, other)
```

A static version of the [Replace](#replace) method.

<a name="cons"></a>
# Constructors

<a name="regex"></a>
## Regex

```swift
Regex(pattern)
```

Constructs an instance of a type with a specified regular expression pattern.

<a name="types"></a>
# Utility objects

<a name="capture"></a>
## Capture

This objects represents the results from a single successful subexpression capture and consists of the following fields:

```swift
index
```

The zero-based starting position in the original string where the captured substring is found.

```swift
length
```

The length of the captured substring.

```swift
value
```

The captured substring from the input string.

<a name="match"></a>
## Match 

This object represents the results from a single regular expression match and includes all the fields from [Capture](#capture) objects and the following fields:

```swift
name
```

The name of the capturing group represented by the current instance.

```swift
success
```

A value indicating whether the match is successful. This value is used when performing conversions of this type to a boolean, e.g. if `success == true` than the result is `true`, otherwise the result is `false`.

```swift
captures
```

A tuple with all the captures matched by the capturing group, in innermost-leftmost-first order. This tuple contains instances of [Capture](#capture) object.
