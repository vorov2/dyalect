﻿using Dyalect.Compiler;
using Dyalect.Linker;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Linq;

namespace Dyalect;

public static class Exe
{
    public static DyObject? Eval(SourceBuffer buffer, BuilderOptions options, FileLookup lookup, object? args = null)
    {
        DyTuple? tup = null;

        if (args is not null)
        {
            var arr = args.GetType().GetProperties().Select(pi => 
                new DyLabel(pi.Name, TypeConverter.ConvertFrom(pi.GetValue(args)))).ToArray();
            tup = new DyTuple(arr);
        }

        var linker = new DyLinker(lookup ?? FileLookup.Default, options ?? BuilderOptions.Default(), tup);
        var result = linker.Make(buffer);

        if (!result.Success)
            throw new DyBuildException(result.Messages);

        var ctx = DyMachine.CreateExecutionContext(result.Value!);
        var result2 = DyMachine.Execute(ctx);
        return result2.Value;
    }

    public static DyObject? Eval(SourceBuffer buffer, object? args = null) =>
        Eval(buffer, FileLookup.Default.BuilderOptions, FileLookup.Default, args);
}
