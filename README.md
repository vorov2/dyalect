# Dyalect

Dyalect is a dynamic programming language for .NET Core platform.
It is lightweight, fast and modern. Dyalect (or Dy for short) is
written in C# and has zero dependencies except for standard .NET Core
libraries, which means that it can seamlessly run on Windows, MacOS
and Linux. Moreover you can run the same binaries on any of these 
operative systems. Simply type `dotnet dya` in your terminal to run
Dy's interactive environment.

Dy doesn't use DLR nor does it compile to IL (.NET assembly). Instead
it runs on the top of its own high performance virtual machine. It
compiles fast and can be used as an embeddable language or as a
scripting language of your choice.

Dyalect offers modern syntax, inspired by such languages as C#, Swift,
Go and Rust, first class functions, expressive modules, a dynamic type
system with an ability to extend existing types with new functions and
much more. 

A taste of Dy:

```
func fib(n) {
    if n < 2 {
        return n
    }
    fib(n - 1) + fib(n - 2)
}

//Calculate the n-th fibonacci number
fib(11)
```

Dy is shipped with Dya, a crossplatform interactive console which can
help you to familiarize yourself with the language.

The project is still in early development, however you can already
download the binaries or study existing source code.
