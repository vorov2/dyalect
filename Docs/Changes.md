# 0.46.5
  * Change: _Code refactoring._
  * Change: _Instruction set for virtual machine is simplified_.
  * Change: _Optimize `Interop` type, cache types_ ([Issue #931](https://github.com/vorov2/dyalect/issues/931)).
  * Add: _Add a built-in `length` function_ ([Issue #930](https://github.com/vorov2/dyalect/issues/930), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#length)).

# 0.46.4
  * Change: _Code refactoring._
  * Change: _`Unbox` opcode is no longer used and decomissioned._
  * Add: _Add `Iterator.Fold` method_ ([Issue #927](https://github.com/vorov2/dyalect/issues/927), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#fold)).
  * Fix: _Method invocation has an error that can cause VM to crush_ ([Issue #928](https://github.com/vorov2/dyalect/issues/928)).

# 0.46.3
  * Fix: _A method can be nested in another function_ ([Issue #922](https://github.com/vorov2/dyalect/issues/922)).

# 0.46.2
  * Change: _Several run-time errors are changed for better clarity._
  * Change: _Several parser error messages are corrected for better clarity._
  * Add: _Write IL generator_ ([Issue #375](https://github.com/vorov2/dyalect/issues/375), [docs](https://github.com/vorov2/dyalect/wiki/Dyalect-console)).
  * Fix: _Iterators do not raise an error in a case of invalid index_ ([Issue #919](https://github.com/vorov2/dyalect/issues/919)).
  * Fix: _Mixin `Show` can generate invalid string representation for type annotations_ ([Issue #920](https://github.com/vorov2/dyalect/issues/920)).

# 0.46.1
  * Change: _Optimization of `for` cycle for simple cases._
  * Fix: _Pretty print of `for` cycle AST node corrected._
  * Fix: _Module member access doesn't work with disabled optimizer_ ([Issue #917](https://github.com/vorov2/dyalect/issues/917)).

# 0.46.0
  * Change: _Disallow override of tuple indexer and `Length` method_ ([Issue #906](https://github.com/vorov2/dyalect/issues/906)).
  * Add: _Consider non-overridable methods_ ([Issue #900](https://github.com/vorov2/dyalect/issues/900), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#extending)).
  * Add: _A function to obtain a list of supported mixins_ ([Issue #913](https://github.com/vorov2/dyalect/issues/913), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#mixins)).
  * Add: _Support for custom format strings for integers and floats_ ([Issue #883](https://github.com/vorov2/dyalect/issues/883)).
  * Fix: _Instance method `Tuple.Concat` doesn't work_ ([Issue #915](https://github.com/vorov2/dyalect/issues/915)).

# 0.45.2
  * Change: _Type system refactoring and clean-up._
  * Change: _`Regex` should support `Equatable` mixin_ ([Issue #911](https://github.com/vorov2/dyalect/issues/911)).
  * Change: _All types with non-reference equality should support `Equatable` mixin_ ([Issue #912](https://github.com/vorov2/dyalect/issues/912)).
  * Change: _All mixins with non-standard `ToString` implementation should explicitly support `Show`_ ([Issue #910  ](https://github.com/vorov2/dyalect/issues/910)).
  * Fix: _Mixin Number not closed for extensibility_ ([Issue #907](https://github.com/vorov2/dyalect/issues/907)).
  * Fix: _Some built-in types have incorrect support for standard mixins_ ([Issue #908](https://github.com/vorov2/dyalect/issues/908)).
  * Fix: _`StringBuilder` doesn't support `Lookup` mixin_ ([Issue #909](https://github.com/vorov2/dyalect/issues/909)).

# 0.45.1
  * Change: _Optimizations in type system._
  * Fix: _Lines in exception stack trace can be duplicated_ ([Issue #902](https://github.com/vorov2/dyalect/issues/902)).
  * Fix: _Invalid override for `Length` still performs override_ ([Issue #903](https://github.com/vorov2/dyalect/issues/903)).

# 0.45.0
  * Change: _Now it is not possible to use keyword `private` as identifier._
  * Change: _Parser grammer refactoring._
  * Change: _It is not possible to implement `Iterate` method for custom types_ ([Issue #889](https://github.com/vorov2/dyalect/issues/889)).
  * Change: _Consider separating operator `in` and `Contains` method_ ([Issue #888](https://github.com/vorov2/dyalect/issues/888)).
  * Change: _Remove `ToLiteral` method, generalize `ToString`_ ([Issue #888](https://github.com/vorov2/dyalect/issues/888)).
  * Change: _Reconsider private constructors for custom types_ ([Issue #895](https://github.com/vorov2/dyalect/issues/895)).
  * Change: _Global initialization blocks for all custom type constructors_ ([Issue #896](https://github.com/vorov2/dyalect/issues/896)).
  * Change: _Redesign of access to custom type internals_ ([Issue #899](https://github.com/vorov2/dyalect/issues/899), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types)).
  * Add: _Add `Disposable` mixin_ ([Issue #894](https://github.com/vorov2/dyalect/issues/894), [docs](https://github.com/vorov2/dyalect/wiki/Mixins#disposable)).
  * Add: _Add `Functor` mixin_ ([Issue #893](https://github.com/vorov2/dyalect/issues/893), [docs](https://github.com/vorov2/dyalect/wiki/Mixins#functor)).
  * Add: _Redesign standard mixins_ ([Issue #892](https://github.com/vorov2/dyalect/issues/892), [docs](https://github.com/vorov2/dyalect/wiki/Mixins)).
  * Add: _Consider automatic implementation of standard mixins_ ([Issue #869](https://github.com/vorov2/dyalect/issues/869), [docs](https://github.com/vorov2/dyalect/wiki/Mixins)).
  * Add: _Add literal for a `Dictionary`_ ([Issue #304](https://github.com/vorov2/dyalect/issues/304), [docs](https://github.com/vorov2/dyalect/wiki/Dictionary)).
  * Add: _Add `toString` function to invoke original "to string" implementation_ ([Issue #884](https://github.com/vorov2/dyalect/issues/884), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#toString)).
  * Add: _Custom type automatic conversion to tuples and dictionaries_ ([Issue #868](https://github.com/vorov2/dyalect/issues/868), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#overview)).
  * Add: _Add `Tuple.RemoveField` method_ ([Issue #875](https://github.com/vorov2/dyalect/issues/875), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#removeField)).
  * Add: _Consider method_missing method_ ([Issue #885](https://github.com/vorov2/dyalect/issues/885), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#missing)).
  * Fix: _Stack corruption because of indexer_ ([Issue #863](https://github.com/vorov2/dyalect/issues/863)).
  * Fix: _When InvalidOverload error is generated an overload is still done_ ([Issue #891](https://github.com/vorov2/dyalect/issues/891)).
  * Fix: _Tuple is incorrectly transformed to an iterator_ ([Issue #890](https://github.com/vorov2/dyalect/issues/890)).
  * Fix: _`rawset` and `rawget` only accept integer indices_ ([Issue #897](https://github.com/vorov2/dyalect/issues/897)).

# 0.44.4
  * Change: _Refactoring of built-in collection types._
  * Fix: _Regression: invalid index for tuple indexer causes a crash_ ([Issue #880](https://github.com/vorov2/dyalect/issues/880)).
  * Fix: _`Slice` for arrays and tuples does unnecessary allocations_ ([Issue #881](https://github.com/vorov2/dyalect/issues/881)).
  * Fix: _Method `Format` doesn't take into account format strings_ ([Issue #882](https://github.com/vorov2/dyalect/issues/882)).

# 0.44.3 
  * Change: _Type system refactoring._
  * Change: _Refactor `Regex` to use tuples for the results_ ([Issue #872](https://github.com/vorov2/dyalect/issues/872), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Regex)).
  * Fix: _Passing arguments to a variant, undesirable side effects_ ([Issue #874](https://github.com/vorov2/dyalect/issues/874)).
  * Fix: _Implicit conversions from string to char can crash VM_ ([Issue #876](https://github.com/vorov2/dyalect/issues/876)).
  * Fix: _An exception from iterator indexer can crush VM_ ([Issue #877](https://github.com/vorov2/dyalect/issues/877)).

# 0.44.2
  * Fix: _It is possible to obtain a reference to `Contains` even if an object doesn't implement it_ ([Issue #867](https://github.com/vorov2/dyalect/issues/867)).
  * Fix: _VM crashes when calling `Contains` on an instance of custom type_ ([Issue #866](https://github.com/vorov2/dyalect/issues/866)).
  * Fix: _Error stack trace is not always accurate_ ([Issue #871](https://github.com/vorov2/dyalect/issues/871)).

# 0.44.1
  * Change: _Optimizations and refactoring of exception handling logic._
  * Change: _Global refactoring of internal mechanism of member function invocation._
  * Fix: _`throw` with a value other than variant crashes VM_ ([Issue #834](https://github.com/vorov2/dyalect/issues/834)).

# 0.44.0
  * Change: _Refactorings and optimizations in virtual machine and runtime environment._
  * Change: _Enhancements and optimizations to source generators for types and modules._
  * Change: _Mixin Bounded contains only static properties_ ([Issue #831](https://github.com/vorov2/dyalect/issues/831)).
  * Change: _Make method `String.Empty` property_ ([Issue #834](https://github.com/vorov2/dyalect/issues/834)).
  * Change: _Make method `Float.Inf` property and rename it to `Infinity`_ ([Issue #833](https://github.com/vorov2/dyalect/issues/833)).
  * Change: _Make methods `Min`, `Max` and `Default` properties_ ([Issue #832](https://github.com/vorov2/dyalect/issues/832)).
  * Change: _Convert `ByteArray.Position` method to property_ ([Issue #835](https://github.com/vorov2/dyalect/issues/835)).
  * Change: _Optimize cloning behavior for tuples_ ([Issue #838](https://github.com/vorov2/dyalect/issues/838)).
  * Change: _Optimize cloning behavior for variants_ ([Issue #841](https://github.com/vorov2/dyalect/issues/841)).
  * Change: _Dyalect Console "swallows" errors thrown during formatting of output to a string_ ([Issue #846](https://github.com/vorov2/dyalect/issues/846)).
  * Change: _Rename `Result.Value` method to `Result.GetValue`_ ([Issue #847](https://github.com/vorov2/dyalect/issues/847)).
  * Change: _Stack trace doesn't print the name of the type for member functions_ ([Issue #849](https://github.com/vorov2/dyalect/issues/849)).
  * Change: _Change patterns priority_ ([Issue #850](https://github.com/vorov2/dyalect/issues/850)).
  * Change: _Use system UI culture for formatting to string_ ([Issue #852](https://github.com/vorov2/dyalect/issues/852)).
  * Change: _ToLiteral can raise an exception if it is not explicitly supported_ ([Issue #766](https://github.com/vorov2/dyalect/issues/766)).
  * Change: _A typeinfo instance (if it implements a static function of the same name) should be a substitute for a function_ ([Issue #860](https://github.com/vorov2/dyalect/issues/860)).
  * Add: _Remove name field from `TypeInfo`, add `getTypeName` function_ ([Issue #813](https://github.com/vorov2/dyalect/issues/813), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#operators)).
  * Add: _Support concatenation with + for iterators_ ([Issue #809](https://github.com/vorov2/dyalect/issues/809), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#typeName)).
  * Add: _Add a way to enforce reference comparison_ ([Issue #839](https://github.com/vorov2/dyalect/issues/839), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#referenceEquals)).
  * Add: _Add `Iterator.Single` method_ ([Issue #818](https://github.com/vorov2/dyalect/issues/818), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#single)).
  * Add: _Concatenate strings using juxtaposition_ ([Issue #842](https://github.com/vorov2/dyalect/issues/842), [docs](https://github.com/vorov2/dyalect/wiki/String#concatenation)).
  * Add: _All types should implicitly support ToLiteral_ ([Issue #859](https://github.com/vorov2/dyalect/issues/859)).
  * Fix: _Regression: incorrect type name in some type related errors_ ([Issue #836](https://github.com/vorov2/dyalect/issues/836)).
  * Fix: _Different behavior with optimizer turned on_ ([Issue #840](https://github.com/vorov2/dyalect/issues/840)).
  * Fix: _Stack trace incorrectly reporting global scope_ ([Issue #843](https://github.com/vorov2/dyalect/issues/843)).
  * Fix: _Strange behavior of `Iterator.Concat`_ ([Issue #845](https://github.com/vorov2/dyalect/issues/845)).
  * Fix: _Compiler may report similar errors twice_ ([Issue #848](https://github.com/vorov2/dyalect/issues/848)).
  * Fix: _Correct formatting to a string of several patterns_ ([Issue #851](https://github.com/vorov2/dyalect/issues/851)).
  * Fix: _Function `parse` works incorrectly with interpolated strings_ ([Issue #853](https://github.com/vorov2/dyalect/issues/853)).
  * Fix: _Interpolated strings do not work correctly inside pattern ranges_ ([Issue #856](https://github.com/vorov2/dyalect/issues/856)).
  * Fix: _Parsers optimizations doesn't work in escape code parser_ ([Issue #857](https://github.com/vorov2/dyalect/issues/857)).
  * Fix: _Invalid compilation of range pattern with interpolated strings_ ([Issue #854](https://github.com/vorov2/dyalect/issues/854)).
  * Fix: _Incorrect behavior of interpolated strings as default values_ ([Issue #858](https://github.com/vorov2/dyalect/issues/858)).

# 0.43.0
  * Add: _All collection types should support casting to `Set`_ ([Issue #806](https://github.com/vorov2/dyalect/issues/806)).
  * Add: _An ability to convert an iterator to a stateful function_ ([Issue #807](https://github.com/vorov2/dyalect/issues/807), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#operators)).
  * Add: _`Bool` to support `Min` and `Max`_ ([Issue #817](https://github.com/vorov2/dyalect/issues/817), [docs](https://github.com/vorov2/dyalect/wiki/Bool#min)).
  * Add: _Consider a special mixin (e.g. `Bounded`) for `Max` and `Min` methods_ ([Issue #817](https://github.com/vorov2/dyalect/issues/817), [docs](https://github.com/vorov2/dyalect/wiki/Mixins#bounded)).
  * Add: _An ability to change working directory in `Dya`_ ([Issue #827](https://github.com/vorov2/dyalect/issues/827), [docs](https://github.com/vorov2/dyalect/wiki/Dyalect-console#commands)).
  * Add: _Use `+` for function composition_ ([Issue #812](https://github.com/vorov2/dyalect/issues/812), [docs](https://github.com/vorov2/dyalect/wiki/Functions#operators)).
  * Add: _Add `Object` property to functions_ ([Issue #810](https://github.com/vorov2/dyalect/issues/810), [docs](https://github.com/vorov2/dyalect/wiki/Functions#object)).
  * Change: _Static `Array.Sort` to support other data types_ ([Issue #797](https://github.com/vorov2/dyalect/issues/797), [docs](https://github.com/vorov2/dyalect/wiki/Array#ssort)).
  * Change: _Change `Integer.IsMultiple(of)` to `Integer.IsMultipleOf(value)`_ ([Issue #804](https://github.com/vorov2/dyalect/issues/804), [docs](https://github.com/vorov2/dyalect/wiki/Integer#isMultipleOf)).
  * Change: _Refactor date and time API internals_ ([Issue #825](https://github.com/vorov2/dyalect/issues/825)).
  * Change: _Use source generators to aid implementation of types and modules_ ([Issue #826](https://github.com/vorov2/dyalect/issues/826)).
  * Fix: _Static `Array.Sort` fails with a wrong exception when a type is invalid_ ([Issue #814](https://github.com/vorov2/dyalect/issues/814)).
  * Fix: _Interactive console crush when invoking a function_ ([Issue #828](https://github.com/vorov2/dyalect/issues/828)).
  * Fix: _`Time` returns incorrect values for `Min` and `Max`_ ([Issue #824](https://github.com/vorov2/dyalect/issues/824)).
  * Fix: _Function comparison_ ([Issue #821](https://github.com/vorov2/dyalect/issues/821)).
  * Fix: _`Drive.TotalFreeSpace` property is incorrectly named `Drive.TotalFreeSize`_ ([Issue #822](https://github.com/vorov2/dyalect/issues/822)).
  * Fix: _`Iterator.Shuffle` can return the same sequence as before in rare cases_ ([Issue #820](https://github.com/vorov2/dyalect/issues/820)).
  * Fix: _Invalid names of parameters for `SetCursorPosition` function_ ([Issue #823](https://github.com/vorov2/dyalect/issues/823)).
  * Fix: _Associativity of operator `<<`_ ([Issue #829](https://github.com/vorov2/dyalect/issues/829), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#apps)).
  * Fix: _Fix the behavior of negative indices_ ([Issue #819](https://github.com/vorov2/dyalect/issues/819)).

# 0.42.1
  * Change: _Runtime type system refactoring._
  * Fix: _Methods `Directory.Copy` and `Directory.Move` do not work_ ([Issue #803](https://github.com/vorov2/dyalect/issues/803)).
  * Fix: _Some metadata about `Variant` constructor parameters may be lost_ ([Issue #805](https://github.com/vorov2/dyalect/issues/805)).

# 0.42.0
  * Change: _`TimeDelta` data type is refactored._
  * Change: _Refactoring in virtual machine and runtime error handling layer._
  * Change: _Refactor `DateTime` and `LocalDateTime` to be based on `Date` and `Time`_ ([Issue #794](https://github.com/vorov2/dyalect/issues/794)).
  * Change: _`DateType` types should return instances of `Time` and `Date` from properties of the same name_ ([Issue #790](https://github.com/vorov2/dyalect/issues/790)).
  * Change: _Use new engine for `DateTime` and `LocalDateTime` formatting_ ([Issue #792](https://github.com/vorov2/dyalect/issues/792)).
  * Change: _Use new engine for `DateTime` and `LocalDateTime` parsing_ ([Issue #793](https://github.com/vorov2/dyalect/issues/793)).
  * Change: _Refactor `DateTime` and `LocalDateTime` to be based on `Date` and `Time`_ ([Issue #794](https://github.com/vorov2/dyalect/issues/794)).
  * Add: _`TimeDelta` type should support `Comparable` mixin_ ([Issue #789](https://github.com/vorov2/dyalect/issues/789), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta)).
  * Add: _Add `Time` date type_ ([Issue #787](https://github.com/vorov2/dyalect/issues/787), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Time)).
  * Add: _Add `Date` date type_ ([Issue #791](https://github.com/vorov2/dyalect/issues/791), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Date)).
  * Add: _`Guid` should support comparison operations_ ([Issue #799](https://github.com/vorov2/dyalect/issues/799)).
  * Add: _`Guid` should implement `Comparable` mixin_ ([Issue #796](https://github.com/vorov2/dyalect/issues/796)).
  * Add: _Add `TimeDelta.Negate` method_ ([Issue #800](https://github.com/vorov2/dyalect/issues/800), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta#negate)).
  * Add: _Add method `Calendar.ParseDateTime`_ ([Issue #798](https://github.com/vorov2/dyalect/issues/798), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Calendar#parseDateTime)).
  * Fix: _Escape codes in Date/Time format specifiers do not work correctly_ ([Issue #795](https://github.com/vorov2/dyalect/issues/795)).
  * Fix: _`TimeDelta` formatting can do incorrect padding for output values_ ([Issue #801](https://github.com/vorov2/dyalect/issues/801)).

# 0.41.0
  * _Project migrated to .NET 6.0_
  * Add: _Add `ToSet` method to collections_ ([Issue #770](https://github.com/vorov2/dyalect/issues/770)).
  * Add: _Add an ability to modify fields in read-only tuples through creating new tuple instances_ ([Issue #741](https://github.com/vorov2/dyalect/issues/741), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#alter)).
  * Add: _Add `Iterator.ForEach` method_ ([Issue #765](https://github.com/vorov2/dyalect/issues/765), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#forEach)).
  * Add: _Type Module should support `Contains` method_ ([Issue #772](https://github.com/vorov2/dyalect/issues/772), [docs](https://github.com/vorov2/dyalect/wiki/Modules#contains)).
  * Add: _Add `Iterator.Distinct` method_ ([Issue #739](https://github.com/vorov2/dyalect/issues/739), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#distinct)).
  * Add: _Support casting of an `Iterator` to `Set`_ ([Issue #776](https://github.com/vorov2/dyalect/issues/776)).
  * Add: _Add properties `Date` and `Time` to `DateTime` and `LocalDateTime`_ ([Issue #758](https://github.com/vorov2/dyalect/issues/758), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.DateTime#date)).
  * Add: _Add method `GetField` to `Interop` type_  ([Issue #779](https://github.com/vorov2/dyalect/issues/779), [docs](https://github.com/vorov2/dyalect/wiki/Interop#getField)).
  * Add: _Add `TimeDelta.FromTicks` method_ ([Issue #785](https://github.com/vorov2/dyalect/issues/785), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta#fromTicks)).
  * Add: _`TimeDelta` to support both `Ticks` and `TotalTicks` properties_ ([Issue #784](https://github.com/vorov2/dyalect/issues/784), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta#totalTicks)).
  * Add: _`Add support for negation operator to `TimeDelta`_ ([Issue #783](https://github.com/vorov2/dyalect/issues/783), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta#operators)).
  * Change: _Implement custom parser for `TimeDelta`_ ([Issue #782](https://github.com/vorov2/dyalect/issues/782)).
  * Change: _Refactor `TimeDelta` formatting to a string_ ([Issue #781](https://github.com/vorov2/dyalect/issues/781)).
  * Change: _Disallow negative numbers on `TimeDelta` constructor_ ([Issue #786](https://github.com/vorov2/dyalect/issues/786)).
  * Change: _Parameter `predicate` of `Iterator.Count` method should be optional_ ([Issue #774](https://github.com/vorov2/dyalect/issues/774), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#count)).
  * Change: _Refactor `Console` class to better support colors_ ([Issue #775](https://github.com/vorov2/dyalect/issues/775), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Console)).
  * Change: _All foreign high order functions should accept objects with `Call` method as well as regular functions_ ([Issue #773](https://github.com/vorov2/dyalect/issues/773)).
  * Fix: _`ToLiteral` is not supported by the type `Set`_ ([Issue #768](https://github.com/vorov2/dyalect/issues/768)).
  * Fix: _`Set` is not recognized as a element sequence when you pass it to vararg function_ ([Issue #771](https://github.com/vorov2/dyalect/issues/771)).
  * Fix: _Refactoring of incorrect `Contains` built-in method implementation._
  * Fix: _Iterators may raise a NullReferenceException_ ([Issue #777](https://github.com/vorov2/dyalect/issues/777)).
  * Fix: _`Interop.GetMethod` can return incorrect method overload because of the error in logic_ ([Issue #780](https://github.com/vorov2/dyalect/issues/780)).

# 0.40.6
  * Fix: _Duplicate fields in `Tuple`_ ([Issue #764](https://github.com/vorov2/dyalect/issues/764)).
  * Fix: _It is not possible to invoke type members on a module instance directly_ ([Issue #767](https://github.com/vorov2/dyalect/issues/767)).

# 0.40.5
  * Change: _Various virtual machine and compiler optimizations._
  * Change: _Representation of modules as string is refactored and now contains more information._
  * Change: _Dump of variables in dya is rewritten and reformatted._
  * Change: _Errors generated in standard library now contain more information._
  * Fix: _Interactive mode is broken, globals are not refreshed between inputs_ ([Issue #761](https://github.com/vorov2/dyalect/issues/761)).
  * Fix: _Module import in interactive mode do not always work correctly_ ([Issue #762](https://github.com/vorov2/dyalect/issues/762)).

# 0.40.4
  * Change: _Runtime error messages are corrected for better clarity for `Interop` objects and external functions._
  * Change: _Optimizations for .Net interop_ ([Issue #746](https://github.com/vorov2/dyalect/issues/746)).
  * Add: _Add an ability to log assembly and module loading by linker_ ([Issue #751](https://github.com/vorov2/dyalect/issues/751)).
  * Fix: _Linker would look for .obj file first even for the startup module_ ([Issue #757](https://github.com/vorov2/dyalect/issues/757)).

# 0.40.3
  * Change: _Function call optimizations for simple cases._
  * Change: _Optimize initialization of a new `Set` with predefined values_ ([Issue #754](https://github.com/vorov2/dyalect/issues/754)).
  * Change: _Enhance Dyalect console interactive mode multiline entry_ ([Issue #755](https://github.com/vorov2/dyalect/issues/755)).
  * Fix: _Interactive environment keeps declared variable even in a case of exception_ ([Issue #749](https://github.com/vorov2/dyalect/issues/749)).

# 0.40.2
  * Fix: _Convertion of `LocalDateTime` to .NET type fails_ ([Issue #750](https://github.com/vorov2/dyalect/issues/750)).
  * Fix: _Unable to retrieve Dyalect assembly location under Linux_ ([Issue #748](https://github.com/vorov2/dyalect/issues/748)).

# 0.40.1
  * Change: _Parser errors corrected according to language changes._
  * Change: _Corrections and optimizations in standard library._
  * Change: _Parameters `year`, `month` and `day` in constructors for `DateTime` and `LocalDateTime` shouldn't be optional_ ([Issue #744](https://github.com/vorov2/dyalect/issues/744)).
  * Fix: _`LocalDateTime` is not correctly translated into .NET objects_ ([Issue #745](https://github.com/vorov2/dyalect/issues/745)).

# 0.40.0
  * Change: _Optimizations in string hashing._
  * Change: _Refactor and enhance type conversion between Dyalect and .Net_ ([Issue #738](https://github.com/vorov2/dyalect/issues/738)).
  * Add: _Implement .NET interoperability_ ([Issue #714](https://github.com/vorov2/dyalect/issues/714), [docs](https://github.com/vorov2/dyalect/wiki/Interop)).
  * Fix: _Tuple expansion in function call doesn't always work correctly_ ([Issue #742](https://github.com/vorov2/dyalect/issues/742)).

# 0.39.0
  * Add: _Add an ability to delete files_ ([Issue #733](https://github.com/vorov2/dyalect/issues/733), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.File#delete)).
  * Add: _Add an ability to Copy and Move files_ ([Issue #725](https://github.com/vorov2/dyalect/issues/725), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.File#copy)).
  * Add: _Add an ability to get and set file attributes_ ([Issue #734](https://github.com/vorov2/dyalect/issues/734), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.File#getAttributes)).
  * Add: _An ability to set/get various file related time stamps_ ([Issue #735](https://github.com/vorov2/dyalect/issues/735), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.File#getCreationTime)).
  * Add: _Add a new `Drive` type_ ([Issue #736](https://github.com/vorov2/dyalect/issues/736), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.Drive)).
  
# 0.38.1
  * Fix: _It is possible to specify one mixin several times_ ([Issue #729](https://github.com/vorov2/dyalect/issues/729)).
  * Fix: _It is possible to specify the type itself as a mixin_ ([Issue #730](https://github.com/vorov2/dyalect/issues/730)).
  * Fix: _It is possible to redefine module variable_ ([Issue #731](https://github.com/vorov2/dyalect/issues/731)).

# 0.38.0
  * Change: _Optimizations in virtual machine and type system refactoring._
  * Add: _Consider implementation of mixins_ ([Issue #724](https://github.com/vorov2/dyalect/issues/724), [docs](https://github.com/vorov2/dyalect/wiki/Mixins)).
  * Fix: _An empty catch statement may cause stack corruption_ ([Issue #727](https://github.com/vorov2/dyalect/issues/727)).

# 0.37.0
  * Change: _Refactor current `DateTime` implementation_ ([Issue #721](https://github.com/vorov2/dyalect/issues/721)).
  * Add: _Add `LocalDateTime` type_ ([Issue #720](https://github.com/vorov2/dyalect/issues/720), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.LocalDateTime)).
  * Add: _Consider adding `Calendar` type_ ([Issue #719](https://github.com/vorov2/dyalect/issues/719), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Calendar)).
  * Add: _Add `Today` method to `DateTime`_ ([Issue #722](https://github.com/vorov2/dyalect/issues/722), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.DateTime#today), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.LocalDateTime#today)).
  
# 0.36.1
  * Change: _Exception generation logic is corrected in Dyalect standard library._
  * Change: _Standard function assert should specify a type of an operand when assert fails_ ([Issue #713](https://github.com/vorov2/dyalect/issues/713)).
  * Change: _`DateTime` data type corrections._

# 0.36.0
  * Change: _Standard method `ToString` should accept an optional `format` parameter_ ([Issue #706](https://github.com/vorov2/dyalect/issues/706)).
  * Change: _Field check pattern is not overridable_ ([Issue #710](https://github.com/vorov2/dyalect/issues/710)).
  * Add: _Add `Console.ReadKey` method_ ([Issue #698](https://github.com/vorov2/dyalect/issues/698), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Console#readKey)).
  * Add: _No introspection on a constructor name_ ([Issue #680](https://github.com/vorov2/dyalect/issues/680), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#constructorName)).
  * Add: _Add a new `TimeDelta` type_ ([Issue #705](https://github.com/vorov2/dyalect/issues/705), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.TimeDelta)).
  * Add: _Library: file IO_ ([Issue #329](https://github.com/vorov2/dyalect/issues/329), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.File)).
  * Add: _Add a `Path` type with static methods to deal with paths_ ([Issue #709](https://github.com/vorov2/dyalect/issues/709), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.Path)).
  * Add: _Library: `DateTime` data type_ ([Issue #332](https://github.com/vorov2/dyalect/issues/332), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.DateTime)).
  * Add: _Add a `Directory` type with an ability to create new directories_ ([Issue #711](https://github.com/vorov2/dyalect/issues/711), [docs](https://github.com/vorov2/dyalect/wiki/Library.IO.Directory)).
  * Fix: _It is possible to override built-in with a wrong number of arguments_ ([Issue #707](https://github.com/vorov2/dyalect/issues/707)).
  * Fix: _A getter and setter can have any number of arguments_ ([Issue #708](https://github.com/vorov2/dyalect/issues/708)).

# 0.35.10
  * Change: _Refactoring of unit test module in Dyalect Console._
  * Add: _Add support for test result generation in Markdown_ ([Issue #702](https://github.com/vorov2/dyalect/issues/702)).
  * Fix: _Options are not correctly displayed in Dyalect Console_ ([Issue #703](https://github.com/vorov2/dyalect/issues/703)).

# 0.35.9
  * Fix: _Passing a tuple to a vararg function_ ([Issue #700](https://github.com/vorov2/dyalect/issues/700)).
  * Change: _Dyalect console (dya) no longer has a reference to the Dyalect standard library and is using standard code for JSON parsing._

# 0.35.8
  * Change: _Virtual machine and type system optimizations._
  * Fix: _Variadic function invocation error_ ([Issue #696](https://github.com/vorov2/dyalect/issues/696)).
  * Fix: _Variadic function should preserve tuple labels_ ([Issue #697](https://github.com/vorov2/dyalect/issues/697)).

# 0.35.7
  * Fix: _Several runtime error messages are corrected for better clarity._
  * Fix: _Getting an item by index from some built-in type causes unnecessary conversions_ ([Issue #691](https://github.com/vorov2/dyalect/issues/691)).
  * Fix: _Array created from tuple reuses the same internal array_ ([Issue #692](https://github.com/vorov2/dyalect/issues/692)).
  * Fix: _Methods `Array.Reverse` and `Array.SortBy` can work incorrectly_ ([Issue #694](https://github.com/vorov2/dyalect/issues/694)).
  * Fix: _Array.GetValues returns an internal array with nulls instead of a safe copy_ ([Issue #693](https://github.com/vorov2/dyalect/issues/693)).

# 0.35.6
  * Change: _Virtual machine optimizations._
  * Fix: _Several runtime error messages are corrected for better clarity._
  * Fix: _Equality and hash code calculation is not always correctly implemented for standard types_ ([Issue #688](https://github.com/vorov2/dyalect/issues/688)).
  * Fix: _Function `abs` is not generic_ ([Issue #689](https://github.com/vorov2/dyalect/issues/689)).

# 0.35.5
  * Change: _Minor optimizations in Dyalect runtime._
  * Add: _Add `abs` built-in function_ ([Issue #684](https://github.com/vorov2/dyalect/issues/684), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#abs)).
  
# 0.35.4
  * Fix: _Incorrect inference of implicit lambda parameters_ ([Issue #676](https://github.com/vorov2/dyalect/issues/676)).

# 0.35.3
  * Add: _Consider `rawget` and `rawset` built-in functions_ ([Issue #671](https://github.com/vorov2/dyalect/issues/671), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#rawget)).
  * Fix: _VM may crush if a custom implementation of `Iterate` returns a type other than `Iterator`_ ([Issue #678](https://github.com/vorov2/dyalect/issues/678)).
  * Fix: _Virtual machine crush_ ([Issue #681](https://github.com/vorov2/dyalect/issues/681)).
  * Fix: _A function nested in an iterator can be incorrectly treated as iterator_ ([Issue #683](https://github.com/vorov2/dyalect/issues/683)).
  * Fix: _Unable to return iterator from custom `Iterate` function_ ([Issue #682](https://github.com/vorov2/dyalect/issues/682)).

# 0.35.2
  * Fix: _Console can fail if command line options duplicate options from config_ ([Issue #672](https://github.com/vorov2/dyalect/issues/672)).

# 0.35.1
  * Fix: _Mutable variant declaration do not work as expected_ ([Issue #667](https://github.com/vorov2/dyalect/issues/667)).
  * Fix: _It is possible to use `var` modifier in a function call_ ([Issue #668](https://github.com/vorov2/dyalect/issues/668)).
  * Fix: _All unresolved constructors becomes variants_ ([Issue #669](https://github.com/vorov2/dyalect/issues/669)).

# 0.35.0
  * Add: _Consider shortcut syntax for parameterless lambda_ ([Issue #591](https://github.com/vorov2/dyalect/issues/591), [docs](https://github.com/vorov2/dyalect/wiki/Functions#lambdas)).
  * Add: _Consider adding `Function.Apply` method_ ([Issue #660](https://github.com/vorov2/dyalect/issues/660), [docs](https://github.com/vorov2/dyalect/wiki/Functions#apply)).
  * Add: _Function application operators_ ([Issue #105](https://github.com/vorov2/dyalect/issues/105), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#apps)).
  * Change: _Consider changing `Iterator.Reduce` signature_ ([Issue #659](https://github.com/vorov2/dyalect/issues/659), [docs](https://github.com/vorov2/dyalect/wiki/Iterators#reduce)).
  * Change: _Redesign lazy bindings implementation_ ([Issue #663](https://github.com/vorov2/dyalect/issues/663), [docs](https://github.com/vorov2/dyalect/wiki/Variables#lazy)).
  * Change: _Redesign conversion to booleans_ ([Issue #664](https://github.com/vorov2/dyalect/issues/664)).
  * Fix: _Correct grammar for labels/names function arguments_ ([Issue #658](https://github.com/vorov2/dyalect/issues/658)).
  * Fix: _`base` allows to assign a constant_ ([Issue #662](https://github.com/vorov2/dyalect/issues/662)).

# 0.34.1
  * Change: _Errors generated by parser are corrected after language changes._
  * Fix: _Correct grammar for `throw` expression_ ([Issue #655](https://github.com/vorov2/dyalect/issues/655)).

# 0.34.0
  * Change: _`Console.SetOutput` method corrected._
  * Change: _Consider replacing `Error` type with a `Variant` type_ ([Issue #653](https://github.com/vorov2/dyalect/issues/653)).
  * Add: _Consider adding variant type_ ([Issue #652](https://github.com/vorov2/dyalect/issues/652), [docs](https://github.com/vorov2/dyalect/wiki/Variant)).

# 0.33.0
  * Add: _Create foreign types from other foreign modules_ ([Issue #644](https://github.com/vorov2/dyalect/issues/644)).
  * Add: _`StringBuilder`_ ([Issue #22](https://github.com/vorov2/dyalect/issues/22), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.StringBuilder)).
  * Add: _Add a `Result` type_ ([Issue #391](https://github.com/vorov2/dyalect/issues/391), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Result)).
  * Add: _Add `Guid` type_ ([Issue #606](https://github.com/vorov2/dyalect/issues/606), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Guid)).
  * Add: _Library: regular expressions_ ([Issue #330](https://github.com/vorov2/dyalect/issues/330), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Regex)).
  * Add: _Library: `ByteArray` data type_  ([Issue #333](https://github.com/vorov2/dyalect/issues/333), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.ByteArray)).
  * Add: _Support for casting dictionary to a tuple with named elements_ ([Issue #647](https://github.com/vorov2/dyalect/issues/647)).
  * Add: _Add an ability to save test results to a file_ ([Issue #648](https://github.com/vorov2/dyalect/issues/648), [docs](https://github.com/vorov2/dyalect/wiki/Dyalect-consoles#switches)).
  * Add: _Add a module to work with console input/output_ ([Issue #255](https://github.com/vorov2/dyalect/issues/255), [docs](https://github.com/vorov2/dyalect/wiki/Library.Core.Console)).
  * Fix: _Type related error messages can be incorrectly displayed for custom types_ ([Issue #645](https://github.com/vorov2/dyalect/issues/645)).
  * Fix: _It is not possible to create foreign static properties_ ([Issue #649](https://github.com/vorov2/dyalect/issues/649)).

# 0.32.2
  * Fix: _Type might not be correctly recognized in `as` expression_ ([Issue #640](https://github.com/vorov2/dyalect/issues/640)).
  * Fix: _Type might not be correctly recognized in `as` function_ ([Issue #637](https://github.com/vorov2/dyalect/issues/637)).
  * Fix: _Incorrect compilation of indexer with long type name_ ([Issue #638](https://github.com/vorov2/dyalect/issues/638)).
  * Fix: _Field `code` of `DyTypeInfo` returns incorrect value_ ([Issue #642](https://github.com/vorov2/dyalect/issues/642)).
  * Fix: _It is possible to override `Name` and `Code` properties of `DyTypeInfo`_ ([Issue #641](https://github.com/vorov2/dyalect/issues/641)).

# 0.32.1
  * Fix: _Virtual machine stack corruption_ ([Issue #633](https://github.com/vorov2/dyalect/issues/633)).
  * Fix: _Inconsistent boolean conversions_ ([Issue #632](https://github.com/vorov2/dyalect/issues/632)).
  * Fix: _Incorrect behavior of a conversion to boolean_ ([Issue #635](https://github.com/vorov2/dyalect/issues/635)).
  * Fix: _Type is not correctly recognized in custom conversions_ ([Issue #636](https://github.com/vorov2/dyalect/issues/636)).
  * Fix: _Conversion of type to itself_ ([Issue #634](https://github.com/vorov2/dyalect/issues/634)).

# 0.32.0
  * Change: _Virtual machine optimizations._
  * Change: _Parser refactoring and optimization._
  * Change: _Rethink guards_ ([Issue #605](https://github.com/vorov2/dyalect/issues/605)).
  * Change: _`String.Join` should accept char as separator_ ([Issue #611](https://github.com/vorov2/dyalect/issues/611)).
  * Change: _Refactor error messages raised by standard types for better clarity_ ([Issue #612](https://github.com/vorov2/dyalect/issues/612)).
  * Change: _Refactor parameter names in standard functions for better consistency_ ([Issue #613](https://github.com/vorov2/dyalect/issues/613)).
  * Change: _Allow integers to pass Float type restriction_ ([Issue #617](https://github.com/vorov2/dyalect/issues/617)).
  * Change: _Simplify dictionary creation_ ([Issue #621](https://github.com/vorov2/dyalect/issues/621), [docs](https://github.com/vorov2/dyalect/wiki/Dictionary#Dictionary)).
  * Change: _Correct operation names in error messages_ ([Issue #625](https://github.com/vorov2/dyalect/issues/625)).
  * Change: _Make error message for "not supported operations" more clear_ ([Issue #627](https://github.com/vorov2/dyalect/issues/627)).
  * Add: _Add `String.Format` method_ ([Issue #603](https://github.com/vorov2/dyalect/issues/603), [docs](https://github.com/vorov2/dyalect/wiki/String#format)).
  * Add: _Add methods `Dictionary.ContainsValue` and `Dictionary.GetAndRemove`_ ([Issue #608](https://github.com/vorov2/dyalect/issues/608), [docs](https://github.com/vorov2/dyalect/wiki/Dictionary#containsValue)).
  * Add: _Add a universal `ToLiteral` method_ ([Issue #607](https://github.com/vorov2/dyalect/issues/607)).
  * Add: _Add `Tuple.ToArray` method_ ([Issue #609](https://github.com/vorov2/dyalect/issues/609), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#toArray)).
  * Add: _Add `Tuple.Compact` method_ ([Issue #615](https://github.com/vorov2/dyalect/issues/615), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#compact)).
  * Add: _Add an ability to specify an optional predicate for an `Array.Compact`_ ([Issue #616](https://github.com/vorov2/dyalect/issues/616), [docs](https://github.com/vorov2/dyalect/wiki/Array#compact)).
  * Add: _Allow multiple type annotations for a single parameter_ ([Issue #618](https://github.com/vorov2/dyalect/issues/618), [docs](https://github.com/vorov2/dyalect/wiki/Functions#anno)).
  * Add: _Implement special notation for passing a tuple names elements to a function_ ([Issue #622](https://github.com/vorov2/dyalect/issues/622), [docs](https://github.com/vorov2/dyalect/wiki/Functions#variadic_functions)).
  * Add: _An ability to specify error message for `assert` function_ ([Issue #624](https://github.com/vorov2/dyalect/issues/624), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#assert)).
  * Add: _Consider comparison operations for tuples_ ([Issue #626](https://github.com/vorov2/dyalect/issues/626), [docs](https://github.com/vorov2/dyalect/wiki/Tuple#operators)).
  * Add: _Initialization blocks for types_ ([Issue #628](https://github.com/vorov2/dyalect/issues/628), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types#inits)).
  * Fix: _Correct errors for undefined types_ ([Issue #610](https://github.com/vorov2/dyalect/issues/610)).
  * Fix: _Test runner doesn't display file name in "onlyfailed" display mode_ ([Issue #614](https://github.com/vorov2/dyalect/issues/614)).
  * Fix: _`Dictionary.ToLiteral` is not implemented correctly_ ([Issue #623](https://github.com/vorov2/dyalect/issues/623)).
  * Fix: _`yield` outside of function can cause parser to crush_ ([Issue #629](https://github.com/vorov2/dyalect/issues/629)).

# 0.31.2
  * _Optimizations and cleanups in Dyalect console and Dyalect parser._
  * _Optimizations and corrections in parser error reporting._

# 0.31.1
  * _Minor optimizations in Dyalect console._
  * Fix: _Problems in Dyalect grammar_ ([Issue #601](https://github.com/vorov2/dyalect/issues/601)).

# 0.31.0
  * Change: _Methods such as Trim should accept both strings and chars_ ([Issue #598](https://github.com/vorov2/dyalect/issues/598)).
  * Add: _Add a `Lazy` type_ ([Issue #551](https://github.com/vorov2/dyalect/issues/551), [docs](https://github.com/vorov2/dyalect/wiki/Variables#lazy)).
  * Add: _Consider implementing generic conversion routines_ ([Issue #506](https://github.com/vorov2/dyalect/issues/506), [docs](https://github.com/vorov2/dyalect/wiki/Standard-operators#as)).
  * Fix: _Incorrect `ToString` implementation for dictionaries_ ([Issue #599](https://github.com/vorov2/dyalect/issues/599)).

# 0.30.1
  * Fix: _VM can crush when invoking functions from foreign modules_ ([Issue #595](https://github.com/vorov2/dyalect/issues/595)).
  * Fix: _Last error can be overwritten in an execution context_ ([Issue #596](https://github.com/vorov2/dyalect/issues/596)).

# 0.30.0
  * Change: _Referencing modules from stdlib_ ([Issue #502](https://github.com/vorov2/dyalect/issues/502)).
  * Add: _Simplify tuple construction from C# code_ ([Issue #588](https://github.com/vorov2/dyalect/issues/588)).
  * Add: _Add an ability for foreign modules to reference other foreign modules_ ([Issue #590](https://github.com/vorov2/dyalect/issues/590)).
  * Add: _Properties are not supported for foreign functions_ ([Issue #568](https://github.com/vorov2/dyalect/issues/568)).
  * Fix: _Overloading of methods and properties consistency_ ([Issue #592](https://github.com/vorov2/dyalect/issues/592)).
  * Fix: _Disallow guards on bindings_ ([Issue #564](https://github.com/vorov2/dyalect/issues/564)).
  * Fix: _Compiler fails if a module is referenced twice_ ([Issue #593](https://github.com/vorov2/dyalect/issues/593)).

# 0.29.2
  * Fix: _Regression for foreign module import_ ([Issue #587](https://github.com/vorov2/dyalect/issues/587)). 

# 0.29.1
  * Fix: _Uppercase labels for tuples shouldn't be allowed_ ([Issue #583](https://github.com/vorov2/dyalect/issues/583)).
  * Fix: _Custom errors are not correctly serialized to string_ ([Issue #584](https://github.com/vorov2/dyalect/issues/584)).
  * Fix: _Stack overflow detection can incorrectly break execution_ ([Issue #585](https://github.com/vorov2/dyalect/issues/585)).

# 0.29.0
  * Change: _Global refactoring of Dyalect internals and type system_ ([Issue #569](https://github.com/vorov2/dyalect/issues/569)).
  * Change: _Change field access syntax_ ([Issue #571](https://github.com/vorov2/dyalect/issues/571)).
  * Change: _Use member access syntax for module prefixes_ ([Issue #577](https://github.com/vorov2/dyalect/issues/577)).
  * Change: _Use private modified to declare private constructors_ ([Issue #575](https://github.com/vorov2/dyalect/issues/575)).
  * Change: _All built-in methods should start with a capital letter_ ([Issue #576](https://github.com/vorov2/dyalect/issues/576)).
  * Change: _Rename methods `len` and `iter` to `Length` and `Iterate`_ ([Issue #574](https://github.com/vorov2/dyalect/issues/574)).
  * Change: _Rename tuple methods fst and snd to First and Second_ ([Issue #573](https://github.com/vorov2/dyalect/issues/573)).
  * Change: _A bug in Dyalect console in processing of boolean command line switches_ ([Issue #572](https://github.com/vorov2/dyalect/issues/572)).
  * Change: _Exception generation redesign_ ([Issue #513](https://github.com/vorov2/dyalect/issues/513)).
  * Change: _Change syntax for indexer definition_ ([Issue #578](https://github.com/vorov2/dyalect/issues/578)).
  * Change: _Method check pattern should accept operators_ ([Issue #579](https://github.com/vorov2/dyalect/issues/579)).
  * Change: _Simplify object file format_ ([Issue #580](https://github.com/vorov2/dyalect/issues/580)).
  * Fix: _Dya: Whole test suite fails because of the parser error_ ([Issue #581](https://github.com/vorov2/dyalect/issues/581)).
  * Fix: _Allow to declare types only in global scope_ ([Issue #570](https://github.com/vorov2/dyalect/issues/570)).

# 0.28.0
  * Change: _Rethink Dyalect type system_ ([Issue #563](https://github.com/vorov2/dyalect/issues/563), [docs](https://github.com/vorov2/dyalect/wiki/Custom-types)).
  * Fix: _VM may crush in some cases with stack overflow exception_ ([Issue #566](https://github.com/vorov2/dyalect/issues/566)).
  * Add: _Add standard `setOutput` function_ ([Issue #554](https://github.com/vorov2/dyalect/issues/554), [docs](https://github.com/vorov2/dyalect/wiki/Standard-functions#setOut)).