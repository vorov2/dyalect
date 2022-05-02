using Dyalect.Codegen;
using Dyalect.Compiler;
using Dyalect.Parser;
using Dyalect.Runtime;
using Dyalect.Runtime.Types;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace Dyalect.Linker;

[GeneratedModule]
internal sealed partial class Lang : ForeignUnit
{
    private readonly DyTuple? startupArguments;
    private const string VAR_CONSOLEOUTPUT = "sys.ConsoleOutput";

    public Lang() : this(null) { }

    public Lang(DyTuple? args)
    {
        FileName = "lang";
        startupArguments = args;
    }

    protected override void Execute(ExecutionContext ctx) => Add("args", startupArguments ?? Nil);

    [StaticMethod("referenceEquals")]
    internal static bool Equals(DyObject value, DyObject other) => ReferenceEquals(value, other);

    [StaticMethod("print")]
    internal static void Print(ExecutionContext ctx, [VarArg] DyTuple values, [Default(",")]string separator, [Default("\n")]DyObject terminator)
    {
        var fst = true;
        
        foreach (var a in values)
        {
            if (!fst && !string.IsNullOrEmpty(separator))
                Console.Write(separator);

            if (a.Is(Dy.String))
                Console.Write(a.GetString());
            else
                Console.Write(a.ToString(ctx));

            fst = false;

            if (ctx.Error is not null)
                break;
        }

        if (terminator.TypeId is Dy.String)
            Console.Write(terminator.GetString());
    }

    [StaticMethod("setOut")]
    internal static void SetOutput(ExecutionContext ctx, DyObject? output = null)
    {
        if (output is null)
        {
            var outputWriter = ctx.GetContextVariable<TextWriter>(VAR_CONSOLEOUTPUT);
            if (outputWriter is not null)
                Console.SetOut(outputWriter);
        }
        else
        {
            if (!ctx.HasContextVariable(VAR_CONSOLEOUTPUT))
                ctx.SetContextVariable(VAR_CONSOLEOUTPUT, Console.Out);

            Console.SetOut(new ConsoleTextWriter(ctx, output));
        }
    }

    [StaticMethod("constructorName")]
    internal static string? GetConstructorName(DyObject value) => value is IConstructor c ? c.Constructor : null;

    [StaticMethod("typeName")]
    internal static string GetTypeName(DyObject value)
    {
        if (value.Is(Dy.TypeInfo))
            return ((DyTypeInfo)value).ReflectedTypeName;
        else
            return value.TypeName;
    }

    [StaticMethod("rawget")]
    internal static DyObject RawGet(ExecutionContext ctx, DyObject values, DyInteger index) =>
        ctx.RuntimeContext.Types[values.TypeId].GetDirect(ctx, values, index);

    [StaticMethod("rawset")]
    internal static void RawSet(ExecutionContext ctx, DyObject values, DyInteger index, DyObject value) =>
        ctx.RuntimeContext.Types[values.TypeId].SetDirect(ctx, values, index, value);

    [StaticMethod("caller")]
    internal static DyObject Caller(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 2)
        {
            var cp = ctx.CallStack[ctx.CallStack.Count - 2];
            if (!ReferenceEquals(cp, global::Dyalect.Runtime.Caller.External))
                return cp.Function;
        }

        return Nil;
    }

    [StaticMethod("current")]
    internal static DyObject Current(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 1)
            return ctx.CallStack.Peek().Function;

        return Nil;
    }

    [StaticMethod("readLine")]
    internal static string Read() => Console.ReadLine() ?? "";

    [StaticMethod("rnd")]
    internal static int Randomize(ExecutionContext ctx, int min = 0, int max = int.MaxValue, int? seed = null)
    {
        if (seed is null)
        {
            var dt = DateTime.UtcNow;
            var dt2 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            seed = (int)(dt2 - dt).Ticks;
        }

        if (min > max)
        {
            ctx.InvalidValue("min", "max");
            return default;
        }

        var rnd = new Random(seed.Value);
        return rnd.Next(min, max);
    }

    [StaticMethod("assert")]
    internal static void Assert(ExecutionContext ctx, [Default(true)]DyObject expect, DyObject got, string? errorText = null)
    {
        if (!Eq(ctx, expect.ToObject(), got.ToObject()))
        {
            if (errorText is not null)
                ctx.AssertionFailed(errorText);
            else
                ctx.AssertionFailed($"Expected \"{expect.ToString(ctx)}\" :: {expect.TypeName}, got \"{got.ToString(ctx)}\" :: {got.TypeName}.");
        }
    }

    private static bool Eq(ExecutionContext ctx, object? x, object? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (x is string a && y is string b)
        {
            a = a.Replace("\r\n", "\n");
            b = b.Replace("\r\n", "\n");
            return Equals(a, b);
        }

        if (x is IList xs && y is IList ys)
        {
            if (xs.Count != ys.Count)
                return false;

            for (var i = 0; i < xs.Count; i++)
                if (!Eq(ctx, xs[i], ys[i]))
                    return false;

            return true;
        }

        if (x is DyObject xa && y is DyObject ba)
            return xa.Equals(ba, ctx);

        return Equals(x, y);
    }

    [StaticMethod("sqrt")]
    internal static double Sqrt(double x) => Math.Sqrt(x);

    [StaticMethod("pow")]
    internal static double Pow(double x, double y) => Math.Pow(x, y);

    [StaticMethod("min")]
    internal static DyObject Min(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Lesser(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("max")]
    internal static DyObject Max(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Greater(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("abs")]
    internal static DyObject Abs(ExecutionContext ctx, DyObject value)
    {
        if (value.Lesser(DyInteger.Zero, ctx))
            return value.Negate(ctx);

        return value;
    }

    [StaticMethod("round")]
    internal static double Round(double number, int digits = 2) => Math.Round(number, digits);
    
    [StaticMethod("sign")]
    internal static DyObject Sign(ExecutionContext ctx, DyObject x)
    {
        if (ReferenceEquals(x, DyInteger.Zero)) 
            return DyInteger.Zero;

        if (x.Lesser(DyInteger.Zero, ctx))
            return DyInteger.MinusOne;
        
        return DyInteger.One;
    }

    [StaticMethod("parse")]
    internal static DyObject Parse(ExecutionContext ctx, string expression)
    {
        var res = DyParser.Parse(SourceBuffer.FromString(expression));

        if (!res.Success)
            return ctx.ParsingFailed(res.Messages.First().ToString());

        if (res.Value!.Root is null || res.Value!.Root.Nodes.Count == 0)
            return ctx.ParsingFailed("Empty expression.");
        else if (res.Value!.Root.Nodes.Count > 1)
            return ctx.ParsingFailed("Only single expressions allowed.");

        return LiteralEvaluator.Eval(res.Value!.Root.Nodes[0]);
    }

    [StaticMethod("eval")]
    internal static DyObject Eval(string source, DyTuple? args = null)
    {
        var sb = new StringBuilder();
        sb.Append("func __x12(");
        
        if (args is not null)
        {
            var tv = args.UnsafeAccessValues();
            
            for (var i = 0; i < args.Count; i++)
            {
                var o = tv[i];

                if (o is not DyLabel lab)
                    continue;
                
                if (i > 0)
                    sb.Append(',');

                sb.Append(lab.Label);
            }
        }

        sb.Append("){");
        sb.Append(source);
        sb.Append('}');
        sb.Append("__x12");

        var linker = new DyLinker(FileLookup.Default, BuilderOptions.Default());
        var result = linker.Make(SourceBuffer.FromString(sb.ToString()));

        if (!result.Success)
            throw new DyBuildException(result.Messages);

        var newctx = DyMachine.CreateExecutionContext(result.Value!);
        var result2 = DyMachine.Execute(newctx);
        var func = result2.Value!;
        var argsList = new List<DyObject>();

        if (args is null)
            return func!.Invoke(newctx, argsList.ToArray());
        
        var tvv = args.UnsafeAccessValues();

        for (var i = 0; i < args.Count; i++)
        {
            var o = tvv[i];

            if (o is DyLabel lab)
                argsList.Add(lab.Value);
        }

        return func!.Invoke(newctx, argsList.ToArray());
    }

    [StaticMethod("__invoke")]
    internal static DyObject Invoke(ExecutionContext ctx, DyObject functor, params DyObject[] values) => functor.Invoke(ctx, values);
}
