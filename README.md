[![Build](https://github.com/vorov2/dyalect/actions/workflows/dotnet.yml/badge.svg)](https://github.com/vorov2/dyalect/actions/workflows/dotnet.yml)
[![Tests](https://img.shields.io/badge/Tests-passing-33CB56?style=flat&logo=pytest&labelColor=2E343A&logoColor=959DA5)](https://github.com/vorov2/dyalect/blob/master/Docs/TestResult.md)

# Dyalect programming language 

:blue_book: [Quick start guide](https://github.com/vorov2/dyalect/wiki/Language-overview)

[![GitHub tag (latest SemVer)](https://img.shields.io/badge/Download-0.38.0-blue?style=for-the-badge&logo=github)](https://github.com/vorov2/dyalect/releases/latest)

Dyalect is a dynamic programming language for .NET platform.
It is lightweight, fast and modern. Dyalect (or Dy for short) is
written in C# and has zero dependencies except for standard .NET
libraries, which means that it can seamlessly run on Windows, MacOS
and Linux. Moreover you can use the same binaries on any of these 
platforms!

Dy doesn't utilize DLR nor does it compile to IL (.NET assembly). Instead
it runs on the top of its own high performance virtual machine. It
compiles fast and can be used as an embeddable language or as a
scripting language of your choice. It is also a good language to learn
programming.

Dyalect offers modern syntax, inspired by such languages as C#, Swift,
Go and Rust, first class functions, coroutines, expressive modules,
a dynamic type system with an ability to extend existing types with
new functions and much more.

A taste of Dy:

```swift
func fib(n) {
    n < 2 ? n : fib(n - 1) + fib(n - 2)
}

//Calculate the n-th fibonacci number
fib(11) 
```

Extending standard types:

```swift
func Float.Pow(n) {
    var result = 1.0

    for i in 1..n {
        result *= this
    } when n > 0

    for i in -1..n {
        result /= this
    } when n < 0

    result
}


20.12.Pow(3) //Output: 8144.865728
```

And a small example with iterators:

```swift
func fetch() { 
    yield "Hello, world!"
    yield 22 * 1.25
}

for x in fetch() {
    print(x)
}
```

Output:

```
Hello, world!
27.5
```

Dy is shipped with a crossplatform interactive console which can
help you to familiarize yourself with the language.

Please refer to [wiki](https://github.com/vorov2/dyalect/wiki) for more information.

## Links

* [SourceForge page](https://sourceforge.net/projects/dyalect/)
* [Dyalect at RosettaCode](http://rosettacode.org/wiki/Category:Dyalect)

