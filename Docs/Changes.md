# 0.3.0
 * A new guard expressions are added, e.g.
    `res = x if cond else y`, which is equivalent to 
    `res = if cond x else y`.
 * A new syntax added for expression bodied functions, e.g.
    `func A(x, y) x + y` instead of `func A(x, y) { x + y }`.
 * An unary plus `+` operator is added (for built-in types it
    is an identity function, but can be overriden).

# 0.2.1
 * A bug fixed in parser that didn't allow to use expression
    in indexers (e.g. `arr[x - y]`).
 * A bug fixed in tuple initialization logic (reproducible
    with pairs, e.g. `(2, 4)`).

# 0.2.0
 * Added support for special `iterator` function which can be
    implemented for any type. This function is used to iterate
    through containers. It should another function (a closure)
    which in turn should iterate over a collection by yielding
    a tuple in a form `(bool, value)`, where `bool` if a
    boolean flag which determines where a function has
    returned something and `value` is an returned element 
    (if any). This is pretty similar to `IEnumerator` from
    .NET but uses a single closure instead of an interface
    with two methods.
 * Added a new type `Iterator` (which is actually a special
    kind of function) which allows to implement non-strict
    functions in a manner of LINQ.
 * A `for` cycle (based on the iterator functionality) is
    implemented, e.g.: `for x in seq { doSomething(x) }`.
 * Arrays now supports methods `add`, `remove`, `removeAt`
    and `addRange`. The latter one accepts an iterator, e.g.
    `var arr = []; arr.addRange("Hello, world!")`.
 * Multiple refactorings and optimizations in the function
    invocation code.
 * A bug fixed in standard `toString` function implementation
    that could cause virtual machine to crash.
 * A bug fixes with special `self` variable (available in
    member functions) being incorrectly interpreted by
    nested functions.
 * Fixed compilation logic for `while` loops (previously they
    didn't create a lexical scope).
 * Bug fixes - interactive mode not restoring properly after
    compilation errors.
 * Fixes in grammar and parser.
 * Fixes in interactive console exception handling not always
    working correctly.
 * Empty blocks `{ }` are now allowed.

# 0.1.0
Initial release