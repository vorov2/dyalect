* [Overview](#overview)
* [Operators](#operators)
* [Methods](#methods) 
* [Constructors](#cons)

<a name="overview"></a>

# Overview

Regular strings in Dy are immutable, `StringBuilder` is an implementation of a mutable string which can be used for effective string manipulations.

<a name="operators"></a>
# Operators

`StringBuilder` support the following standard operators:
* `==` (equals)
* `!=` (not equals)
* `!` (not): always returns `false`

<a name="#methods"></a>
# Methods

<a name="append"></a>
## Append

```swift
Append(value)
```

Converts a given `value` to a string and appends it to the end of a constructed string. Returns the same instance of `StringBuilder` that was used to invoke this method.

<a name="appendLine"></a>
## AppendLine

```swift
AppendLine(value = "")
```

The same as `Append`, but adds a new line character to the end of the constructed string. If a `value` parameter is not provided this method simply appends a new line character.

<a name="remove"></a>
## Remove

```swift
Remove(index, len)
```

Removes a given number of characters (`len`) starting from `index`. Returns the same instance of `StringBuilder` that was used to invoke this method.

<a name="replace"></a>
## Replace

```swift
Replace(old, new)
```

Replaces an `old` substring with a `new` one. Returns the same instance of `StringBuilder` that was used to invoke this method.

<a name="insert"></a>
## Insert

```swift
Insert(value, index)
```

Converts a given `value` to a string and inserts it at a given `index`. Returns the same instance of `StringBuilder` that was used to invoke this method.

<a name="cons"></a>
# Construction

```swift
StringBuilder(values...)
```

Constructs an instance of a type with a sequence of `values` (values are converted to strings and concatenated to a solid string):

```swift
let sb1 = StringBuilder() //empty instance
print(sb1.ToString()) //Outputs an empty string
let sb2 = StringBuilder(1, 2, 3)
print(sb2.ToString()) //Outputs a string: "123"
```