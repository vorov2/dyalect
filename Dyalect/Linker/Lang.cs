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

    public Lang() => FileName = "lang";

    public Lang(DyTuple? args) : this() => startupArguments = args;

    protected override void Execute(ExecutionContext ctx) => Add("args", startupArguments ?? Nil);

    [StaticMethod("mixins")]
    public static DyObject[] GetMixins(ExecutionContext ctx, DyObject value)
    {
        var ti = value.GetTypeInfo(ctx);
        return ti.GetMixins().Select(i => ctx.RuntimeContext.Types[i]).ToArray();
    }

    [StaticMethod("toString")]
    public static string DirectToString(DyObject value) => value.ToString();

    [StaticMethod("length")]
    public static DyObject GetLength(DyObject value)
    {
        if (value is IMeasurable seq)
            return new DyInteger(seq.Count);
        else if (value is DyClass cls)
            return new DyInteger(cls.Fields.Count);
        else
            return Nil;
    }

    [StaticMethod("referenceEquals")]
    public static bool Equals(DyObject value, DyObject other) => ReferenceEquals(value, other);

    [StaticMethod("clone")]
    public static DyObject Clone(DyObject value) => value.Clone() ?? Nil;

    [StaticMethod("print")]
    public static void Print(ExecutionContext ctx, [VarArg]DyTuple values, [Default(",")]string separator, [Default("\n")]DyObject terminator)
    {
        var fst = true;
        
        foreach (var a in values)
        {
            if (!fst && !string.IsNullOrEmpty(separator))
                Console.Write(separator);

            if (a is DyString s)
                Console.Write(s.Value);
            else
                Console.Write(a.ToString(ctx));

            fst = false;

            if (ctx.Error is not null)
                break;
        }

        if (terminator.TypeId is Dy.String or Dy.Char)
            Console.Write(terminator.ToString());
        else if (terminator.TypeId is not Dy.Nil)
            throw new DyCodeException(DyError.InvalidType, terminator);
    }

    [StaticMethod("setOut")]
    public static void SetOutput(ExecutionContext ctx, DyObject? output = null)
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
    public static string? GetConstructorName(DyObject value) => value is IProduction c ? c.Constructor : null;

    [StaticMethod("typeName")]
    public static string GetTypeName(DyObject value)
    {
        if (value.TypeId is Dy.TypeInfo)
            return ((DyTypeInfo)value).ReflectedTypeName;
        else
            return value.TypeName;
    }

    [StaticMethod("instanceMember")]
    public static DyObject GetInstanceMember(ExecutionContext ctx, DyObject value, string name)
    {
        var member = ctx.RuntimeContext.Types[value.TypeId].LookupInstanceMember(ctx, value, name);

        if (member is not null)
        {
            if (member.Auto)
            {
                member = member.BindToInstance(ctx, value);
                var setter = ctx.RuntimeContext.Types[value.TypeId].LookupInstanceMember(ctx, value, "set_" + name);
                if (setter is not null)
                    return new DyPropertyFunction(name, member, setter.BindToInstance(ctx, value));
                return member;
            }

            return member.BindToInstance(ctx, value);
        }

        return Nil;
    }

    [StaticMethod("staticMember")]
    public static DyObject GetStaticMember(ExecutionContext ctx, DyObject value, string name)
    {
        var typeId = value is DyTypeInfo typ ? typ.ReflectedTypeId : value.TypeId;
        var member = ctx.RuntimeContext.Types[typeId].LookupStaticMember(ctx, name);
        return member ?? Nil;
    }

    [StaticMethod("rawget")]
    public static DyObject RawGet(ExecutionContext ctx, DyObject values, DyObject index)
    {
        if (values is DyClass cls)
            return cls.Fields.GetItem(ctx, index);

        return ctx.RuntimeContext.Types[values.TypeId].RawGet(ctx, values, index);
    }

    [StaticMethod("rawset")]
    public static void RawSet(ExecutionContext ctx, DyObject values, DyObject index, DyObject value)
    {
        if (values is DyClass cls)
            cls.Fields.SetItem(ctx, index, value);
        else
            ctx.RuntimeContext.Types[values.TypeId].RawSet(ctx, values, index, value);
    }

    [StaticMethod("caller")]
    public static DyObject GetCaller(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 2)
        {
            var cp = ctx.CallStack[^2];
            if (!ReferenceEquals(cp, Caller.External))
                return cp.Function;
        }

        return Nil;
    }

    [StaticMethod("current")]
    public static DyObject Current(ExecutionContext ctx)
    {
        if (ctx.CallStack.Count > 1)
            return ctx.CallStack.Peek().Function;

        return Nil;
    }

    [StaticMethod("readLine")]
    public static string Read() => Console.ReadLine() ?? "";

    [StaticMethod("rnd")]
    public static int Randomize(ExecutionContext ctx, int min = 0, int max = int.MaxValue, int? seed = null)
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
    public static void Assert(ExecutionContext ctx, [Default(true)]DyObject expect, DyObject got, string? errorText = null)
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
    public static double Sqrt(double x) => Math.Sqrt(x);

    [StaticMethod("pow")]
    public static double Pow(double x, double y) => Math.Pow(x, y);

    [StaticMethod("min")]
    public static DyObject Min(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Lesser(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("max")]
    public static DyObject Max(ExecutionContext ctx, DyObject x, DyObject y)
    {
        if (x.Greater(y, ctx))
            return x;
        else
            return y;
    }

    [StaticMethod("abs")]
    public static DyObject Abs(ExecutionContext ctx, DyObject value)
    {
        if (value.Lesser(DyInteger.Zero, ctx))
            return value.Negate(ctx);

        return value;
    }

    [StaticMethod("round")]
    public static double Round(double number, int digits = 2) => Math.Round(number, digits);

    [StaticMethod("sign")]
    public static DyObject Sign(ExecutionContext ctx, DyObject x)
    {
        if (ReferenceEquals(x, DyInteger.Zero)) 
            return DyInteger.Zero;

        if (x.Lesser(DyInteger.Zero, ctx))
            return DyInteger.MinusOne;
        
        return DyInteger.One;
    }

    [StaticMethod("parse")]
    public static DyObject Parse(ExecutionContext ctx, string expression)
    {
        var res = DyParser.Parse(SourceBuffer.FromString(expression));

        if (!res.Success)
            return ctx.ParsingFailed(res.Messages
                .Where(m => m.Type == BuildMessageType.Error).First().ToString());

        if (res.Value!.Root is null || res.Value!.Root.Nodes.Count == 0)
            return ctx.ParsingFailed("Empty expression.");
        else if (res.Value!.Root.Nodes.Count > 1)
            return ctx.ParsingFailed("Only single expressions allowed.");

        return LiteralEvaluator.Eval(res.Value!.Root.Nodes[0]);
    }

    [StaticMethod("eval")]
    public static DyObject Eval(string source, DyTuple? args = null)
    {
        var sb = new StringBuilder();
        sb.Append("func __sys_x12(");
        
        if (args is not null)
        {
            var tv = args.UnsafeAccess();
            
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
        sb.Append("__sys_x12");

        var linker = new DyLinker(FileLookup.Default, BuilderOptions.Default());
        var result = linker.Make(SourceBuffer.FromString(sb.ToString()));

        if (!result.Success)
            throw new DyBuildException(result.Messages);

        var newctx = DyMachine.CreateExecutionContext(result.Value!);
        var result2 = DyMachine.Execute(newctx);
        var func = result2.Value!;
        var argsList = new List<DyObject>();

        if (args is null)
            return func.Invoke(newctx, argsList.ToArray());
        
        var tvv = args.UnsafeAccess();

        for (var i = 0; i < args.Count; i++)
        {
            var o = tvv[i];

            if (o is DyLabel lab)
                argsList.Add(lab.Value);
        }

        return func.Invoke(newctx, argsList.ToArray());
    }

    [StaticMethod("__invoke")]
    public static DyObject Invoke(ExecutionContext ctx, DyObject functor, params DyObject[] values) => functor.Invoke(ctx, values);
}
