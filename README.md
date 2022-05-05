[![Build](https://github.com/vorov2/dyalect/actions/workflows/dotnet.yml/badge.svg)](https://github.com/vorov2/dyalect/actions/workflows/dotnet.yml)
[![Tests](https://img.shields.io/badge/Tests-passing-33CB56?style=flat&logo=pytest&labelColor=2E343A&logoColor=959DA5)](https://github.com/vorov2/dyalect/blob/master/Docs/TestResult.md)

# Dyalect programming language 

[![GitHub tag (latest SemVer)](https://img.shields.io/badge/Download-0.44.4-blue?style=for-the-badge&logo=github)](https://github.com/vorov2/dyalect/releases/latest)

Dyalect is a dynamic programming language for .NET platform.
It is lightweight, fast and modern. Dyalect (or Dy for short)
supports Windows, MacOS and Linux.

Dy runs on the top of its own high performance virtual machine. It
compiles fast and can be used as an embeddable language or as a
scripting language of your choice. It is also a good language to learn
programming.

Dyalect offers modern syntax, inspired by such languages as C#, Swift,
Go and Rust, first class functions, coroutines, expressive modules,
a dynamic type system with an ability to extend existing types with
new methods and much more.

A taste of Dy:

```swift
func fib(n) {
    func fib(a = 0, b = 1, c) {
        return a when c is 0
        fib(b, a + b, c - 1)
    }
    fib(c: n)
}

//Calculate the n-th fibonacci number
fib(50) 
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

Dy is shipped with a CLI (command line interface) which can
help you to familiarize yourself with the language.

Please refer to [wiki](https://github.com/vorov2/dyalect/wiki) for more information.

## Links

* [Quick start guide](https://github.com/vorov2/dyalect/wiki/Language-overview)
* [SourceForge page](https://sourceforge.net/projects/dyalect/)
* [Dyalect at RosettaCode](http://rosettacode.org/wiki/Category:Dyalect)

