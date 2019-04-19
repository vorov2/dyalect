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
 * Empty blocks `{ }` are now allowed.

# 0.1.0
Initial release