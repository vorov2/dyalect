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
 * Interactive console now supports multiline mode - simply end an input with an open curly bracket.

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