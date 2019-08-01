# Dyalect ![GitHub tag (latest SemVer)](https://img.shields.io/badge/version-0.9-blue.svg)

[![Build status](https://ci.appveyor.com/api/projects/status/lu26t16of7nhetp0?svg=true)](https://ci.appveyor.com/project/vorov2/dyalect)
![AppVeyor tests](https://img.shields.io/appveyor/tests/vorov2/dyalect.svg)

Dyalect is a dynamic programming language for .NET Core platform.
It is lightweight, fast and modern. Dyalect (or Dy for short) is
written in C# and has zero dependencies except for standard .NET Core
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
    if n < 2 {
        return n
    }
    fib(n - 1) + fib(n - 2)
}

//Calculate the n-th fibonacci number
fib(11)
```

And a small example with iterators (coroutines):

```swift
func fetch() { 
    yield 22 * 1.25
    yield "Hello, world!"
    yield (1,2,3)
    yield true  
}

for x in fetch() {
    print(x)
}
```

Outputs:

```
27.5
Hello, world!
(1, 2, 3)
true
```

Dy is shipped with a crossplatform interactive console which can
help you to familiarize yourself with the language.

The project is still in early development, however you can already
download the binaries or study existing code.

Please refer to [wiki](https://github.com/vorov2/dyalect/wiki) for more information.

## Links

* [SourceForge page](https://sourceforge.net/projects/dyalect/)
* [Dyalect at RosettaCode](http://rosettacode.org/wiki/Category:Dyalect)
* [Dyalect at OpenHub](https://www.openhub.net/p/dyalect)
