* [Types](#types)
    * [StringBuilder](#sb)

<a name="types"></a>
## Types

### StringBuilder

Regular strings in Dyalect are immutable, `StringBuilder` is an implementation of a mutable string which can be used for effective string manipulations. The type offers the following instance members:

```swift
Append(value)
```

Converts a given `value` to a string and appends it to the end of a constructed string. Returns the same instance of `StringBuilder` that was used to invoke this method.

```swift
AppendLine(value)
```

The same as `append`, but adds a new line character to the end of the constructed string.

```swift
Remove(index, len)
```
Removes a given number of characters (`len`) starting from `index`. Returns the same instance of `StringBuilder` that was used to invoke this method.

```swift
Replace(old, new)
```

Replaces an `old` substring with a `new` one. Returns the same instance of `StringBuilder` that was used to invoke this method.

```swift
Insert(value, index)
```

Converts a given `value` to a string and inserts it at a given `index`. Returns the same instance of `StringBuilder` that was used to invoke this method.

Additionally `StringBuilder` type implements the following constructors:

```swift
StringBuilder(values = nil)
```

Constructs an instance of a type with a optional sequence of `values` (which if provided are converted to strings and concatenated to a solid string).