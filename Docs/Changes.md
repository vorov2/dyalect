# 0.5.4
  * A bug fixed: _Multiline mode works incorrectly in console_ ([Issue #70](https://github.com/vorov2/dyalect/issues/70)).

# 0.5.3
  * A bug fixed: _Previous errors are not always cleared in interactive console_ ([Issue #64](https://github.com/vorov2/dyalect/issues/64)).
  * Type system is refactored for better consistency.

# 0.5.2
  * A feature request implemented: _Unit testing in Dya console_ ([Issue #6](https://github.com/vorov2/dyalect/issues/6)).

# 0.5.1
  * A bug fixed: _No default values for vararg parameter_ ([Issue #65](https://github.com/vorov2/dyalect/issues/65)).
  * A bug fixed: _Variadic functions behavior_ ([Issue #63](https://github.com/vorov2/dyalect/issues/63)).

# 0.5.0
  * Now it is possible to pass function arguments by name, e.g.:
    ```swift
    func sum(x, y, z) {
    }

    sum(z: 3, x: 1, y: 2)
    ```
    Related issue: [#37](https://github.com/vorov2/dyalect/issues/37).
  * Now it is possible to set a default value for a function argument:
    ```swift
    func sum(x, y, z = 3) {
    }
    sum(y: 2, x: 1)
    ```
    A default value is stored in metadata and can be of type `Integer`, `Float`, `Char`, `String`, `Boolean` or `Nil`. Related issue: [#37](https://github.com/vorov2/dyalect/issues/37).
  * Implementation of tuples is heavily refactored and optimized.
  * Now numeric literals can be negative, e.g. `-42`. Previously this would be always interpreted as `negation` operation.
  * Bug fixed: _Crush because of a debugger_ ([Issue #46](https://github.com/vorov2/dyalect/issues/46)).
  * Bug fixed: _Incorrect type name for Char_ ([Issue #53](https://github.com/vorov2/dyalect/issues/53)).
  * Arrays are redesigned for better effeciency and flexibility.
  * Feature implemented: _Redesign array creation_ ([Issue #47](https://github.com/vorov2/dyalect/issues/47)).
  * Feature implemented: _Construct a empty array filled with values_ ([Issue #30](https://github.com/vorov2/dyalect/issues/30)). A new static method `empty` with the following signature is added to an `Array` type:
    ```swift
    static func Array.empty(size, default = nil) { ... }
    //Usage:
    Array.empty(3) //Result is [nil, nil, nil]
    Array.empty(5, 0) //Result is [0, 0, 0, 0, 0]
    ```
  * Now ranges are supported ([Issue #8](https://github.com/vorov2/dyalect/issues/8)). Support for ranges is implemented through the `to` method which has the following signature:
    ```swift
    func TypeName.to(value) { ... }
    ```
    This method returns an iterator and is currently implemented by data types `Integer`, `Float` and `Char`. Additionally a special syntax for ranges is now supported:
    ```swift
    var range = 10..1 //same as 10.to(1)
    var alphaRange = 'a'..'z' //same as 'a'.to('z')
    ```
  * A static method `new` with a single `values` parameter is added to an `Array` type. Now array literal is translated to a call of this method, e.g.:
    ```swift
    [1, 2, 3] //same as Array.new(1, 2, 3)
    ```
  * Method `sortBy` is decomissioned. Now a single `sort` method should be used in all the cases. This method now has the following signature:
    ```swift
    func sort(comparator = nil) { }
    ```
    If an argument is not provided than a default comparator is used.
  * Bug fixed: _Incorrect boolean comparisons_ ([Issue #58](https://github.com/vorov2/dyalect/issues/58)).
  * Now Dy supports string interpolation ([Issue #3](https://github.com/vorov2/dyalect/issues/3)). String interpolation is implemented similar to Swift:
    ```swift
    var x = 42
    var y = 12
    "the value of (x + y) is \(x + y)" //Evaluates to "the value of (x + y) is 54"
    ```

# 0.4.3
  * A bug fixed: Method overloading and concatenation ([Issue #44](https://github.com/vorov2/dyalect/issues/44)).

# 0.4.2
  * Now strings and characters support `\s` escape code (inserts a space, `\u0020` character).
  * Fixed a number comparisons bug ([Issue #41](https://github.com/vorov2/dyalect/issues/41)).

# 0.4.1
  * Now parser generates better error messages for the "unrecognized escape sequence" case.
  * A standard `assert` function is added. This function accepts two values and raises and exception if these values are not equal.

# 0.4.0
  * Now it is possible to access named values in containers (e.g. named items in tuples), using regular member access syntax:
    ```swift
    var tup = (x: 12, y: 14)
    var x = tup.x //read a value from x
    tup.y = 114 //write a value to y
    ``` 
  * A method `slice` added to the type `Array` ([Issue #19](https://github.com/vorov2/dyalect/issues/19)). This method returns a slice of an array based on the provided indices:
    ```swift
    var arr = [1,2,3,4,5,6]
    arr.slice(2,5) //returns [3,4,5]
    ```
  * Methods `sort` and `sortBy` are added to the type `Array` ([Issue #14](https://github.com/vorov2/dyalect/issues/14)). These method allows to sort array in place with the help of a provided comparator:
    ```swift
    var arr = [4,6,1,3,2,5]
    arr.sortBy((x,y) => x - y)
    arr //arr is [1,2,3,4,5,6]
    ```
    Method `sort` uses a default comparer (it assumes that types implement operators `==`, `!=`, `>` and `<`).
  * A method `compact` added to the type `Array` (removes all occurences of `nil` in place).
  * Methods `fst` and `snd` (that return first and second elements) are added to the type `Tuple`.
  * Methods `contains`, `indexOf` and `lastIndexOf` are added to the type `String` (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)).
  * A method `split` is added to the type `String` (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20))- it converts a string into an array of strings based on the provided separator(s):
    ```swift    
    var str = "Name=John;Surname=Doe"
    var arr = str.split("=", ";")
    arr //arr is ["Name", "John", "Surname", "Doe"]
    ```
  * Methods `upper` and `lower` are added to the `String` data type (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)), these methods trasforms a string into lower or upper case.
  * Methods `startsWith` and `endsWith` are added to the `String` data type (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)).
  * A method `sub` is added to the type `String` (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)), this method returns a substring of a string:
    ```swift
    "abcdef".sub(2, 4) //returns "cdef"
    "qwerty".sub(4) //returns "ty"
    ```
  * A method `capitalize` that converts a first letter to a capital letter is added to `String` data type (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)).
  * Methods `trim`, `trimStart` and `trimEnd` are added to the `String` data type (related to [Issue #20](https://github.com/vorov2/dyalect/issues/20)):
    ```swift
    " str ".trim() //returns "str"
    "!!str!!".trim('!') //returns "str"
    ```
  * **(Breaking change)** A new `Char` data type is added ([Issue #12](https://github.com/vorov2/dyalect/issues/12)). Now double quoted literals create strings, while single quoted literals create chars (previously both would result in a string):
    ```swift
    var str = "Foo" //A string
    var ch = 'F' //A char
    ```
    Chars support escape codes in a manner similar to strings. Also chars support method `toString`, comparison operators, equality operations and addition operator (the result of addition of two chars is a string).
  * **(Breaking change)** Now string iterator returns a sequence of chars (instead of sequence of strings).
  * A _method presence check_ operator `?` is added ([Issue #25](https://github.com/vorov2/dyalect/issues/25)). Now it is possible to check whether a value has a certain method:
    ```swift
    42.toString? //evaluates to true
    42.fooBar? //evaluates to false
    ```
  * A bug fixed - incorrect type info was generated for the values of type `String`.
  * Grammar for the `base` keyword is corrected, now compiler better handles the cases where `base` is not allowed (but it is still possible to report about that in a clear way).
  * Foreign modules now support regular value objects and not just functions.
  * A bug fixed (_Dy type system and embedding_, [Issue #33](https://github.com/vorov2/dyalect/issues/33)).
  * Implemented _Make types first class values_ ([Issue #31](https://github.com/vorov2/dyalect/issues/31)). Now all type names (e.g. `Integer`) are regular variables resolved to a `TypeInfo` instance that describes a type.
  * Now it is possible to create static methods that can be called against types, not values ([Issue #27](https://github.com/vorov2/dyalect/issues/27)), e.g.:
    ```swift
    String.concat("one", "two", "three") //static method, String is a type
    "foo".len() //instance method, "foo" is an instance
    ```
  * A new static method `concat` is added to a `String` data type ([Issue #29](https://github.com/vorov2/dyalect/issues/29)).

# 0.3.4
  * A bug fixed - incorrect type info was generated for the values of type `Array`.
  * Runtime type system is refactored.
  * Optimizations in members functions execution.
  * Now module import directives are always compiled before the rest of the code.

# 0.3.3
  * Logic, responsible for the implementation of function calls, is largely rewritten and optimized.
  * Internal call stack structure is changed for better effeciency.

# 0.3.2
  * Dyalect byte code simplified (removed redundant instructions).
  * Removed redundant code from parser.

# 0.3.1
  * Code refactoring in virtual machine ([Issue #2](https://github.com/vorov2/dyalect/issues/2)).
  * Tuples now support both read and write operations on their fields.
  * Several runtime error messages corrected.

# 0.3.0
  * An unary plus `+` operator is added (for built-in types it is an identity function, but can be overriden).
  * **(Experimental)** A support for implicit anonymous function declaration is added. For short functions one can use the following notation - instead of declaring a full lambda, e.g. `x => x * 2` one can write `$0 * 2`. A `$` prefix instructs the compiler that the whole expression is a function, and all the dollar names are automatically promoted as function arguments in the appropriate order, e.g.: `$1 - $0` is equivalent to `(x,y) => y - x`. Example:
    ```swift
    var arr = [1, 2, 3, 4, 5, 6]
    var searchResult = arr.findBy($0 % 2 == 0)
    ```
 * Optimizations in implementation of iterators.
 * Refactoring in implementation of functions, including the marshalling between Dy's functions and foreign functions.
 * A bug fixed in array indexing function that could cause virtual machine to crash.
 * Added missing message string for the `IndexOutOfRange` runtime error messge.
 * A new `insert` function is added to the array prototype. This function accepts an index and a value and inserts a value at a given index. Only indices of type `Integer` are supported. If index is out of range an `IndexOutOfRange` exception is thrown.
 * Functions `indexOf` (accepts a value and returns an index of first occurence or `-1`) and `lastIndexOf` (retuns an index of last occurence) are added to the array prototype.
 * Support for variadic functions is added (functions that can accept an unlimited number of arguments). The implementation of such functions is similar to C#:
    ```swift
    func format(formatString, args...) {
        //Implementation
    }
    //All of these are valid calls:
    format("foo")
    format("foo {0}", 2)
    format("foo {0} {1}", 2, 4)
    ```
    If the last argument ends with three dots `...` it would be initialized as tuple and receive all the other passed values.
 * **(Breaking change)** Now functions would raise exception when called with inexact number of arguments (except for variable functions).
 * Interactive console now supports multiline mode - simply put a trailing bracket `{` at the end of a line. Interactive would than balance the brackets and execute code only when all brackets are matched.
 * A bug fixed in `TypeInfo` `toString` function.
 * Fixes, refactorings and multiple enhancements in interactive console. The following commands and switches are added: command `:options` that prints out current console options,  command `:dump` that prints out all global variables with their type and values, command `eval` that evaluates a given file in the current interactive session, support for command line switch `-i` that keeps console in interactive mode after evaluating a file. Also an ability to set colors (along with `-nocolor` switch) is temporary removed.
 * **(Breaking change)** Operator "get length" `#` is decomissioned, now you should use a member function `len` instead.
 * **(Breaking change)** Standard member function `iterator` is renamed to `iter`.
 * Methods `indices` that returns an iterator with indices is added to array and tuple.
 * Methods `keys` and `values` (return an iterator with keys if any and with all the values) are added to tuple.
 * Method `toArray` is added to iterators (converts a given iterator to array).
 * A support for special `base` keyword. You can obtain a value of a variable from the enclosing function (or a global scope) by referencing it through `base`, e.g.:
    ```swift
    var x = 12
    func checkIt(x) {
        print(base.x)
    }
    checkIt(42) //Would print 12
    ```
 * A bug fixed - an error in execution context wasn't cleared after generating an exception (and therefore could be falsely raised one more time if instance of VM is cached).

# 0.2.3
 * A bug fixed in parser - previously tuples may not be 
    parsed correctly and could cause parser to fail for
    the tuple with correct syntax.

# 0.2.2
 * Code clean-ups
 * A new constructor is added to tuple type for convinience.
 * Some strings (related to error messages) are translated into english.

# 0.2.1
 * A bug fixed in parser that didn't allow to use expression in indexers (e.g. `arr[x - y]`).
 * A bug fixed in tuple initialization logic (reproducible with pairs, e.g. `(2, 4)`).

# 0.2.0
 * Added support for special `iterator` function which can be implemented for any type. This function is used to iterate through containers. It returns another function (a closure) which iterates over a collection by yielding values. This is pretty similar to `IEnumerator` from .NET but uses a single closure instead of an interface with two methods. In order to support this infrastructure a new `Iterator` type is added as well (which is actually a special kind of function).
 * All foreign objects can now automatically support Dy's iterators as long as they implement `IEnumerable<DyObject>` interface.
 * A `for` cycle (based on the iterator functionality) is implemented, e.g.: 
    ```swift
    for x in seq { 
        doSomething(x) 
    }
    ```
 * Arrays now supports methods `add`, `remove`, `removeAt` and `addRange`. The latter one accepts an iterator, e.g.:
    ```swift
    var arr = []
    arr.addRange("Hello, world!")
    ```
 * Multiple refactorings and optimizations in the function invocation code.
 * A bug fixed in standard `toString` function implementation that could cause virtual machine to crash.
 * A bug fixes with special `self` variable (available in member functions) being incorrectly interpreted by nested functions.
 * Fixed compilation logic for `while` loops (previously they didn't create a lexical scope).
 * Bug fixes - interactive mode not restoring properly after compilation errors.
 * Fixes in grammar and parser.
 * Fixes in interactive console exception handling not always working correctly.
 * Empty blocks `{ }` are now allowed.

# 0.1.0
Initial release