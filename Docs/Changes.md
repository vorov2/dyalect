# 0.3.0
 * A new guard expressions are added, e.g.
    `res = x if cond else y`, which is equivalent to 
    `res = if cond x else y`.
 * A new syntax added for expression bodied functions, e.g.
    `func A(x, y) x + y` instead of `func A(x, y) { x + y }`.
 * An unary plus `+` operator is added (for built-in types it
    is an identity function, but can be overriden).
 * Syntax for bitwise operator is changed from `|`, `&`, `^`,
    `<<` and `>>` to `|||`, `&&&`, `^^^`, `<<<` and `>>>`.
 * (Experimental) A support for guards is added to statements
    `return`, `break`, `continue` and `yield`. Now instead of
    writing e.g. `if n < 2 { return n }` one can write
    `return n when n < 2` (the two statements are complete
    equivalents).
 * (Experimental) A support for implicit anonymous function
    declaration is added. For short functions one can use the
    following notation - instead of declaring a full lambda,
    e.g. `x => x * 2` one can write `$0 * 2`. A `$` prefix
    instructs the compiler that the whole expression is a
    function, and all the dollar names are automatically
    promoted as function arguments in the appropriate order,
    e.g. `$1 if $0 < 3 else $0 * $x` is equivalent to:
    `(i,x) => if (i < 3) { x } else { i * x }`.

# 0.2.2
 * Code clean-ups
 * A new constructor is added to tuple type for convinience.
 * Some strings (related to error messages) are translated
    into english.

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