# Dyalect for Visual Studio Code
This extension adds a basic support for a Dyalect programming language (https://github.com/vorov2/dyalect.git).

Dyalect is a dynamic programming language for .NET Core platform. It is lightweight, fast and modern. Dyalect (or Dy for short) is written in C# and has zero dependencies except for standard .NET Core libraries, which means that it can seamlessly run on Windows, MacOS and Linux. Moreover you can use the same binaries on any of these platforms!

Dy doesn't utilize DLR nor does it compile to IL (.NET assembly). Instead it runs on the top of its own high performance virtual machine. It compiles fast and can be used as an embeddable language or as a scripting language of your choice. It is also a good language to learn programming.

Dyalect offers modern syntax, inspired by such languages as C#, Swift, Go and Rust, first class functions, coroutines, expressive modules, a dynamic type system with an ability to extend existing types with new functions and much more.

A taste of Dy:

```
func fib(n) {
    return n when n < 2
    fib(n - 1) + fib(n - 2)
}

//Calculate the n-th fibonacci number
fib(11)
```

## Features

This extension provides highlighting support and the following commands (available in editor context menu and in command palette):
* `Eval file` (evaluates currently opened file and prints its output to console)
* `Compile file` (compiles currently opened file into an object file)

## Extension Settings

* `dyalect.path` (path to the Dyalect installation directory)
* `dyalect.disableOptimizations` (disable all compiler optimizations)
* `dyalect.disableCompilerWarnings` (disable compiler warnings)
* `dyalect.disableLinkerWarnings` (disable linker warnings)
* `dyalect.lookupPaths` (a list of directories where linker should look for referenced modules)