# 0.17.0
  * Change: _Code clean-ups and optimizations._
  * Add: _Add a generic `Object` type which can be used for interop_ ([Issue #359](https://github.com/vorov2/dyalect/issues/359)).
  * Add: _StringBuilder_ ([Issue #22](https://github.com/vorov2/dyalect/issues/22)).
  * Add: _Add automatic conversion to/from Dictionary_ ([Issue #360](https://github.com/vorov2/dyalect/issues/360)).
  * Add: _Implement module dependencies for foreign modules_ ([Issue #361](https://github.com/vorov2/dyalect/issues/361)).

# 0.16.0
  * Change: _All code migrated to .NET 5.0._
  * Change: _Serialization of maps to string is updated to match other types._
  * Change: _Signature for methods `Integer.to`, `Float.to` and `Char.to` is updated. Also these methods are renamed to `range`._
  * Add: _Add methods to `Iterator` type_ ([Issue #355](https://github.com/vorov2/dyalect/issues/355)).
  * Add: _Infinite ranges_ ([Issue #348](https://github.com/vorov2/dyalect/issues/348)).
  * Add: _Update methods `String.concat` and `String.join` to work with all sequences_ ([Issue #356](https://github.com/vorov2/dyalect/issues/356)).
  * Fixed: _Crush because of `??=` operator_ ([Issue #354](https://github.com/vorov2/dyalect/issues/354)).

# 0.15.1
  * Change: _Code clean-ups and optimizations._
  * Fixed: _Interactive mode crushes when trying to re-declare variable_ ([Issue #352](https://github.com/vorov2/dyalect/issues/352)).

# 0.15.0
  * Change: _[import] Change import syntax for foreign modules_ ([Issue #343](https://github.com/vorov2/dyalect/issues/343)).
  * Change: _[import] Import aliases_ ([Issue #345](https://github.com/vorov2/dyalect/issues/345)).
  * Change: _[import] Importing modules from standard library_ ([Issue #344](https://github.com/vorov2/dyalect/issues/344)).
  * Change: _Extend support for ranges to be able to specify a step_ ([Issue #346](https://github.com/vorov2/dyalect/issues/346)).
  * Add: _Add a support for exporting new data types from C# foreign modules_ ([Issue #339](https://github.com/vorov2/dyalect/issues/339)).
  * Fixed: _Ranges are not checked for consistency_ ([Issue #349](https://github.com/vorov2/dyalect/issues/349)).

# 0.14.1
  * Change: _Code clean-ups and optimizations._
  * Fixed: _Dyalect crushes for foreign functions without parameters_ ([Issue #340](https://github.com/vorov2/dyalect/issues/340)).

# 0.14.0
  * Change: _Refactorings and enhancements in linker foreign unit infrastructure._
  * Change: _General code clean-ups and optimizations._
  * Done: _Implement a foreign module interface with support for type conversion_ ([Issue #308](https://github.com/vorov2/dyalect/issues/308)).
  * Done: _Add a compound `??=` assignment operator_ ([Issue #306](https://github.com/vorov2/dyalect/issues/306), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#compound_assignment)).
  * Done: _Support an easier way to reference modules in standard library_ ([Issue #309](https://github.com/vorov2/dyalect/issues/309), [docs](https://github.com/vorov2/dyalect/wiki/Modules#foreign_modules)).
  * Done: _Make assignment a statement other than an expression_ ([Issue #311](https://github.com/vorov2/dyalect/issues/311), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#assignment)).
  * Done: _Change syntax for tuples and named arguments_ ([Issue #312](https://github.com/vorov2/dyalect/issues/312)).
  * Done: _Change field access syntax_ ([Issue #313](https://github.com/vorov2/dyalect/issues/313), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#access)).
  * Done: _String concatenation and `+` operator_ ([Issue #315](https://github.com/vorov2/dyalect/issues/315)).
  * Done: _Tuples: add a support for `+` operator_ ([Issue #316](https://github.com/vorov2/dyalect/issues/316), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#operators)).
  * Done: _Tuples: add `Concat` method_ ([Issue #317](https://github.com/vorov2/dyalect/issues/317), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#concat)).
  * Done: _Modules should use a field access syntax_ ([Issue #314](https://github.com/vorov2/dyalect/issues/314), [docs](https://github.com/vorov2/dyalect/wiki/Modules)).
  * Done: _Use backslash instead of dot for module namespacing_ ([Issue #321](https://github.com/vorov2/dyalect/issues/321), [docs](https://github.com/vorov2/dyalect/wiki/Modules)).
  * Done: _Redesign assignment operators and field access syntax_ ([Issue #305](https://github.com/vorov2/dyalect/issues/305)).
  * Done: _Allow modules to import fields as well as methods_ ([Issue #323](https://github.com/vorov2/dyalect/issues/323), [docs](https://github.com/vorov2/dyalect/wiki/Modules#export)).
  * Done: _Remove `auto` methods_ ([Issue #320](https://github.com/vorov2/dyalect/issues/320)).
  * Done: _Enhance parser error processing_ ([Issue #325](https://github.com/vorov2/dyalect/issues/325)).
  * Done: _Consider removing `private` modifier for functions_ ([Issue #324](https://github.com/vorov2/dyalect/issues/324)).
  * Done: _Static fields should be converted to static methods to maintain consistency_ ([Issue #318](https://github.com/vorov2/dyalect/issues/318)).
  * Done: _Add `min` and `max` built-in functions_ ([Issue #326](https://github.com/vorov2/dyalect/issues/326), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#min), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#max)).
  * Done: _Add `sign` and `sqrt` built-in functions_ ([Issue #327](https://github.com/vorov2/dyalect/issues/327), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#sign), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#sqrt)).
  * Fixed: _Parser may crush when processing `import`_ ([Issue #307](https://github.com/vorov2/dyalect/issues/307)).
  * Fixed: _Dyalect crushes when it cannot find a DLL of a foreign module_ ([Issue #310](https://github.com/vorov2/dyalect/issues/310)).

# 0.13.0
  * Change: _Collection object model refactoring._
  * Done: _A Dyalect Console option to show only failed tests_ ([Issue #299](https://github.com/vorov2/dyalect/issues/299)).
  * Done: _Arrays, tuples and strings: negative indices_ ([Issue #301](https://github.com/vorov2/dyalect/issues/301), [docs](https://github.com/vorov2/dyalect/wiki/Array#overview), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#overview), [docs](https://github.com/vorov2/dyalect/wiki/String#overview)).
  * Done: _Tuples: modification (add, insert, remove) methods_ ([Issue #292](https://github.com/vorov2/dyalect/issues/292), [docs](https://github.com/vorov2/dyalect/wiki/Array#add)).
  * Done: _Yield many_ ([Issue #96](https://github.com/vorov2/dyalect/issues/96), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#nested)).
  * Fixed: _Disabling warnings through compiler directive doesn't work_ ([Issue #300](https://github.com/vorov2/dyalect/issues/300)).
  * Fixed: _Simplified equaility test by `Array.remove`, `Array.indexOf`, `Array.lastIndexof`_ ([Issue #301](https://github.com/vorov2/dyalect/issues/301)).

# 0.12.0
  * Change: _Internal collection (arrays and tuples) object model refactoring._
  * Change: _Optimizations in compiler and virtual machine._
  * Change: _Embedding API is enhanced and refactored._
  * Done: _Migrate Dyalect to .NET Core 3.0_ ([Issue #295](https://github.com/vorov2/dyalect/issues/295)).
  * Done: _Add `Array.removeRange(items)` method_ ([Issue #285](https://github.com/vorov2/dyalect/issues/285), [docs](https://github.com/vorov2/dyalect/wiki/Array#removeRange)).
  * Done: _Add `Array.removeAll(predicate)` method_ ([Issue #286](https://github.com/vorov2/dyalect/issues/286), [docs](https://github.com/vorov2/dyalect/wiki/Array#removeAll)).
  * Done: _Add an ability to disable specific warnings (through command line)_ ([Issue #282](https://github.com/vorov2/dyalect/issues/282)).
  * Done: _Iterators to support `toTuple` method_ ([Issue #289](https://github.com/vorov2/dyalect/issues/289), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#toTuple)).
  * Done: _Tuple slicing_ ([Issue #287](https://github.com/vorov2/dyalect/issues/287), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#slice)).
  * Done: _Add a static `Array.sort` method_ ([Issue #291](https://github.com/vorov2/dyalect/issues/291), [docs](https://github.com/vorov2/dyalect/wiki/Array#ssort)).
  * Done: _Implement tuple sorting_ ([Issue #290](https://github.com/vorov2/dyalect/issues/290), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#sort)).
  * Done: _Allow to create tuples with one element using traling comma_ ([Issue #293](https://github.com/vorov2/dyalect/issues/293), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#overview)).
  * Done: _Tail call recursion optimization_ ([Issue #62](https://github.com/vorov2/dyalect/issues/62)).
  * Done: _There is no suffix to distinguish integers from floats_ ([Issue #297](https://github.com/vorov2/dyalect/issues/297), [docs](https://github.com/vorov2/dyalect/wiki/Float#overview)).
  * Done: _Support for compiler directives_ ([Issue #149](https://github.com/vorov2/dyalect/issues/149), [docs](https://github.com/vorov2/dyalect/wiki/Compiler-directives)).
  * Done: _Add an ability to disable specific warnings using compiler directives_ ([Issue #281](https://github.com/vorov2/dyalect/issues/281), [docs](https://github.com/vorov2/dyalect/wiki/Compiler-directives#warning)).
  * Done: _Match analysis is not powerful enough_ ([Issue #280](https://github.com/vorov2/dyalect/issues/280)).
  * Fixed: _Dyalect console doesn't print warnings when running tests_ ([Issue #283](https://github.com/vorov2/dyalect/issues/283)).
  * Fixed: _Dyalect console ignores build settings when running tests_ ([Issue #284](https://github.com/vorov2/dyalect/issues/284)).
  * Fixed: _Invalid treatment of shortcut function syntax_ ([Issue #294](https://github.com/vorov2/dyalect/issues/294)).

# 0.11.0
  * Change: _Enhancements and amendments to the .NET API and internal object model._
  * Fixed: _Member check operator `?` doesn't work with static members_ ([Issue #270](https://github.com/vorov2/dyalect/issues/270)).
  * Fixed: _Method `has` doesn't always work correctly_ ([Issue #271](https://github.com/vorov2/dyalect/issues/271)).
  * Fixed: _Label syntax and virtual machine crush_ ([Issue #274](https://github.com/vorov2/dyalect/issues/274)).
  * Fixed: _Type descriptors do not support method `has`_ ([Issue #272](https://github.com/vorov2/dyalect/issues/272)).
  * Fixed: _Function `len` is not fully supported by custom types_ ([Issue #276](https://github.com/vorov2/dyalect/issues/276)).
  * Done: _Add a built-in `eval` function_ ([Issue #269](https://github.com/vorov2/dyalect/issues/269), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#eval)).
  * Done: _Add `Array.swap` method_ ([Issue #273](https://github.com/vorov2/dyalect/issues/273), [docs](https://github.com/vorov2/dyalect/wiki/Array#swap)).
  * Done: _Implement `Map` data type_ ([Issue #235](https://github.com/vorov2/dyalect/issues/235), [docs](https://github.com/vorov2/dyalect/wiki/Map)).
  * Done: _Add a compiler switch to disable optimizations_ ([Issue #277](https://github.com/vorov2/dyalect/issues/277)).
  * Done: _Optimization of assignments using tuples_ ([Issue #278](https://github.com/vorov2/dyalect/issues/278)).

# 0.10.2
  * Fixed: _Parsing logic of `return` operator_ ([Issue #266](https://github.com/vorov2/dyalect/issues/266)).
  * Fixed: _Help command (#help) causes Dyalect Interactive to crush_ ([Issue #267](https://github.com/vorov2/dyalect/issues/267)).

# 0.10.1
  * Done: _Parser error messages are corrected in according to the language changes._
  * Done: _Add `readLine` built-in function_ ([Issue #263](https://github.com/vorov2/dyalect/issues/263)).
  * Done: _Code clean-ups and enhancements._

# 0.10.0
  * Done: _Add a multiline string literal_ ([Issue #206](https://github.com/vorov2/dyalect/issues/206), [docs](https://github.com/vorov2/dyalect/wiki/String#multiline)).
  * Done: _Object files_ ([Issue #49](https://github.com/vorov2/dyalect/issues/49)).
  * Done: _Support for script arguments_ ([Issue #39](https://github.com/vorov2/dyalect/issues/39)).
  * Done: _Code guards_ ([Issue #211](https://github.com/vorov2/dyalect/issues/211), [docs](https://github.com/vorov2/dyalect/wiki/Control-flow#guards)).
  * Done: _Add a standard `round` function_ ([Issue #257](https://github.com/vorov2/dyalect/issues/257), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#round)).
  * Done: _Add an ability to obtain a name of a function_ ([Issue #260](https://github.com/vorov2/dyalect/issues/260), [docs](https://github.com/vorov2/dyalect/wiki/Functions#name)).
  * Done: _Add an ability to obtain a reference to a caller function and current function_ ([Issue #259](https://github.com/vorov2/dyalect/issues/260), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#current)).
  * Done: _`do/while` loop_ ([Issue #32](https://github.com/vorov2/dyalect/issues/32), [docs](https://github.com/vorov2/dyalect/wiki/Control-flow#dowhile)).
  * Fixed: _Correct default custom type `toString` implementation_ ([Issue #256](https://github.com/vorov2/dyalect/issues/256)).
  * Fixed: _`assert` function crushes the runtime if a `nil` is passed as one of the arguments_ ([Issue #261](https://github.com/vorov2/dyalect/issues/261)).

# 0.9.14
  * Change: _Multiple runtime error messages are corrected._
  * Fixed: _Incorrect exception message for custom types for `IndexOutOfRange`_ ([Issue #253](https://github.com/vorov2/dyalect/issues/253)).

# 0.9.13
  * Done: _Pattern match validation_ ([Issue #241](https://github.com/vorov2/dyalect/issues/241)).
  * Done: _Tuple pattern optimization_ ([Issue #244](https://github.com/vorov2/dyalect/issues/244)).
  * Done: _Add `Array.reverse` method_ ([Issue #250](https://github.com/vorov2/dyalect/issues/250)).
  * Done: _Add `String.reverse` method_ ([Issue #249](https://github.com/vorov2/dyalect/issues/249)).
  * Fixed: _Pattern matching in variable declaration_ ([Issue #245](https://github.com/vorov2/dyalect/issues/245)).
  * Fixed: _Debug info is not generated for variable binding_ ([Issue #246](https://github.com/vorov2/dyalect/issues/246)).
  * Fixed: _Index out of range exception_ ([Issue #247](https://github.com/vorov2/dyalect/issues/247)).
  * Fixed: _Formatting of custom types as exceptions_ ([Issue #248](https://github.com/vorov2/dyalect/issues/248)).

# 0.9.12
  * _Code clean-ups and refactorings._
  * Fixed: _Dyalect console: specify directory as well as file_ ([Issue #234](https://github.com/vorov2/dyalect/issues/234)).
  * Fixed: _Shortcut syntax for functions_ ([Issue #236](https://github.com/vorov2/dyalect/issues/236)).
  * Fixed: _`Integer` and `Float` constructors do not work correctly_ ([Issue #237](https://github.com/vorov2/dyalect/issues/237)).
  * Fixed: _Illogical behavior of `for` loop_ ([Issue #238](https://github.com/vorov2/dyalect/issues/238)).
  * Fixed: _Pattern matching and `const`_ ([Issue #239](https://github.com/vorov2/dyalect/issues/239)).
  * Fixed: _Binding through pattern matching and redefinition_ ([Issue #240](https://github.com/vorov2/dyalect/issues/240)).
  * Fixed: _Interactive console: formatting of values_ ([Issue #242](https://github.com/vorov2/dyalect/issues/242)).

# 0.9.11
  * Change: _Default implementation of `toString` method for custom types is corrected for better output_.
  * Fixed: _Debug: Location is not generated for function application_ ([Issue #230](https://github.com/vorov2/dyalect/issues/230)).
  * Fixed: _Stack corruption_ ([Issue #231](https://github.com/vorov2/dyalect/issues/231)).
  * Fixed: _Nested comments are not supported_ ([Issue #232](https://github.com/vorov2/dyalect/issues/232)).

# 0.9.10
  * Done: _Allow to specify multiple files in Dyalect console_ ([Issue #228](https://github.com/vorov2/dyalect/issues/228)).
  * Fixed: _Incorrect location for the errors inside interpolated strings_ ([Issue #227](https://github.com/vorov2/dyalect/issues/227)).

# 0.9.9
  * Fixed: _Error message for type error is corrected._
  * Fixed: _`Integer` constructor should accepts strings_ ([Issue #223](https://github.com/vorov2/dyalect/issues/223)).
  * Fixed: _Too many arguments runtime error causes runtime to crash_ ([Issue #224](https://github.com/vorov2/dyalect/issues/224)).
  * Fixed: _Runtime crash_ ([Issue #225](https://github.com/vorov2/dyalect/issues/225)).
  * Fixed: _`Array.sort`: comparer always returns 0 if the result is of type `Float`_ ([Issue #226](https://github.com/vorov2/dyalect/issues/226)).

# 0.9.8
  * Done: _Add String.repeat method_ ([Issue #221](https://github.com/vorov2/dyalect/issues/221), [docs](https://github.com/vorov2/dyalect/wiki/String#repeat)).
  * Change: _Compiler error `CodeIslandMultipleExpressions` is decomissioned, now multiple expressions are allowed in code islands inside strings._
  * Fixed: _Incorrect location for errors inside interpolated strings_ ([Issue #220](https://github.com/vorov2/dyalect/issues/220)).
  * Fixed: _`String.concat` doesn't favor sequences_ ([Issue #219](https://github.com/vorov2/dyalect/issues/219)).

# 0.9.7
  * Fixed: _Tuples do not support indices of type `Char`_ ([Issue #217](https://github.com/vorov2/dyalect/issues/217)).
  * Fixed: _Tuple syntax: lambdas as elements_ ([Issue #216](https://github.com/vorov2/dyalect/issues/216)).
  * Fixed: _Tuple syntax - strings as keys_ ([Issue #215](https://github.com/vorov2/dyalect/issues/215)).

# 0.9.6
  * Fixed: _Current linker logic needs redesign to support object files_ ([Issue #212](https://github.com/vorov2/dyalect/issues/212)).
  * Fixed: _Dya: eval doesn't aways correctly recognize file names_ ([Issue #213](https://github.com/vorov2/dyalect/issues/213)).
  * Change: _Prefix for commands in interactive console is changed from `:` to `#`, e.g. `#help` instead of `:help`._

# 0.9.5
  * _Code clean-ups and refactorings._
  * Fixed: _Error when trying to override `set` operator_ ([Issue #205](https://github.com/vorov2/dyalect/issues/205)).
  * Fixed: _Method `String.capitalize` doesn't work correctly_ ([Issue #207](https://github.com/vorov2/dyalect/issues/207)).
  * Fixed: _Member check operator doesn't always work correctly_ ([Issue #208](https://github.com/vorov2/dyalect/issues/208)).
  * Fixed: _Inconsistency of autogenerated constructors_ ([Issue #209](https://github.com/vorov2/dyalect/issues/209)).
  * Change: _Exception message for `Index of invalid type` is updated to include more details and be less misleading._

# 0.9.4
  * Done: _Provide auto implementation of `set` for custom types_ ([Issue #203](https://github.com/vorov2/dyalect/issues/203)).
  * Fixed: _Member access and indexer overloading_ ([Issue #201](https://github.com/vorov2/dyalect/issues/201)).

# 0.9.3
  * Fixed: _Argument names are lost in variadic functions_ ([Issue #200](https://github.com/vorov2/dyalect/issues/200)).

# 0.9.2
  * Done: _Optimize compilation logic of referenced types and external functions_ ([Issue #193](https://github.com/vorov2/dyalect/issues/193)).
  * Fixed: _Linker doesn't look module in all the available paths_ ([Issue #195](https://github.com/vorov2/dyalect/issues/195)).
  * Fixed: _Interactive console failure after error in `import`_ ([Issue #194](https://github.com/vorov2/dyalect/issues/194)).
  * Fixed: _Fully qualified methods are not compiled correctly_ ([Issue #198](https://github.com/vorov2/dyalect/issues/198)).
  * Fixed: _Type constructors are not included into export list of a module_ ([Issue #197](https://github.com/vorov2/dyalect/issues/197)).
  * Fixed: _Fully qualified types from other modules do not work_ ([Issue #196](https://github.com/vorov2/dyalect/issues/196)).

# 0.9.1
  * Fixed: _`private` modifier doesn't work_ ([Issue #187](https://github.com/vorov2/dyalect/issues/187)).
  * Fixed: _`do` is recognized as a keyword_ ([Issue #188](https://github.com/vorov2/dyalect/issues/188)).
  * Fixed: _Reference resolution error_ ([Issue #189](https://github.com/vorov2/dyalect/issues/189)).
  * Fixed: _Stack corruption when module has only `import`-s_ ([Issue #190](https://github.com/vorov2/dyalect/issues/190)).
  * Fixed: _Multiple references of the same module_ ([Issue #191](https://github.com/vorov2/dyalect/issues/191)).

# 0.9.0
  * Done: _Modules as first class objects_ ([Issue #51](https://github.com/vorov2/dyalect/issues/51), [docs](https://github.com/vorov2/dyalect/wiki/Modules)).
  * Done: _Change a way how a custom type is deconstructed_ ([Issue #185](https://github.com/vorov2/dyalect/issues/185), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#methods)).
  * Done: _Add a support for relative paths in module imports_ ([Issue #173](https://github.com/vorov2/dyalect/issues/173), [docs](https://github.com/vorov2/dyalect/wiki/Modules)).
  * Done: _Access modifiers for functions_ ([Issue #50](https://github.com/vorov2/dyalect/issues/50), [docs](https://github.com/vorov2/dyalect/wiki/Functions#private_functions)).
  * Done: _Implement pattern matching with `is` operator_ ([Issue #172](https://github.com/vorov2/dyalect/issues/172), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#is)).
  * Done: _Optimize qualified access to an external name_ ([Issue #174](https://github.com/vorov2/dyalect/issues/174))
  * Done: _Add `String.replace` method_ ([Issue #175](https://github.com/vorov2/dyalect/issues/175), [docs](https://github.com/vorov2/dyalect/wiki/String#replace)).
  * Done: _Add `String.remove` method_ ([Issue #176](https://github.com/vorov2/dyalect/issues/176), [docs](https://github.com/vorov2/dyalect/wiki/String#remove)).
  * Done: _Remove an obsolete `typeof` operator_ ([Issue #171](https://github.com/vorov2/dyalect/issues/171)).
  * Done: _Optimize custom type creation (with autogenerated constructors)_ ([Issue #182](https://github.com/vorov2/dyalect/issues/182)).
  * Done: _Change design of type constructors_ ([Issue #184](https://github.com/vorov2/dyalect/issues/184), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#ctor)).
  * Fixed: _Nil pattern causes compiler for crush_ ([Issue #178](https://github.com/vorov2/dyalect/issues/178)).
  * Fixed: _Autogenerated constructor with no arguments_ ([Issue #179](https://github.com/vorov2/dyalect/issues/179)).
  * Fixed: _Types with duplicate names in the same module causes compiler to crush_ ([Issue #180](https://github.com/vorov2/dyalect/issues/180)).
  * Fixed: _Type definitions can be duplicated in interactive mode_ ([Issue #181](https://github.com/vorov2/dyalect/issues/181)).
  * Fixed: _Type member names in interactive mode_ ([Issue #183](https://github.com/vorov2/dyalect/issues/183)).

# 0.8.0
  * Done: _Custom types_ ([Issue #5](https://github.com/vorov2/dyalect/issues/5), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types)).
  * Done: _Auto-invoke functions_ ([Issue #159](https://github.com/vorov2/dyalect/issues/159), [docs](https://github.com/vorov2/dyalect/wiki/Functions#auto)).
  * Done: _Make `Integer.max`, `Integer.min`, `Float.max`, etc. static constants_ ([Issue #154](https://github.com/vorov2/dyalect/issues/154)).
  * Done: _Change `as` pattern syntax_ ([Issue #163](https://github.com/vorov2/dyalect/issues/163), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#patterns)).
  * Done: _Shortcut syntax for a default constructor_ ([Issue #165](https://github.com/vorov2/dyalect/issues/165), [docs](https://github.com/vorov2/dyalect/wiki/Functions#static_methods)).
  * Done: _Add an ability to pattern match custom types_ ([Issue #161](https://github.com/vorov2/dyalect/issues/161), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#patterns)).
  * Done: _Change all standard `new` static methods to default constructors_ ([Issue #166](https://github.com/vorov2/dyalect/issues/166)).
  * Done: _Add default constructors to `Integer`, `Float`, etc._ ([Issue #169](https://github.com/vorov2/dyalect/issues/169)).
  * Done: _Make indexer overridable_ ([Issue #155](https://github.com/vorov2/dyalect/issues/155), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#oplist)).
  * Done: _Add a common static `default` field_ ([Issue #167](https://github.com/vorov2/dyalect/issues/167)).

# 0.7.6
  * Done: _Add `Tuple.new` static method_ ([Issue #153](https://github.com/vorov2/dyalect/issues/153), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#new)).
  * Done: _`obj.Has(...)` optimization for simple cases_ ([Issue #156](https://github.com/vorov2/dyalect/issues/156)).
  * Access to tuple elements using member access syntax (e.g. `tuple.fieldName`) is refactored and redesigned for better efficiency.
  * Fixed: _Regression for member check (`?`) operator_ ([Issue #157](https://github.com/vorov2/dyalect/issues/157)).

# 0.7.5
  * Done: _Replace `typeof` with `getType` method_ ([Issue #142](https://github.com/vorov2/dyalect/issues/142), [docs](https://github.com/vorov2/dyalect/wiki/Standard-methods#getType)).
  * Done: _`TypeInfo` object and metadata_ ([Issue #150](https://github.com/vorov2/dyalect/issues/150), [docs](https://github.com/vorov2/dyalect/wiki/TypeInfo)).
  * Fixed: _Stack is corrupted_ ([Issue #151](https://github.com/vorov2/dyalect/issues/151)).
  * Fixed: _Detection whether an object supports a standard method_ ([Issue #148](https://github.com/vorov2/dyalect/issues/148)).

# 0.7.4
  * Done: _Add an ability to convert a char to int and vice versa_ ([Issue #146](https://github.com/vorov2/dyalect/issues/146), [docs](https://github.com/vorov2/dyalect/wiki/Char#order), [more docs](https://github.com/vorov2/dyalect/wiki/Char#new))).
  * Done: _Add padLeft/padRight methods to String_ ([Issue #144](https://github.com/vorov2/dyalect/issues/144), [docs](https://github.com/vorov2/dyalect/wiki/String#padLeft)).
  * Fixed: _An error in `Array.insert`_ ([Issue #145](https://github.com/vorov2/dyalect/issues/145)).

# 0.7.3
  * Done: _Restrict overriding `has`_ ([Issue #134](https://github.com/vorov2/dyalect/issues/134), [docs](https://github.com/vorov2/dyalect/wiki/Standard-methods#has)).
  * Done: _Add `Array.insertRange` method_ ([Issue #135](https://github.com/vorov2/dyalect/issues/135), [docs](https://github.com/vorov2/dyalect/wiki/Array#insertRange)).
  * Done: _Support `+` for array concatenation_ ([Issue #137](https://github.com/vorov2/dyalect/issues/137), [docs](https://github.com/vorov2/dyalect/wiki/Array#operators)).
  * Fixed: _`Array.insert` exception_ ([Issue #136](https://github.com/vorov2/dyalect/issues/136)).
  * Fixed: _`Iterator` literal and methods_ ([Issue #138](https://github.com/vorov2/dyalect/issues/138)).
  * Fixed: _Incorrect `toString` of a function_ ([Issue #139](https://github.com/vorov2/dyalect/issues/139)).
  * Fixed: _`Function.toString` insufficient details_ ([Issue #140](https://github.com/vorov2/dyalect/issues/140)).
  * Fixed: _`Iterator.concat` failure_ ([Issue #141](https://github.com/vorov2/dyalect/issues/141)).

# 0.7.2
  * Fixed: _Compiler indexing tables_ ([Issue #129](https://github.com/vorov2/dyalect/issues/129)).
  * Fixed: _Quotes inside code islands (string interpolation)_ ([Issue #132](https://github.com/vorov2/dyalect/issues/132)).

# 0.7.1
  * Done: _Variadic functions and iterators_ ([Issue #127](https://github.com/vorov2/dyalect/issues/127), [docs](https://github.com/vorov2/dyalect/wiki/Functions#variadic_functions)).
  * Fixed: _Variadic functions and named arguments_ ([Issue #128](https://github.com/vorov2/dyalect/issues/128)).
  * Fixed: _Named arguments and VM crush_ ([Issue #130](https://github.com/vorov2/dyalect/issues/130)).

# 0.7.0
  * Done: _Cloning objects_ ([Issue #48](https://github.com/vorov2/dyalect/issues/48), [docs](https://github.com/vorov2/dyalect/wiki/Standard-methods#clone)).
  * Done: _Tuple: add static methods `pair` and `triple`_ ([Issue #103](https://github.com/vorov2/dyalect/issues/103), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#pair), [more docs](https://github.com/vorov2/dyalect/wiki/Tuple#triple)).
  * Done: _Function composition_ ([Issue #104](https://github.com/vorov2/dyalect/issues/104), [docs](https://github.com/vorov2/dyalect/wiki/Functions#compose), [more docs](https://github.com/vorov2/dyalect/wiki/Functions#scompose)).
  * Done: _Char: add standard methods_ ([Issue #52](https://github.com/vorov2/dyalect/issues/52), [docs](https://github.com/vorov2/dyalect/wiki/Char#methods)).
  * Done: _Array copying_ ([Issue #36](https://github.com/vorov2/dyalect/issues/36), [docs](https://github.com/vorov2/dyalect/wiki/Array#copy)).
  * Done: _Extend `String.indexOf` functon_ ([Issue #112](https://github.com/vorov2/dyalect/issues/112), [docs](https://github.com/vorov2/dyalect/wiki/String#indexOf)).
  * Done: _Add `String.join` method_ ([Issue #121](https://github.com/vorov2/dyalect/issues/121), [docs](https://github.com/vorov2/dyalect/wiki/String#join)).
  * Done: _Add coalesce operator_ ([Issue #100](https://github.com/vorov2/dyalect/issues/100), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#coalesce)).
  * Done: _Add `rnd` built-in function_ ([Issue #122](https://github.com/vorov2/dyalect/issues/122), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#rnd)).
  * Done: _Exception handling_ ([Issue #24](https://github.com/vorov2/dyalect/issues/24), [docs](https://github.com/vorov2/dyalect/wiki/Exception-handling)).
  * Done: _Check whether an operator is implemented_ ([Issue #67](https://github.com/vorov2/dyalect/issues/67), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#opmethods), [more docs](https://github.com/vorov2/dyalect/wiki/Standard-methods#has)).
  * Done: _Add a pattern to check if a method is implemented_ ([Issue #107](https://github.com/vorov2/dyalect/issues/107), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#patterns)).
  * Done: _Allow rebinding variables using patterns_ ([Issue #118](https://github.com/vorov2/dyalect/issues/118), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#set)).
  * Done: _Arrays: Make slices first class entities_ ([Issue #60](https://github.com/vorov2/dyalect/issues/60)).
  * Done: _Pattern matching inside var_ ([Issue #106](https://github.com/vorov2/dyalect/issues/106), [docs](https://github.com/vorov2/dyalect/wiki/Pattern-matching#var)).
  * Fixed: _Method comparison_ ([Issue #119](https://github.com/vorov2/dyalect/issues/119)).
  * Fixed: _Char escape codes_ ([Issue #120](https://github.com/vorov2/dyalect/issues/120)).
  * Fixed: _`Array.addRange` and iterator literal_ ([Issue #125](https://github.com/vorov2/dyalect/issues/125)).
  * Fixed: _Iterators and modifications_ ([Issue #124](https://github.com/vorov2/dyalect/issues/124)).

# 0.6.2
  * Done: _Array.empty should accept function as a default_ ([Issue #114](https://github.com/vorov2/dyalect/issues/114)).
  * Fixed: _Negative literals and parser_ ([Issue #113](https://github.com/vorov2/dyalect/issues/113)). A minus sign `-` is no longer a part of numeric literals and is always treated as a negation operator.

# 0.6.1
  * Fixed: _Stack is corruped and ranges_ ([Issue #108](https://github.com/vorov2/dyalect/issues/108)).
  * Fixed: _Error in pattern grammar_ ([Issue #109](https://github.com/vorov2/dyalect/issues/109)).
  * Fixed: _Allow variable rebinding in patterns_ ([Issue #110](https://github.com/vorov2/dyalect/issues/110)).

# 0.6.0
  * Error reporting by parser is expanded and corrected.
  * Implemented pattern matching using `match` expression ([Issue #4](https://github.com/vorov2/dyalect/issues/4)):
    ```swift
    match exp {
        (1, x, 3) => x,
        (_, 4, x) when x % 2 == 0 => x,
        _ => 0
    }
    ```
    The following patterns are currently supported: "and" pattern (`&&`), "or" pattern (`||`), grouping pattern, tuple pattern, array pattern, name/field pattern, constant pattern, nil pattern, range pattern, biding-to-name pattern, type check pattern, "as" pattern. Guards are also supported.
  * Pattern matching is supported inside `for` cycles. Instead of a plain loop variables it is possible to use patterns, including nested patterns (related issue: [Issue #4](https://github.com/vorov2/dyalect/issues/4)). If an element doesn't match it gets skipped, e.g. this code:
    ```swift
    for (x, 1) in [(2, 1), (3,3), (1, 1)] {
        print(x)
    }
    ```
    outputs `2` and `1`.
  * Now `for` cycle supports guards in the following manner:
    ```swift
    var arr = []
    for x in [1,2,3,4,5,6,7,8,9,10] when x % 2 == 0 {
        arr.add(x)
    }
    ```
  * New static method `concat` is added to types `Iterator` and `Array` (related issue: [#37](https://github.com/vorov2/dyalect/issues/37)). This method has the following signature:
    ```swift
    static func concat(values...) { }
    ```
    It accepts a variable number of objects that implement iterator pattern and returns either an array (for `Array` type) or a new combined iterator (for `Iterator` type).
  * An implementation of `toString` method is changed for iterators - now `toString` execute an iterator and formats all of its values to a string:
    ```swift
    (1..5).toString() == "{ 1, 2, 3, 4, 5}" //Evaluates to true
    ```
  * Now interactive console displays a correct error message if an exception occurs while trying to format script output to a string.
  * Structural equality for tuples is implemented (related issue: [#43](https://github.com/vorov2/dyalect/issues/43)).
  * Now it is possible to slice arrays using indexer syntax (related issue: [#59](https://github.com/vorov2/dyalect/issues/59)):
    ```swift
    var xs = [0,1,2,3,4,5,6,7,8,9]
    xs[1..5] //evaluates to [1,2,3,4]
    ```
    Slicing is implemented by calling a `slice` method, so the code above is equivalent to:
    ```swift
    xs.slice(1, 4)
    ```
  * Now it is possible to create an array based on range using the following syntax: `[n..k]`. It is an equivalent to `(n..k).toArray()` (related issue: [#92](https://github.com/vorov2/dyalect/issues/92)).
  * Now iterators has a `len` method which returns a total number of elements in an interator (related issue: [#99](https://github.com/vorov2/dyalect/issues/99)).
  * A feature is implemented: _Integer: add standard methods_ ([Issue #54](https://github.com/vorov2/dyalect/issues/54)).
  * A feature is implemented: _Float: add standard methods_ ([Issue #55](https://github.com/vorov2/dyalect/issues/55)).
  * A bug fixed: _Incorrect type info is generated for iterator_ ([Issue #93](https://github.com/vorov2/dyalect/issues/93)).
  * A bug fixed: _Anonymous function and iterator_ ([Issue #94](https://github.com/vorov2/dyalect/issues/94)).
  * A bug fixed: _Array.slice - Index out of range_ ([Issue #97](https://github.com/vorov2/dyalect/issues/97)).
  * A bug fixed: _Array.slice - index of invalid type crushed VM_ ([Issue #98](https://github.com/vorov2/dyalect/issues/98)).

# 0.5.8
  * A bug fixed: _String indexer returns a string instead of a char_ ([Issue #84](https://github.com/vorov2/dyalect/issues/84)).
  * A bug fixed: _String indexation by a compiler_ ([Issue #83](https://github.com/vorov2/dyalect/issues/83)).
  * A bug fixed: _String and char comparison_ ([Issue #85](https://github.com/vorov2/dyalect/issues/85)).

# 0.5.7
  * A task is implemented: _Better parser error messages_ ([Issue #26](https://github.com/vorov2/dyalect/issues/26)).

# 0.5.6
  * A bug fixed: _VM crush: Stack is corrupted_ ([Issue #77](https://github.com/vorov2/dyalect/issues/77)).
  * A bug fixed: _VM crush & invalid break behavior_ ([Issue #78](https://github.com/vorov2/dyalect/issues/78)).
  * A task is implemented: _Optimize compilation logic_ ([Issue #76](https://github.com/vorov2/dyalect/issues/76)).
  * A bug fixed: _Invalid processing for default value_ ([Issue #79](https://github.com/vorov2/dyalect/issues/79)).

# 0.5.5
  * Test runner in interactive console is corrected (it didn't display the reason of a test failure).
  * Now interactive console supports a `-time` switch that displays execution time.
  * Pretty print corrected for the unary operations.
  * A bug fixed: _Critical VM failure_ ([Issue #74](https://github.com/vorov2/dyalect/issues/74)).

# 0.5.4
  * A bug fixed: _Multiline mode works incorrectly in console_ ([Issue #70](https://github.com/vorov2/dyalect/issues/70)).
  * A new method `isEmpty` is added to a `String` data ([Issue #68](https://github.com/vorov2/dyalect/issues/68)). This method returns `true` if a string consists of only white spaces, tabs or new lines characters.
  * A new standard `parse` function is added ([Issue #57](https://github.com/vorov2/dyalect/issues/57)):
    ```
    dy>parse("[1,2,3,('c',true,23.4),nil]")
    [1, 2, 3, ('c', true, 23.4), nil] :: Array
    ```
  * Error handling refactored.

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
    [1, 2, 3] //same as Array(1, 2, 3)
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
